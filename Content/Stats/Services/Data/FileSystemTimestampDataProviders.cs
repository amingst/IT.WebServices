using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Content.Stats.Services.Subscriptions;
using System.Runtime.CompilerServices;
using IT.WebServices.Helpers;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public class FileSystemShareDataProvider : GenericFileSystemTimestampDataProvider, IShareDataProvider
    {
        public FileSystemShareDataProvider(IOptions<AppSettings> settings, SubscriptionList subList) : base("shares", settings, subList) { }

        public async Task LogShare(Guid userId, Guid contentId)
        {
            await AppendTimestamp(userId, contentId);
            await subList.ContentChanges.Writer.WriteAsync(contentId);
            await subList.UserChanges.Writer.WriteAsync(userId);
        }
    }

    public class FileSystemViewDataProvider : GenericFileSystemTimestampDataProvider, IViewDataProvider
    {
        public FileSystemViewDataProvider(IOptions<AppSettings> settings, SubscriptionList subList) : base("views", settings, subList) { }

        public async Task LogView(Guid userId, Guid contentId)
        {
            await AppendTimestamp(userId, contentId);
            await subList.ContentChanges.Writer.WriteAsync(contentId);
            await subList.UserChanges.Writer.WriteAsync(userId);
        }
    }

    public class GenericFileSystemTimestampDataProvider
    {
        protected readonly SubscriptionList subList;

        private readonly DirectoryInfo contentDataDir;
        private readonly DirectoryInfo userDataDir;
        private readonly string subDirName;

        public const int SIZE_OF_TIMESTAMP = 8;

        public GenericFileSystemTimestampDataProvider(string subDirName, IOptions<AppSettings> settings, SubscriptionList subList)
        {
            this.subList = subList;

            this.subDirName = subDirName;

            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            contentDataDir = root.CreateSubdirectory("stats").CreateSubdirectory("content");
            userDataDir = root.CreateSubdirectory("stats").CreateSubdirectory("users");
        }

        public async Task AppendTimestamp(Guid userId, Guid contentId)
        {
            var fContent = GetContentFilePath(contentId, userId);
            var fUser = GetUserFilePath(userId, contentId);

            await AppendTimestamp(fContent);
            await AppendTimestamp(fUser);

            //await Task.WhenAll(
            //        AppendTimestamp(fContent),
            //        AppendTimestamp(fUser)
            //    );
        }

        private async Task AppendTimestamp(FileInfo fi)
        {
            using var stream = FileStreamHelper.WaitForFile(fi.FullName, FileMode.Append, FileAccess.Write, FileShare.None);
            if (stream == null)
                return;

            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            var unixTime = dto.ToUnixTimeSeconds();
            byte[] bytes = BitConverter.GetBytes(unixTime);

            await stream.WriteAsync(bytes, 0, SIZE_OF_TIMESTAMP);
            await stream.FlushAsync();
        }

        public async IAsyncEnumerable<IQueryableTimestampDataProvider.Data> GetAllCountsForContent(Guid contentId)
        {
            foreach (var file in GetContentDir(contentId).GetFiles("*", SearchOption.AllDirectories))
                yield return await Task.FromResult(new IQueryableTimestampDataProvider.Data(file.Name.ToGuid(), file.Length / SIZE_OF_TIMESTAMP));
        }

        public async IAsyncEnumerable<IQueryableTimestampDataProvider.Data> GetAllCountsForUser(Guid userId)
        {
            foreach (var file in GetUserDir(userId).GetFiles("*", SearchOption.AllDirectories))
                yield return await Task.FromResult(new IQueryableTimestampDataProvider.Data(file.Name.ToGuid(), file.Length / SIZE_OF_TIMESTAMP));
        }

        private DirectoryInfo GetContentDir(Guid contentId)
        {
            return GetDir(contentDataDir, contentId);
        }

        private DirectoryInfo GetUserDir(Guid userId)
        {
            return GetDir(userDataDir, userId);
        }

        private DirectoryInfo GetDir(DirectoryInfo parentDir, Guid id)
        {
            var dir = parentDir;
            var name = id.ToString();
            dir = dir.CreateSubdirectory(name.Substring(0, 2)).CreateSubdirectory(name.Substring(2, 2)).CreateSubdirectory(name);
            dir = dir.CreateSubdirectory(subDirName);

            return dir;
        }

        private FileInfo GetContentFilePath(Guid contentId, Guid userId)
        {
            var dir = GetContentDir(contentId);
            var name = userId.ToString();
            dir = dir.CreateSubdirectory(name.Substring(0, 2)).CreateSubdirectory(name.Substring(2, 2));

            return new FileInfo(dir.FullName + "/" + name);
        }

        private FileInfo GetUserFilePath(Guid userId, Guid contentId)
        {
            var dir = GetUserDir(userId);
            var name = contentId.ToString();
            dir = dir.CreateSubdirectory(name.Substring(0, 2)).CreateSubdirectory(name.Substring(2, 2));

            return new FileInfo(dir.FullName + "/" + name);
        }
    }
}
