using System.Collections.Generic;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Web.Models.ViewModels
{
    /// <summary>
    /// Serialized shape of a node in the Cytoscape.js dependency graph.
    /// </summary>
    public class GraphNodeViewModel
    {
        public string id { get; set; }          // "app-12"
        public string name { get; set; }
        public int applicationId { get; set; }
        public int level { get; set; }          // depth from the root application
        public string environment { get; set; }
        public string criticality { get; set; }
        public string status { get; set; }
        public string category { get; set; }
        public bool isRoot { get; set; }
        public int dependencyCount { get; set; }
    }

    /// <summary>
    /// Serialized shape of an edge in the Cytoscape.js dependency graph.
    /// </summary>
    public class GraphEdgeViewModel
    {
        public string id { get; set; }          // "dep-5"
        public string source { get; set; }      // node id "app-4"
        public string target { get; set; }      // node id "app-2"
        public int dependencyId { get; set; }
        public string type { get; set; }        // API, Database, ...
        public string criticality { get; set; }
        public string impact { get; set; }
        public string frequency { get; set; }
        public string direction { get; set; }
        public int level { get; set; }
    }

    public class GraphDataViewModel
    {
        public IEnumerable<GraphNodeViewModel> nodes { get; set; }
        public IEnumerable<GraphEdgeViewModel> edges { get; set; }
        public int rootApplicationId { get; set; }
        public int maxDepth { get; set; }
    }

    public class GraphPageViewModel
    {
        public IEnumerable<Application> Applications { get; set; }
        public int? SelectedApplicationId { get; set; }
        public string Direction { get; set; }
        public int MaxDepth { get; set; }
        public bool CanEdit { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalApplications { get; set; }
        public int TotalDependencies { get; set; }
        public int CriticalDependencies { get; set; }
        public int ActiveApplications { get; set; }
        public int RetiredApplications { get; set; }
        public int ApplicationsWithNoDependencies { get; set; }
        public IEnumerable<Application> RecentApplications { get; set; }
        public IEnumerable<ActivityLogEntry> RecentActivity { get; set; }
        public IDictionary<string, int> ByEnvironment { get; set; }
        public IDictionary<string, int> ByCriticality { get; set; }
    }
}
