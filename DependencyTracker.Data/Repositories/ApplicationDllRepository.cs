using System;
using System.Collections.Generic;
using System.Linq;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Utilities;

namespace DependencyTracker.Data.Repositories
{
    public class ApplicationDllRepository : Repository<ApplicationDll>, IApplicationDllRepository
    {
        public ApplicationDllRepository() : base() { }

        public ApplicationDllRepository(DependencyTrackerDbContext context) : base(context) { }

        public IEnumerable<ApplicationDll> GetForApplication(int applicationId)
        {
            return DbSet
                .Where(d => d.ApplicationId == applicationId)
                .OrderBy(d => d.FileName)
                .ToList();
        }

        public int CountForApplication(int applicationId)
        {
            return DbSet.Count(d => d.ApplicationId == applicationId);
        }

        public IEnumerable<ApplicationDll> FindByDll(string dllName, string dllOperator, string version)
        {
            if (string.IsNullOrWhiteSpace(dllName))
                return Enumerable.Empty<ApplicationDll>();

            var rows = DbSet
                .Where(d => d.FileName.Contains(dllName))
                .ToList();

            return rows
                .Where(d => VersionMatches(d.Version, dllOperator, version))
                .OrderBy(d => d.ApplicationId)
                .ToList();
        }

        private static bool VersionMatches(string actualVersion, string dllOperator, string targetVersion)
        {
            if (string.IsNullOrWhiteSpace(dllOperator) && string.IsNullOrWhiteSpace(targetVersion))
                return true;

            ulong[] actual, target;
            if (!VersionHelpers.TryParse(actualVersion, out actual) || !VersionHelpers.TryParse(targetVersion, out target))
                return false;

            var comparison = VersionHelpers.CompareSegments(actual, target);
            switch (dllOperator)
            {
                case ">": return comparison > 0;
                case ">=": return comparison >= 0;
                case "<": return comparison < 0;
                case "<=": return comparison <= 0;
                default: return comparison == 0;
            }
        }
    }
}
