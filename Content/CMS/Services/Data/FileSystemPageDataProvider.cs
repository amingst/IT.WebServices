using Google.Protobuf;
using Microsoft.Extensions.Options;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Fragments.Page;
using IT.WebServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IT.WebServices.Helpers;

namespace IT.WebServices.Content.CMS.Services.Data
{
    public class FileSystemPageDataProvider : IPageDataProvider
    {
        private readonly DirectoryInfo pageDir;

        public FileSystemPageDataProvider(IOptions<AppSettings> settings)
        {
            var root = new DirectoryInfo(settings.Value.DataStore);
            root.Create();
            pageDir = root.CreateSubdirectory("cms").CreateSubdirectory("page");
        }

        public Task<bool> Delete(Guid userId)
        {
            var fd = GetPageFilePath(userId);
            var res = fd.Exists;
            fd.Delete();
            return Task.FromResult(res);
        }

        public Task<bool> Exists(Guid userId)
        {
            var fd = GetPageFilePath(userId);
            return Task.FromResult(fd.Exists);
        }

        public async IAsyncEnumerable<PageRecord> GetAll()
        {
            foreach (var file in pageDir.GetFiles())
            {
                yield return PageRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(file.FullName));
            }
        }

        public async Task<PageRecord> GetById(Guid pageId)
        {
            var fd = GetPageFilePath(pageId);
            if (!fd.Exists)
                return null;

            return PageRecord.Parser.ParseFrom(await File.ReadAllBytesAsync(fd.FullName));
        }

        public async Task<PageRecord> GetByURL(string url)
        {
            await foreach(var rec in GetAll())
            {
                if (rec.Public.Data.URL == url)
                    return rec;
            }

            return null;
        }

        public async Task Save(PageRecord page)
        {
            var id = page.Public.PageID.ToGuid();
            var fd = GetPageFilePath(id);
            await File.WriteAllBytesAsync(fd.FullName, page.ToByteArray());
        }

        private FileInfo GetPageFilePath(Guid pageId)
        {
            return pageDir.CreateGuidFileInfo(pageId);
        }
    }
}
