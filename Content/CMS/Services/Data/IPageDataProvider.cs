using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.CMS.Services.Data
{
    public interface IPageDataProvider
    {
        IAsyncEnumerable<PageRecord> GetAll();
        Task<PageRecord> GetById(Guid PageId);
        Task<PageRecord> GetByURL(string url);
        Task<bool> Delete(Guid pageId);
        Task<bool> Exists(Guid pageId);
        Task Save(PageRecord page);
    }
}
