using IT.WebServices.Fragments.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats.Services.Data
{
    public interface IQueryableTimestampDataProvider
    {
        IAsyncEnumerable<Data> GetAllCountsForContent(Guid contentId);
        IAsyncEnumerable<Data> GetAllCountsForUser(Guid userId);

        public struct Data
        {
            public Guid Id;
            public long Count;

            public Data(Guid id, long count)
            {
                Id = id;
                Count = count;
            }
        }
    }
}
