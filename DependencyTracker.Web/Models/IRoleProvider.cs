using System.Collections.Generic;

namespace DependencyTracker.Web.Models
{
    /// <summary>
    /// Resolves the roles for the current (or a named) Windows-authenticated user.
    /// Implementations determine roles from Active Directory groups or from an
    /// alternate source such as a development-only "full access" provider.
    /// </summary>
    public interface IRoleProvider
    {
        string CurrentUserName { get; }

        string CurrentUserFullName { get; }

        List<string> GetCurrentUserRoles();

        List<string> GetUserRoles(string userName);

        bool IsCurrentUserInRole(string role);
    }
}
