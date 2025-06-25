using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IT.WebServices.Authorization.Events.Data
{
    public class FileSystemTicketDataProvider : ITicketDataProvider
    {
        private readonly ILogger _logger;
        private readonly DirectoryInfo dataDir;

        public FileSystemTicketDataProvider(
            IOptions<AppSettings> settings,
            ILogger<FileSystemTicketDataProvider> logger
        )
        {
            _logger = logger;

            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("event").CreateSubdirectory("tickets");
        }

        public async Task<bool> Create(EventTicketRecord record)
        {
            Guid.TryParse(record.TicketId, out var ticketId);
            if (ticketId == Guid.Empty)
                return false;

            Guid.TryParse(record.Public.EventId, out var eventId);
            if (eventId == Guid.Empty)
                return false;

            var file = GetDataFilePath(eventId, ticketId);
            if (file.Exists)
                return false;

            await File.WriteAllBytesAsync(file.FullName, record.ToByteArray());
            return true;
        }

        public Task<bool> Delete(Guid ticketId)
        {
            var file = dataDir
                .EnumerateFiles(ticketId.ToString(), SearchOption.AllDirectories)
                .FirstOrDefault();

            if (file == null)
                return Task.FromResult(false);

            file.Delete();
            return Task.FromResult(true);
        }

        public Task<bool> Exists(Guid eventId, Guid ticketId)
        {
            var fd = GetDataFilePath(eventId, ticketId);
            return Task.FromResult(fd.Exists);
        }

        public async IAsyncEnumerable<EventTicketRecord> GetAll()
        {
            foreach (var fd in GetAllDataFiles())
                yield return EventTicketRecord.Parser.ParseFrom(
                    await File.ReadAllBytesAsync(fd.FullName)
                );
        }

        public async IAsyncEnumerable<EventTicketRecord> GetAllByEvent(Guid eventId)
        {
            var eventFolder = new DirectoryInfo(Path.Combine(dataDir.FullName, eventId.ToString()));
            if (!eventFolder.Exists)
                yield break;

            foreach (var file in eventFolder.EnumerateFiles("*"))
            {
                yield return EventTicketRecord.Parser.ParseFrom(
                    await File.ReadAllBytesAsync(file.FullName)
                );
            }
        }

        public async Task<EventTicketRecord> GetById(Guid ticketId)
        {
            var file = dataDir
                .EnumerateFiles(ticketId.ToString(), SearchOption.AllDirectories)
                .FirstOrDefault();

            if (file == null)
                return null;

            return EventTicketRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName));
        }

        public async Task Save(EventTicketRecord record)
        {
            Guid.TryParse(record.TicketId, out var ticketId);
            if (ticketId == Guid.Empty)
                throw new ArgumentException("Invalid TicketId");

            Guid.TryParse(record.Public.EventId, out var eventId);
            if (eventId == Guid.Empty)
                throw new ArgumentException("Invalid EventId");

            var file = GetDataFilePath(eventId, ticketId);
            await File.WriteAllBytesAsync(file.FullName, record.ToByteArray());
        }

        private IEnumerable<FileInfo> GetAllDataFiles()
        {
            return dataDir.EnumerateFiles("*", SearchOption.AllDirectories);
        }

        private FileInfo GetDataFilePath(Guid eventId, Guid ticketId)
        {
            var eventFolder = dataDir.CreateSubdirectory(eventId.ToString());
            return new FileInfo(Path.Combine(eventFolder.FullName, ticketId.ToString()));
        }
    }
}
