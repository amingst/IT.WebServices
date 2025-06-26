using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Events;
using IT.WebServices.Helpers;
using IT.WebServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Events.Data
{
    public class FileSystemTicketClassDataProvider : ITicketClassDataProvider
    {
        private readonly ILogger _logger;
        private readonly DirectoryInfo dataDir;

        public FileSystemTicketClassDataProvider(
             IOptions<AppSettings> settings,
                ILogger<FileSystemTicketClassDataProvider> logger
            )
        {
            _logger = logger;

            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("event").CreateSubdirectory("ticket_classes");
        }
        public async Task<bool> Create(EventTicketClass ticketClass)
        {
            Guid.TryParse(ticketClass.TicketClassId, out var ticketClassId);
            if (ticketClassId == Guid.Empty)
                return false;

            var file = GetDataFilePath(ticketClassId);
            if (file.Exists)
            {
                return false;
            }

            await Save(ticketClass);
            return true;
        }

        public async IAsyncEnumerable<EventTicketClass> GetAll()
        {
            foreach (var file in dataDir.GetFiles())
            {
                yield return EventTicketClass.Parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName));
            }
        }

        public IAsyncEnumerable<EventTicketClass> GetAllByEvent(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public Task<EventTicketClass> GetById(Guid ticketClassId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Exists(Guid ticketClassId)
        {
            return Task.FromResult(dataDir.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => f.Name == ticketClassId.ToString()).FirstOrDefault().Exists);
        }

        private async Task Save(EventTicketClass ticketClass)
        {
            if (ticketClass == null)
            {
                throw new ArgumentNullException(nameof(ticketClass));
            }

            Guid.TryParse(ticketClass.TicketClassId, out var ticketClassId);
            if (ticketClassId == Guid.Empty) {
                throw new ArgumentException("Invalid TicketClassId");
            }

            var fd = GetDataFilePath(ticketClassId);

            await File.WriteAllBytesAsync(fd.FullName, ticketClass.ToByteArray());
        }

        private FileInfo GetDataFilePath (Guid ticketClassId)
        {
            var ticketClassFolder = dataDir.CreateGuidFileInfo(ticketClassId);
            return new FileInfo(Path.Combine(ticketClassFolder.FullName, ticketClassId.ToString()));
        }
    }
}
