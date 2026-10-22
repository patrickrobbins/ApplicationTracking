using System;

namespace DependencyTracker.Web.Logging
{
    /// <summary>
    /// Converts exceptions into messages that are safe to show to a user.
    /// Business rule violations (InvalidOperationException) keep their
    /// intentional wording; every other failure is written to the error log
    /// and replaced with a generic message so database details, constraint
    /// names, stack traces and file paths are never rendered in the browser.
    /// </summary>
    public static class UserFacingError
    {
        public const string GenericMessage =
            "The operation could not be completed. Please try again or contact an administrator.";

        /// <summary>
        /// Returns a message safe to display, logging the original exception.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="context">Short description of the failed operation, written to the log.</param>
        public static string From(Exception exception, string context)
        {
            if (exception == null)
                return GenericMessage;

            AppLogger.LogError(exception, context);

            var invalidOperation = exception as InvalidOperationException;
            if (invalidOperation != null && !string.IsNullOrWhiteSpace(invalidOperation.Message))
                return invalidOperation.Message;

            return GenericMessage;
        }
    }
}
