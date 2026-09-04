using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Security;
using DependencyTracker.Data.Repositories;

namespace DependencyTracker.Web.Models
{
    /// <summary>
    /// Resolves application roles by checking membership in the AD groups
    /// configured in the ADGroups table. Falls back to the local Windows
    /// security database when no domain controller is reachable so the app
    /// remains usable on a developer workstation.
    /// </summary>
    public class ActiveDirectoryRoleProvider : RoleProviderBase
    {
        private readonly IAdminRepository _adminRepository;

        public ActiveDirectoryRoleProvider() : this(new AdminRepository())
        {
        }

        public ActiveDirectoryRoleProvider(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        /// <summary>
        /// Returns the roles for the current user based on AD group membership.
        /// Viewer is implied for every authenticated user.
        /// </summary>
        public override List<string> GetCurrentUserRoles()
        {
            return GetUserRoles(CurrentUserName);
        }

        /// <summary>
        /// Returns the roles for the given Windows account based on configured
        /// AD group membership.
        /// </summary>
        public override List<string> GetUserRoles(string userName)
        {
            var roles = DefaultRoles();

            if (string.IsNullOrEmpty(userName))
                return roles;

            var adGroups = ResolveUserGroups(userName);

            if (adGroups.Count == 0)
                return roles;

            var configured = _adminRepository.GetActiveGroups().ToList();
            foreach (var group in configured)
            {
                var normalized = Normalize(group.GroupName);
                if (adGroups.Any(g => string.Equals(Normalize(g), normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    roles.Add(group.Role);
                }
            }

            return roles.Distinct().ToList();
        }

        public override bool IsCurrentUserInRole(string role)
        {
            return GetCurrentUserRoles().Contains(role);
        }

        /// <summary>
        /// Combines the user's AD group memberships (via LDAP) with the
        /// WindowsPrincipal groups (works with the local security database when
        /// no domain is available).
        /// </summary>
        private List<string> ResolveUserGroups(string userName)
        {
            var groups = new List<string>();

            try
            {
                groups.AddRange(GetUserAdGroups(userName));
            }
            catch
            {
                // AD unavailable (e.g. no domain controller reachable).
            }

            try
            {
                var principal = HttpContext.Current?.User as System.Security.Principal.WindowsPrincipal;
                if (principal != null && principal.Identity is System.Security.Principal.WindowsIdentity)
                {
                    var identity = (System.Security.Principal.WindowsIdentity)principal.Identity;
                    foreach (var group in identity.Groups ?? Enumerable.Empty<System.Security.Principal.IdentityReference>())
                    {
                        try
                        {
                            groups.Add(group.Translate(typeof(System.Security.Principal.NTAccount)).Value);
                        }
                        catch
                        {
                            // ignore unresolved SIDs
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return groups;
        }

        /// <summary>
        /// Queries Active Directory for the direct group memberships of a user.
        /// </summary>
        private List<string> GetUserAdGroups(string userName)
        {
            var groups = new List<string>();
            var domain = GetDomainFromConfig() ?? "LDAP://" + GetDefaultDomain();

            using (var entry = new DirectoryEntry(domain))
            {
                using (var searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(&(objectClass=user)(sAMAccountName={EscapeLdapFilter(userName)}))";
                    searcher.PropertiesToLoad.Add("memberOf");
                    searcher.PropertiesToLoad.Add("distinguishedName");
                    searcher.PropertiesToLoad.Add("displayName");

                    var result = searcher.FindOne();
                    if (result != null)
                    {
                        foreach (var prop in result.Properties["memberOf"])
                        {
                            var dn = prop.ToString();
                            groups.Add(ExtractGroupName(dn));
                        }
                    }
                }
            }

            return groups;
        }

        private string ExtractGroupName(string distinguishedName)
        {
            // CN=GroupName,OU=...,DC=...
            var parts = distinguishedName.Split(',');
            foreach (var part in parts)
            {
                if (part.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    var groupName = part.Substring(3);
                    var domain = GetDomainFromConfig();
                    if (!string.IsNullOrEmpty(domain))
                        return domain + "\\" + groupName;
                    return groupName;
                }
            }
            return distinguishedName;
        }

        private string EscapeLdapFilter(string value)
        {
            return value.Replace("\\", "\\5c")
                        .Replace("*", "\\2a")
                        .Replace("(", "\\28")
                        .Replace(")", "\\29");
        }

        private string Normalize(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Replace("/", "\\");
        }

        private string GetDomainFromConfig()
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings["ActiveDirectoryDomain"];
            }
            catch
            {
                return null;
            }
        }

        private string GetDefaultDomain()
        {
            try
            {
                return System.Environment.UserDomainName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
