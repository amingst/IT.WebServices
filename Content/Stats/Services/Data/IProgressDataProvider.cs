using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Content.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public interface IProgressDataProvider
    {
        Task<bool> Delete(Guid userId, Guid contentId);
        Task<bool> Exists(Guid userId, Guid contentId);
        Task<UserProgressRecord> Get(Guid userId, Guid contentId);
        IAsyncEnumerable<UserProgressRecord> GetAllForUser(Guid userId);
        Task LogProgress(Guid userId, Guid contentId, float progress);
    }
}
