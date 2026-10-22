using System;
using System.Configuration;
using System.Web;

namespace DependencyTracker.Web.Models
{
    /// <summary>
    /// Selects the active <see cref="IRoleProvider"/> based on the
    /// "RoleProvider" appSetting ("Development" or "ActiveDirectory").
    /// Development grants full access and is intended for local development;
    /// ActiveDirectory is the production provider.
    /// </summary>
    public static class RoleProviderFactory
    {
        public static bool IsDevelopment()
        {
            var configured = ConfigurationManager.AppSettings["RoleProvider"];
            return string.Equals(configured, "Development", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// True when the current request may be served by the development role
        /// provider. The development provider grants every role to every
        /// caller, so it is only honoured for requests that originate from the
        /// machine hosting the application; remote requests are denied.
        /// </summary>
        public static bool IsDevelopmentRequestAllowed(HttpContextBase httpContext)
        {
            if (!IsDevelopment())
                return false;

            if (httpContext == null || httpContext.Request == null)
                return false;

            try
            {
                return httpContext.Request.IsLocal;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loopback check against the current request, for code paths that do
        /// not have an <see cref="HttpContextBase"/> (static helpers, views).
        /// </summary>
        public static bool IsCurrentRequestLocal()
        {
            var context = HttpContext.Current;
            if (context == null || context.Request == null)
                return false;

            try
            {
                return context.Request.IsLocal;
            }
            catch
            {
                return false;
            }
        }

        public static IRoleProvider Create()
        {
            if (IsDevelopment())
                return new DevelopmentRoleProvider();
            return new ActiveDirectoryRoleProvider();
        }
    }
}
