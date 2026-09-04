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

        public static string CurrentUserName => Resolve().CurrentUserName;

        public static string CurrentUserFullName => Resolve().CurrentUserFullName;

        public static List<string> GetCurrentUserRoles() => Resolve().GetCurrentUserRoles();

        public static List<string> GetUserRoles(string userName) => Resolve().GetUserRoles(userName);

        public static bool IsCurrentUserInRole(string role) => Resolve().IsCurrentUserInRole(role);
    }
}
