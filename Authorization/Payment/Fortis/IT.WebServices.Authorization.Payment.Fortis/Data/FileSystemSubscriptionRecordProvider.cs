using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;

namespace IT.WebServices.Authorization.Payment.Fortis.Data
{
    public class FileSystemSubscriptionRecordProvider : ISubscriptionRecordProvider
    {
        private readonly DirectoryInfo dataDir;

        public FileSystemSubscriptionRecordProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("payment").CreateSubdirectory("pe").CreateSubdirectory("sub");
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

        public async IAsyncEnumerable<FortisSubscriptionRecord> GetAll()
        {
            await foreach (var tuple in GetAllSubscriptionIds())
            {
                var record = await GetById(tuple.userId, tuple.subId);
                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<FortisSubscriptionRecord> GetAllByUserId(Guid userId)
        {
            var dir = GetDataDirPath(userId);

            foreach (var fi in dir.GetFiles())
            {
                var record = await ReadLastOfFile(fi);
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

        public Task<FortisSubscriptionRecord?> GetById(Guid userId, Guid subId)
        {
            var fi = GetDataFilePath(userId, subId);
            return ReadLastOfFile(fi);
        }

        public async Task Save(FortisSubscriptionRecord rec)
        {
            var userId = rec.UserID.ToGuid();
            var subId = rec.SubscriptionID.ToGuid();
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

        private async IAsyncEnumerable<FortisSubscriptionRecord> ReadHistoryFromFile(FileInfo fi)
        {
            if (!fi.Exists)
                yield break;

            await foreach(var line in File.ReadLinesAsync(fi.FullName))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                yield return FortisSubscriptionRecord.Parser.ParseFrom(Convert.FromBase64String(line));
            }
        }

        private async Task<FortisSubscriptionRecord?> ReadLastOfFile(FileInfo fi)
        {
            if (!fi.Exists)
                return null;

            var last = (await File.ReadAllLinesAsync(fi.FullName)).Where(l => l.Length != 0).LastOrDefault();
            if (last == null)
                return null;

            return FortisSubscriptionRecord.Parser.ParseFrom(Convert.FromBase64String(last));
        }
    }
}
