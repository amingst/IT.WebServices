using IT.WebServices.Fragments.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public interface ILikeDataProvider : IQueryableDataProvider
    {
        Task Like(Guid userId, Guid contentId);
        Task Unlike(Guid userId, Guid contentId);
    }
}
