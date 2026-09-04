using System.Web.Http;
using System.Web.Routing;

namespace DependencyTracker.Web
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }
    }
}
