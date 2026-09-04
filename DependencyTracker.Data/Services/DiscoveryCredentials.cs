using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace DependencyTracker.Data.Services
{
    /// <summary>
    /// Credentials supplied on the discovery form to reach network shares / remote
    /// physical paths that the application's own Windows identity cannot access.
    /// Password is held on the server only; it is not round-tripped through the browser
    /// after the initial scan.
    /// </summary>
    public class DiscoveryCredentials
    {
        /// <summary>Account name, e.g. "DOMAIN\jsmith" or "jsmith@company.com".</summary>
        public string Username { get; set; }

        public string Password { get; set; }

        public bool HasCredentials
        {
            get { return !string.IsNullOrWhiteSpace(Username); }
        }
    }

    /// <summary>
    /// Runs filesystem discovery actions under an impersonated Windows identity so that
    /// UNC shares and remote physical paths (from applicationHost.config) can be scanned
    /// with supplied credentials. Falls back to running in the current identity when no
    /// credentials are provided.
    /// </summary>
    public static class DiscoveryImpersonation
    {
        // LOGON32_LOGON_NEW_CREDENTIALS = 9 : lets the user log on with alternate
        //   credentials specifically to access network resources, without needing the
        //   SE_TCB privilege (unlike INTERACTIVE/NETWORK logon types used without a service).
        // LOGON32_PROVIDER_WINNT50 = 3 : default Windows provider.
        private const int Logon32LogonNewCredentials = 9;
        private const int Logon32ProviderWinnt50 = 3;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out SafeAccessTokenHandle phToken);

        public static bool CanImpersonate(DiscoveryCredentials credentials)
        {
            return credentials != null && credentials.HasCredentials;
        }

        /// <summary>
        /// Runs <paramref name="action"/> impersonating <paramref name="credentials"/> when
        /// supplied; otherwise runs it in the current identity. Returns false and leaves
        /// <paramref name="error"/> set when impersonation could not be established.
        /// </summary>
        public static bool TryRun(DiscoveryCredentials credentials, Action action, out string error)
        {
            error = null;

            if (!CanImpersonate(credentials))
            {
                action();
                return true;
            }

            string domain = null;
            string user = credentials.Username;
            var idx = credentials.Username.IndexOf('\\');
            if (idx > 0)
            {
                domain = credentials.Username.Substring(0, idx);
                user = credentials.Username.Substring(idx + 1);
            }
            else if (credentials.Username.Contains("@"))
            {
                // UPN form (user@domain): pass through as-is.
            }

            SafeAccessTokenHandle token;
            if (!LogonUser(user, domain, credentials.Password ?? string.Empty,
                    Logon32LogonNewCredentials, Logon32ProviderWinnt50, out token))
            {
                error = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
                return false;
            }

            using (token)
            {
                WindowsIdentity.RunImpersonated(token, () => action());
            }
            return true;
        }

        /// <summary>
        /// Overload returning a value from the impersonated action.
        /// </summary>
        public static bool TryRun<T>(DiscoveryCredentials credentials, Func<T> action, out T result, out string error)
        {
            error = null;
            result = default(T);

            if (!CanImpersonate(credentials))
            {
                result = action();
                return true;
            }

            string domain = null;
            string user = credentials.Username;
            var idx = credentials.Username.IndexOf('\\');
            if (idx > 0)
            {
                domain = credentials.Username.Substring(0, idx);
                user = credentials.Username.Substring(idx + 1);
            }
            else if (credentials.Username.Contains("@"))
            {
                // UPN form (user@domain): pass through as-is.
            }

            SafeAccessTokenHandle token;
            if (!LogonUser(user, domain, credentials.Password ?? string.Empty,
                    Logon32LogonNewCredentials, Logon32ProviderWinnt50, out token))
            {
                error = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
                return false;
            }

            using (token)
            {
                var captured = default(T);
                WindowsIdentity.RunImpersonated(token, () => { captured = action(); });
                result = captured;
            }
            return true;
        }
    }
}
