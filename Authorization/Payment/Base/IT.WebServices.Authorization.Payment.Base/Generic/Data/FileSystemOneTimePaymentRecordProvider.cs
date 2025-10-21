﻿using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Models;
using Microsoft.Extensions.Options;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    public class FileSystemOneTimePaymentRecordProvider : IGenericOneTimePaymentRecordProvider
    {
        private readonly DirectoryInfo dataDir;

        public FileSystemOneTimePaymentRecordProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory(PaymentConstants.PAYMENT_DIR_NAME).CreateSubdirectory(PaymentConstants.GENERIC_TYPE).CreateSubdirectory("one");
        }

        public Task Delete(Guid userId, Guid internalPaymentId)
        {
            var fi = GetDataFilePath(userId, internalPaymentId);
            if (fi.Exists)
                fi.Delete();

            return Task.CompletedTask;
        }

        public Task DeleteAll(Guid userId)
        {
            var di = GetDataDirPath(userId);
            if (di.Exists)
                di.Delete(true);

            return Task.CompletedTask;
        }

        public Task<bool> Exists(Guid userId, Guid internalPaymentId)
        {
            var fi = GetDataFilePath(userId, internalPaymentId);
            return Task.FromResult(fi.Exists);
        }

        public async IAsyncEnumerable<GenericOneTimePaymentRecord> GetAll()
        {
            await foreach (var tuple in GetAllSubscriptionIds())
            {
                var record = await GetById(tuple.userId, tuple.subId);
                if (record != null)
                    yield return record;
            }
        }

        public async IAsyncEnumerable<GenericOneTimePaymentRecord> GetAllByUserId(Guid userId)
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

        public Task<GenericOneTimePaymentRecord?> GetById(Guid userId, Guid internalPaymentId)
        {
            var fi = GetDataFilePath(userId, internalPaymentId);
            return ReadLastOfFile(fi);
        }

        public async Task Save(GenericOneTimePaymentRecord rec)
        {
            var userId = rec.UserID.ToGuid();
            var intPayId = rec.InternalPaymentID.ToGuid();
            var fi = GetDataFilePath(userId, intPayId);
            await File.AppendAllTextAsync(fi.FullName, Convert.ToBase64String(rec.ToByteArray()) + "\n");
        }

        private DirectoryInfo GetDataDirPath(Guid userId)
        {
            var userIdStr = userId.ToString();
            var dir = dataDir.CreateSubdirectory(userIdStr.Substring(0, 2)).CreateSubdirectory(userIdStr.Substring(2, 2)).CreateSubdirectory(userIdStr);
            return dir;
        }

        private FileInfo GetDataFilePath(Guid userId, Guid internalPaymentId)
        {
            var userIdStr = userId.ToString();
            var internalPaymentIdStr = internalPaymentId.ToString();
            var dir = GetDataDirPath(userId);
            return new FileInfo(dir.FullName + "/" + internalPaymentIdStr);
        }

        private async IAsyncEnumerable<GenericOneTimePaymentRecord> ReadHistoryFromFile(FileInfo fi)
        {
            if (!fi.Exists)
                yield break;

            await foreach (var line in File.ReadLinesAsync(fi.FullName))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                yield return GenericOneTimePaymentRecord.Parser.ParseFrom(Convert.FromBase64String(line));
            }
        }

        private async Task<GenericOneTimePaymentRecord?> ReadLastOfFile(FileInfo fi)
        {
            if (!fi.Exists)
                return null;

            var last = (await File.ReadAllLinesAsync(fi.FullName)).Where(l => l.Length != 0).LastOrDefault();
            if (last == null)
                return null;

            return GenericOneTimePaymentRecord.Parser.ParseFrom(Convert.FromBase64String(last));
        }
    }
}
