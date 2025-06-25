using System;
using System.Collections.Concurrent;
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
    public class FileSystemEventInstanceOverrideDataProvider : IEventInstanceOverrideDataProvider
    {
        private readonly ILogger _logger;
        private readonly DirectoryInfo dataDir;
        private readonly ConcurrentDictionary<Guid, HashSet<string>> instanceIdIndex = new();

        public FileSystemEventInstanceOverrideDataProvider(
            IOptions<AppSettings> settings,
            ILogger<FileSystemEventInstanceOverrideDataProvider> logger
        )
        {
            _logger = logger;
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            dataDir = root.CreateSubdirectory("event").CreateSubdirectory("instance_overrides");
        }

        public async Task<bool> Create(EventInstanceOverride record)
        {
            if (!Guid.TryParse(record.ParentEventId, out var parentEventId))
                return false;

            var instanceId = record.InstanceId;
            if (string.IsNullOrWhiteSpace(instanceId))
                return false;

            var file = GetDataFilePath(parentEventId, instanceId);

            var folder = file.Directory;
            folder.Create();
            await File.WriteAllBytesAsync(file.FullName, record.ToByteArray());

            instanceIdIndex.AddOrUpdate(
                parentEventId,
                new HashSet<string> { instanceId },
                (_, set) =>
                {
                    lock (set)
                    {
                        set.Add(instanceId);
                    }
                    return set;
                }
            );

            return true;
        }

        public IAsyncEnumerable<EventInstanceOverride> GetByEventId(Guid eventId)
        {
            return InternalGetByEventId(eventId);
        }

        private async IAsyncEnumerable<EventInstanceOverride> InternalGetByEventId(Guid eventId)
        {
            var folder = new DirectoryInfo(Path.Combine(dataDir.FullName, eventId.ToString()));
            if (!folder.Exists)
                yield break;

            foreach (var file in folder.EnumerateFiles("*"))
            {
                yield return EventInstanceOverride.Parser.ParseFrom(
                    await File.ReadAllBytesAsync(file.FullName)
                );
            }
        }

        public async Task<EventInstanceOverride> GetById(string id)
        {
            foreach (var folder in dataDir.EnumerateDirectories())
            {
                var file = folder.GetFiles(id).FirstOrDefault();
                if (file != null)
                    return EventInstanceOverride.Parser.ParseFrom(
                        await File.ReadAllBytesAsync(file.FullName)
                    );
            }
            return null;
        }

        public async Task Save(EventInstanceOverride record)
        {
            if (!Guid.TryParse(record.ParentEventId, out var parentEventId))
                throw new ArgumentException("Invalid ParentEventId");

            var instanceId = record.InstanceId;
            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException("Invalid InstanceId");

            var file = GetDataFilePath(parentEventId, instanceId);
            await File.WriteAllBytesAsync(file.FullName, record.ToByteArray());

            instanceIdIndex.AddOrUpdate(
                parentEventId,
                new HashSet<string> { instanceId },
                (_, set) =>
                {
                    lock (set)
                    {
                        set.Add(instanceId);
                    }
                    return set;
                }
            );
        }

        public Task<bool> HasOverride(Guid eventId, string instanceId)
        {
            return Task.FromResult(
                instanceIdIndex.TryGetValue(eventId, out var set) && set.Contains(instanceId)
            );
        }

        private FileInfo GetDataFilePath(Guid eventId, string instanceId)
        {
            var folder = dataDir.CreateSubdirectory(eventId.ToString());
            return new FileInfo(Path.Combine(folder.FullName, instanceId));
        }
    }
}
