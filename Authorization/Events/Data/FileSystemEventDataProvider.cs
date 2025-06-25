using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Helpers;
using IT.WebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IT.WebServices.Authorization.Events.Data
{
    public class FileSystemEventDataProvider : IEventDataProvider
    {
        private readonly ILogger _logger;
        private readonly DirectoryInfo dataDir;

        public FileSystemEventDataProvider(
            IOptions<AppSettings> settings,
            ILogger<FileSystemEventDataProvider> logger
        )
        {
            _logger = logger;
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("event").CreateSubdirectory("events");
        }

        public async Task<bool> Create(EventRecord eventRecord)
        {
            var id = eventRecord.EventIDGuid;
            var fd = GetDataFilePath(id);
            if (fd.Exists)
                return false;

            await File.WriteAllBytesAsync(fd.FullName, eventRecord.ToByteArray());
            return true;
        }

        public async Task<bool> Delete(Guid eventId)
        {
            var fd = GetDataFilePath(eventId);
            if (!fd.Exists)
                return false;

            var rec = EventRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
            fd.Delete();
            return true;
        }

        public Task<bool> Exists(Guid eventId)
        {
            var fd = GetDataFilePath(eventId);
            return Task.FromResult(fd.Exists);
        }

        public async IAsyncEnumerable<EventRecord> GetAll()
        {
            foreach (var fd in GetAllDataFiles())
            {
                EventRecord record = null;
                try
                {
                    var bytes = await File.ReadAllBytesAsync(fd.FullName);
                    record = EventRecord.Parser.ParseFrom(bytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to parse EventRecord from file {FileName}",
                        fd.FullName
                    );
                    continue; // skip this corrupted or incompatible file
                }

                if (record != null)
                    yield return record;
            }
        }

        public async Task<EventRecord> GetById(Guid eventId)
        {
            var fd = GetDataFilePath(eventId);
            if (!fd.Exists)
                return null;

            return EventRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
        }

        public async Task Save(EventRecord record)
        {
            var id = record.EventIDGuid;
            var fd = GetDataFilePath(id);
            await File.WriteAllBytesAsync(fd.FullName, record.ToByteArray());
        }

        private IEnumerable<FileInfo> GetAllDataFiles()
        {
            return dataDir.EnumerateFiles("*", SearchOption.AllDirectories);
        }

        private FileInfo GetDataFilePath(Guid eventId)
        {
            return dataDir.CreateGuidFileInfo(eventId);
        }
    }
}
