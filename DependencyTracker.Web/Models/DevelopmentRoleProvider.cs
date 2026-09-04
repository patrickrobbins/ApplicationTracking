using System.Collections.Generic;

namespace DependencyTracker.Web.Models
{
    /// <summary>
    /// Development-only role provider that grants every authenticated user full
    /// access (Admin, Maintenance, Viewer). Use only when the
    /// "RoleProvider" appSetting is set to "Development".
    /// </summary>
    public class DevelopmentRoleProvider : RoleProviderBase
    {
        private static readonly List<string> AllRoles =
            new List<string> { RoleProviderBase.RoleAdmin, RoleProviderBase.RoleMaintenance, RoleProviderBase.RoleViewer };

        public override List<string> GetCurrentUserRoles()
        {
            return new List<string>(AllRoles);
        }

        public override List<string> GetUserRoles(string userName)
        {
            return new List<string>(AllRoles);
        }

        public override bool IsCurrentUserInRole(string role)
        {
            return AllRoles.Contains(role);
        }
    }
}
