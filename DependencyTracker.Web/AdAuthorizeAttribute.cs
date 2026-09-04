using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DependencyTracker.Web.Models;

namespace DependencyTracker.Web
{
    /// <summary>
    /// Authorizes the current Windows user against roles resolved from
    /// Active Directory group membership.
    /// </summary>
    public class AdAuthorizeAttribute : AuthorizeAttribute
    {
        private string[] _requiredRoles;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                return false;

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                if (RoleProviderFactory.IsDevelopment())
                    return true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(Roles))
                return true;

            if (_requiredRoles == null)
            {
                _requiredRoles = Roles.Split(',')
                    .Select(r => r.Trim())
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToArray();
            }

            if (_requiredRoles.Length == 0)
                return true;

            var roleProvider = DependencyResolver.Current.GetService<IRoleProvider>();
            var userRoles = roleProvider != null
                ? roleProvider.GetCurrentUserRoles()
                : AdRoleManager.GetCurrentUserRoles();
            return _requiredRoles.Any(r => userRoles.Contains(r));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var isAjax = filterContext.HttpContext.Request.IsAjaxRequest();
            if (isAjax)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "You are not authorized to perform this action." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.HttpContext.Response.StatusCode = 403;
                return;
            }

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new ViewResult { ViewName = "AccessDenied" };
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
