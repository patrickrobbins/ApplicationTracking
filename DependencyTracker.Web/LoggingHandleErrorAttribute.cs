using System;
using System.Web.Mvc;
using DependencyTracker.Web.Logging;

namespace DependencyTracker.Web
{
    /// <summary>
    /// <see cref="HandleErrorAttribute"/> that writes the exception to the
    /// error log before the error view is rendered. MVC handled errors never
    /// reach <c>Application_Error</c>, so without this they leave no trace.
    /// </summary>
    public class LoggingHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && !filterContext.ExceptionHandled && filterContext.Exception != null)
            {
                var requestId = filterContext.HttpContext != null && filterContext.HttpContext.Items != null
                    ? filterContext.HttpContext.Items["ErrorRequestId"] as string
                    : null;

                var path = filterContext.HttpContext != null && filterContext.HttpContext.Request != null
                    && filterContext.HttpContext.Request.Url != null
                    ? filterContext.HttpContext.Request.Url.AbsolutePath
                    : null;

                AppLogger.LogError(filterContext.Exception, "Request " + (requestId ?? "unknown") + " " + path);
            }

            base.OnException(filterContext);
        }
    }
}
