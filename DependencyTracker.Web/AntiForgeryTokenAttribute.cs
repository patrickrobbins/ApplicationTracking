using System;
using System.Web.Mvc;
using DependencyTracker.Web.Logging;

namespace DependencyTracker.Web
{
    /// <summary>
    /// Anti-forgery validation that rejects a tampered request with
    /// 400 Bad Request instead of letting the validation failure surface as an
    /// unhandled 500. The validation itself is delegated to the framework
    /// attribute so the semantics stay identical; only the failure handling
    /// differs. Rejected requests are logged because they are either a stale
    /// page or an attempt to forge a request.
    /// </summary>
    public class AntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        private static readonly ValidateAntiForgeryTokenAttribute Inner = new ValidateAntiForgeryTokenAttribute();

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            try
            {
                Inner.OnAuthorization(filterContext);
            }
            catch (Exception ex)
            {
                if (filterContext.Result != null)
                {
                    throw;
                }

                var path = filterContext.HttpContext != null
                    && filterContext.HttpContext.Request != null
                    && filterContext.HttpContext.Request.Url != null
                    ? filterContext.HttpContext.Request.Url.AbsolutePath
                    : null;

                AppLogger.LogError(ex, "Anti-forgery validation failed for " + path);

                filterContext.Result = new HttpStatusCodeResult(
                    400,
                    "The request could not be validated. Please reload the page and try again.");
            }
        }
    }
}
