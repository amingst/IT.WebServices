using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IT.WebServices.Content.Stats.Services.Subscriptions;
using IT.WebServices.Helpers;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public class FileSystemProgressDataProvider : IProgressDataProvider
    {
        private readonly SubscriptionList subList;

        private readonly DirectoryInfo dataDir;
        private readonly MessageParser<UserProgressRecord> parser;

        public FileSystemProgressDataProvider(IOptions<AppSettings> settings, SubscriptionList subList)
        {
            this.subList = subList;

            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("stats").CreateSubdirectory("users");

            var pi = typeof(UserProgressRecord).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
            parser = pi.GetValue(null) as MessageParser<UserProgressRecord>;
        }

        public async Task<bool> Delete(Guid userId, Guid contentId)
        {
            var fd = GetFilePath(userId, contentId);
            var res = fd.Exists;
            fd.Delete();

            await subList.UserChanges.Writer.WriteAsync(userId);

            return res;
        }

        public Task<bool> Exists(Guid userId, Guid contentId)
        {
            var fd = GetFilePath(userId, contentId);
            return Task.FromResult(fd.Exists);
        }

        public async IAsyncEnumerable<UserProgressRecord> GetAll()
        {
            foreach (var file in dataDir.GetFiles("*", SearchOption.AllDirectories))
            {
                yield return parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName));
            }
        }

        public async IAsyncEnumerable<UserProgressRecord> GetAllForUser(Guid userId)
        {
            foreach (var file in GetUserDir(userId).GetFiles("*", SearchOption.AllDirectories))
            {
                yield return parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName));
            }
        }

        public async Task<UserProgressRecord> Get(Guid userId, Guid contentId)
        {
            var fd = GetFilePath(userId, contentId);
            if (!fd.Exists)
                return default;

            return parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
        }

        public async Task LogProgress(Guid userId, Guid contentId, float progress)
        {
            var fd = GetFilePath(userId, contentId);
            var record = new UserProgressRecord()
            {
                ContentID = contentId.ToString(),
                Progress = progress,
                UpdatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
            };

            await File.WriteAllBytesAsync(fd.FullName, record.ToByteArray());

            await subList.UserChanges.Writer.WriteAsync(userId);
        }

        private DirectoryInfo GetUserDir(Guid userId)
        {
            var dir = dataDir;
            dir = dir.CreateGuidDirectory(userId);
            dir = dir.CreateSubdirectory("progress");

            return dir;
        }

        private FileInfo GetFilePath(Guid userId, Guid contentId)
        {
            var dir = GetUserDir(userId);

            return dir.CreateGuidFileInfo(contentId);
        }
    }
}
