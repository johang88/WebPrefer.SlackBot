using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.RequestHandlers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RouteAttribute : Attribute
    {
        public string Method { get; }
        public string Route { get; }

        public RouteAttribute(string method, string route)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Route = route ?? throw new ArgumentNullException(nameof(route));
        }
    }
}
