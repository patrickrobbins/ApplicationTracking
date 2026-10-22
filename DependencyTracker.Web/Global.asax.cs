using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DependencyTracker.Web.App_Start;
using DependencyTracker.Web.Logging;

namespace DependencyTracker.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            AutofacConfig.RegisterDependencies();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Correlation id for every request so an error shown to a user can
            // be matched to the entry written to the error log.
            HttpContext.Current.Items["ErrorRequestId"] = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception == null)
                return;

            var requestId = HttpContext.Current != null && HttpContext.Current.Items["ErrorRequestId"] != null
                ? HttpContext.Current.Items["ErrorRequestId"].ToString()
                : "unknown";

            // The path is logged rather than the full URI so query string
            // values (which can carry tokens or search terms) stay out of the log.
            var path = Request.Url != null ? Request.Url.AbsolutePath : null;
            AppLogger.LogError(exception, "Request " + requestId + " " + path);
        }
    }
}
