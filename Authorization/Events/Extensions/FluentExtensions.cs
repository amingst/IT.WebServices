using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Events.Extensions
{
    public static class FluentExtensions
    {
        public static T Tap<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }
}
