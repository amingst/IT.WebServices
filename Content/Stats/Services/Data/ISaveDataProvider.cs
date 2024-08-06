using IT.WebServices.Fragments.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public interface ISaveDataProvider
    {
        Task Save(Guid userId, Guid contentId);
        Task Unsave(Guid userId, Guid contentId);
    }
}
