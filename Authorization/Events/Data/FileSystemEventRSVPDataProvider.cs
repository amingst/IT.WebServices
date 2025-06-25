using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IT.WebServices.Authorization.Events.Data
{
    public class FileSystemEventRSVPDataProvider : IRSVPDataProvider
    {
        private readonly ILogger _logger;
        private readonly DirectoryInfo dataDir;

        public FileSystemEventRSVPDataProvider(
            IOptions<AppSettings> settings,
            ILogger<FileSystemEventRSVPDataProvider> logger
        )
        {
            _logger = logger;
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("event").CreateSubdirectory("reservations");
        }

        public async Task<bool> Create(EventRSVPRecord record)
        {
            Guid.TryParse(record.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
                return false;

            Guid.TryParse(record.EventRSVPId, out var rsvpId);
            if (rsvpId == Guid.Empty)
                return false;

            var fd = GetDataFilePath(eventGuid, rsvpId);
            if (fd.Exists)
                return false;

            await File.WriteAllBytesAsync(fd.FullName, record.ToByteArray());
            return true;
        }

        public async Task<bool> Cancel(Guid eventRsvpGuid, string reason)
        {
            var fd = dataDir
                .EnumerateFiles(eventRsvpGuid.ToString(), SearchOption.AllDirectories)
                .FirstOrDefault();

            if (fd == null)
                return false;

            fd.Delete();
            return true;
        }

        public async IAsyncEnumerable<EventRSVPRecord> GetAllByEvent(Guid eventGuid)
        {
            var folder = new DirectoryInfo(Path.Combine(dataDir.FullName, eventGuid.ToString()));
            if (!folder.Exists)
                yield break;

            foreach (var file in folder.EnumerateFiles("*"))
            {
                yield return EventRSVPRecord.Parser.ParseFrom(
                    await File.ReadAllBytesAsync(file.FullName)
                );
            }
        }

        public async IAsyncEnumerable<EventRSVPRecord> GetAllByUser(Guid userGuid)
        {
            foreach (var file in GetAllDataFiles())
            {
                var record = EventRSVPRecord.Parser.ParseFrom(
                    await File.ReadAllBytesAsync(file.FullName)
                );
                if (Guid.TryParse(record.UserId, out var parsedUserId) && parsedUserId == userGuid)
                {
                    yield return record;
                }
            }
        }

        public async Task<EventRSVPRecord> GetById(Guid id)
        {
            var fd = dataDir
                .EnumerateFiles(id.ToString(), SearchOption.AllDirectories)
                .FirstOrDefault();

            if (fd == null)
                return null;

            return EventRSVPRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
        }

        public async Task Save(EventRSVPRecord record)
        {
            Guid.TryParse(record.EventId, out var eventGuid);
            if (eventGuid == Guid.Empty)
                throw new ArgumentException("Invalid EventId");

            Guid.TryParse(record.EventRSVPId, out var rsvpId);
            if (rsvpId == Guid.Empty)
                throw new ArgumentException("Invalid RSVP Id");

            var fd = GetDataFilePath(eventGuid, rsvpId);
            await File.WriteAllBytesAsync(fd.FullName, record.ToByteArray());
        }

        private IEnumerable<FileInfo> GetAllDataFiles()
        {
            return dataDir.EnumerateFiles("*", SearchOption.AllDirectories);
        }

        private FileInfo GetDataFilePath(Guid eventId, Guid rsvpId)
        {
            var eventFolder = dataDir.CreateSubdirectory(eventId.ToString());
            return new FileInfo(Path.Combine(eventFolder.FullName, rsvpId.ToString()));
        }
    }
}
