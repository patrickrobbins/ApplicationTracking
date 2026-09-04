using System;
using System.Collections.Generic;
using System.Web;

namespace DependencyTracker.Web.Models
{
    /// <summary>
    /// Base implementation for role providers: resolves the current Windows
    /// user's account names and provides a default Viewer role.
    /// </summary>
    public abstract class RoleProviderBase : IRoleProvider
    {
        public const string RoleViewer = "Viewer";
        public const string RoleMaintenance = "Maintenance";
        public const string RoleAdmin = "Admin";

        /// <summary>
        /// Current user's short account name, e.g. "jsmith" from "DOMAIN\jsmith".
        /// </summary>
        public virtual string CurrentUserName
        {
            get
            {
                if (HttpContext.Current?.User?.Identity != null)
                {
                    var name = HttpContext.Current.User.Identity.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        var idx = name.IndexOf('\\');
                        return idx >= 0 ? name.Substring(idx + 1) : name;
                    }
                }
                return Environment.UserName;
            }
        }

        /// <summary>
        /// Fully qualified account name, e.g. "DOMAIN\jsmith".
        /// </summary>
        public virtual string CurrentUserFullName
        {
            get
            {
                if (HttpContext.Current?.User?.Identity != null &&
                    !string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                {
                    return HttpContext.Current.User.Identity.Name;
                }
                return Environment.UserName;
            }
        }

        public abstract List<string> GetCurrentUserRoles();

        public abstract List<string> GetUserRoles(string userName);

        public abstract bool IsCurrentUserInRole(string role);

        protected static List<string> DefaultRoles()
        {
            return new List<string> { RoleViewer };
        }
    }
}
