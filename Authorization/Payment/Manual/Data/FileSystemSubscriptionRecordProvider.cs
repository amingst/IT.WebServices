using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Models;
using System.Runtime.CompilerServices;
using IT.WebServices.Fragments.Generic;

namespace IT.WebServices.Authorization.Payment.Manual.Data
{
    public class FileSystemSubscriptionRecordProvider : ISubscriptionRecordProvider
    {
        private readonly DirectoryInfo dataDir;

        public FileSystemSubscriptionRecordProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("payment").CreateSubdirectory("manual");
        }

        public Task Delete(Guid userId, Guid subId)
        {
            var fi = GetDataFilePath(userId, subId);
            if (fi.Exists)
                fi.Delete();

            return Task.CompletedTask;
        }

        public Task<bool> Exists(Guid userId, Guid subId)
        {
            var fi = GetDataFilePath(userId, subId);
            return Task.FromResult(fi.Exists);
        }

        public async IAsyncEnumerable<ManualSubscriptionRecord> GetAll()
        {
            await foreach(var tuple in GetAllSubscriptionIds())
            {
                var record = await GetBySubscriptionId(tuple.userId, tuple.subId);
                if (record != null)
                    yield return record;
            }
        }

#pragma warning disable CS1998
        public async IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds()
#pragma warning restore CS1998
        {
            foreach (var fi in dataDir.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var userId = fi.Directory?.Name.ToGuid() ?? Guid.Empty;
                var subId = fi.Name.ToGuid();

                if (userId == Guid.Empty) continue;
                if (subId == Guid.Empty) continue;

                yield return (userId, subId);
            }
        }

        public async Task<ManualSubscriptionRecord?> GetBySubscriptionId(Guid userId, Guid subId)
        {
            var fi = GetDataFilePath(userId, subId);
            if (!fi.Exists)
                return null;

            var last = (await File.ReadAllLinesAsync(fi.FullName)).Last();

            return ManualSubscriptionRecord.Parser.ParseFrom(Convert.FromBase64String(last));
        }

        public async IAsyncEnumerable<ManualSubscriptionRecord> GetByUserId(Guid userId)
        {
            var dir = GetDataDirPath(userId);

            foreach (var fi in dir.GetFiles())
            {
                var last = (await File.ReadAllLinesAsync(fi.FullName)).Last();
                yield return ManualSubscriptionRecord.Parser.ParseFrom(Convert.FromBase64String(last));
            }
        }

        public async Task Save(ManualSubscriptionRecord rec)
        {
            var userId = Guid.Parse(rec.UserID);
            var subId = Guid.Parse(rec.SubscriptionID);
            var fi = GetDataFilePath(userId, subId);
            await File.AppendAllTextAsync(fi.FullName, Convert.ToBase64String(rec.ToByteArray()) + "\n");
        }

        private DirectoryInfo GetDataDirPath(Guid userId)
        {
            var userIdStr = userId.ToString();
            var dir = dataDir.CreateSubdirectory(userIdStr.Substring(0, 2)).CreateSubdirectory(userIdStr.Substring(2, 2)).CreateSubdirectory(userIdStr);
            return dir;
        }

        private FileInfo GetDataFilePath(Guid userId, Guid subId)
        {
            var userIdStr = userId.ToString();
            var subIdStr = subId.ToString();
            var dir = GetDataDirPath(userId);
            return new FileInfo(dir.FullName + "/" + subIdStr);
        }
    }
}
