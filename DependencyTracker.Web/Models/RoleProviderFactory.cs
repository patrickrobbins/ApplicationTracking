using System;
using System.Configuration;

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

        public static IRoleProvider Create()
        {
            if (IsDevelopment())
                return new DevelopmentRoleProvider();
            return new ActiveDirectoryRoleProvider();
        }
    }
}
