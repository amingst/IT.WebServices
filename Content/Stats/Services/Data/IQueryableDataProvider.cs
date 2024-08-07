using IT.WebServices.Fragments.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public interface IQueryableDataProvider
    {
        IAsyncEnumerable<Guid> GetAllForContent(Guid contentId);
        IAsyncEnumerable<Guid> GetAllForUser(Guid userId);
        Task<DateTime> GetLastChangeForContent(Guid contentId);
        Task<DateTime> GetLastChangeForUser(Guid userId);
    }
}
