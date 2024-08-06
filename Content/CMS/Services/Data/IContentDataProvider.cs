using IT.WebServices.Fragments.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.CMS.Services.Data
{
    public interface IContentDataProvider
    {
        IAsyncEnumerable<ContentRecord> GetAll();
        Task<ContentRecord> GetById(Guid contentId);
        Task<ContentRecord> GetByURL(string url);
        Task<bool> Delete(Guid contentId);
        Task<bool> Exists(Guid contentId);
        Task Save(ContentRecord content);
    }
}
