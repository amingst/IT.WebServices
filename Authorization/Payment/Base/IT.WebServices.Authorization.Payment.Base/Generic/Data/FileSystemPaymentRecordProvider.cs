using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using Microsoft.Extensions.Options;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    internal class FileSystemPaymentRecordProvider : IGenericPaymentRecordProvider
    {
        private readonly DirectoryInfo dataDir;

        public FileSystemPaymentRecordProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory(PaymentConstants.PAYMENT_DIR_NAME).CreateSubdirectory(PaymentConstants.GENERIC_TYPE).CreateSubdirectory("pay");
        }

        public Task Delete(Guid userId, Guid subId, Guid paymentId)
        {
            var fi = GetDataFilePath(userId, subId, paymentId);
            if (fi.Exists)
                fi.Delete();

            return Task.CompletedTask;
        }

        public Task DeleteAll(Guid userId, Guid subId)
        {
            var di = GetDataDirPath(userId, subId);
            if (di.Exists)
                di.Delete(true);

            return Task.CompletedTask;
        }

        public Task<bool> Exists(Guid userId, Guid subId, Guid paymentId)
        {
            var fi = GetDataFilePath(userId, subId, paymentId);
            return Task.FromResult(fi.Exists);
        }

        public async IAsyncEnumerable<GenericPaymentRecord> GetAll()
        {
            var dir = dataDir;

            foreach (var fi in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var record = await ReadLastOfFile(fi);
                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<GenericPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subId)
        {
            var dir = GetDataDirPath(userId, subId);

            foreach (var fi in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var record = await ReadLastOfFile(fi);
                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<GenericPaymentRecord> GetAllByUserId(Guid userId)
        {
            var dir = GetDataDirPath(userId);

            foreach (var fi in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var record = await ReadLastOfFile(fi);
                if (record != null)
                    yield return record;
            }
        }

        public Task<GenericPaymentRecord?> GetById(Guid userId, Guid subId, Guid paymentId)
        {
            var fi = GetDataFilePath(userId, subId, paymentId);
            return ReadLastOfFile(fi);
        }

        public Task<GenericPaymentRecord?> GetByProcessorId(string processorPaymentId)
        {
            throw new NotImplementedException();
        }

        public async Task Save(GenericPaymentRecord rec)
        {
            var userId = rec.UserID.ToGuid();
            var subId = rec.InternalSubscriptionID.ToGuid();
            var paymentId = rec.InternalPaymentID.ToGuid();
            var fi = GetDataFilePath(userId, subId, paymentId);
            await File.AppendAllTextAsync(fi.FullName, Convert.ToBase64String(rec.ToByteArray()) + "\n");
        }

        private DirectoryInfo GetDataDirPath(Guid userId)
        {
            var userIdStr = userId.ToString();
            var dir = dataDir.CreateSubdirectory(userIdStr.Substring(0, 2)).CreateSubdirectory(userIdStr.Substring(2, 2)).CreateSubdirectory(userIdStr);
            return dir;
        }

        private DirectoryInfo GetDataDirPath(Guid userId, Guid subId)
        {
            var subIdStr = subId.ToString();
            var dir = GetDataDirPath(userId).CreateSubdirectory(subIdStr);
            return dir;
        }

        private FileInfo GetDataFilePath(Guid userId, Guid subId, Guid paymentId)
        {
            var paymentIdStr = paymentId.ToString();
            var dir = GetDataDirPath(userId, subId);
            return new FileInfo(dir.FullName + "/" + paymentIdStr);
        }

        private async IAsyncEnumerable<GenericPaymentRecord> ReadHistoryFromFile(FileInfo fi)
        {
            if (!fi.Exists)
                yield break;

            await foreach (var line in File.ReadLinesAsync(fi.FullName))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                yield return GenericPaymentRecord.Parser.ParseFrom(Convert.FromBase64String(line));
            }
        }

        private async Task<GenericPaymentRecord?> ReadLastOfFile(FileInfo fi)
        {
            if (!fi.Exists)
                return null;

            var last = (await File.ReadAllLinesAsync(fi.FullName)).Where(l => l.Length != 0).LastOrDefault();
            if (last == null)
                return null;

            return GenericPaymentRecord.Parser.ParseFrom(Convert.FromBase64String(last));
        }
    }
}
