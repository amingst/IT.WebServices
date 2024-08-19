using IT.WebServices.Authentication;
using IT.WebServices.Fragments.Content.Stats;
using IT.WebServices.Fragments.Settings;
using System;
using System.Threading.Tasks;

namespace IT.WebServices.Content.Stats
{
    public interface IViewService
    {
        Task<LogViewContentResponse> LogViewContentInternal(LogViewContentRequest request, ONUser userToken);
    }
}
