using System.Collections.Generic;
using System.Web.Mvc;

namespace DependencyTracker.Web.Models
{
    /// <summary>
    /// Static facade over the configured <see cref="IRoleProvider"/>. Kept for
    /// use from Razor views and as a fallback for authorization attributes so
    /// they do not need to resolve the provider directly.
    /// </summary>
    public static class AdRoleManager
    {
        public const string RoleAdmin = "Admin";
        public const string RoleMaintenance = "Maintenance";
        public const string RoleViewer = "Viewer";

        private static IRoleProvider Resolve()
        {
            var provider = DependencyResolver.Current?.GetService<IRoleProvider>();
            return provider ?? RoleProviderFactory.Create();
        }

        /// <summary>
        /// False when the development role provider is configured but the
        /// request did not come from the machine hosting the application. The
        /// development provider grants every role, so it must not be honoured
        /// for remote callers.
        /// </summary>
        private static bool RolesAllowed()
        {
            if (!RoleProviderFactory.IsDevelopment())
                return true;

            return RoleProviderFactory.IsCurrentRequestLocal();
        }

        public static string CurrentUserName => Resolve().CurrentUserName;

        public static string CurrentUserFullName => Resolve().CurrentUserFullName;

        public static List<string> GetCurrentUserRoles() =>
            RolesAllowed() ? Resolve().GetCurrentUserRoles() : new List<string>();

        public static List<string> GetUserRoles(string userName) =>
            RolesAllowed() ? Resolve().GetUserRoles(userName) : new List<string>();

        public static bool IsCurrentUserInRole(string role) =>
            RolesAllowed() && Resolve().IsCurrentUserInRole(role);
    }
}
