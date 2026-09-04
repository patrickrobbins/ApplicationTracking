using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    /// <summary>
    /// AJAX endpoints feeding the Cytoscape.js visualization.
    /// </summary>
    [AdAuthorize(Roles = "Viewer,Maintenance,Admin")]
    public class ApiController : Controller
    {
        private readonly IDependencyService _dependencyService;
        private readonly IApplicationService _applicationService;

        public ApiController(IDependencyService dependencyService, IApplicationService applicationService)
        {
            _dependencyService = dependencyService;
            _applicationService = applicationService;
        }

        // GET: /api/graph?applicationId=5&depth=3&direction=Both
        public JsonResult Graph(int? applicationId, int depth = 0, string direction = "Both")
        {
            // 0 (default) means "all levels"; 1-10 limit the chain depth.
            if (depth < 0 || depth > 10)
                depth = 0;

            if (direction != "Upstream" && direction != "Downstream")
                direction = "Both";

            // "All levels" queries the chain with an effectively unlimited depth.
            var queryDepth = depth == 0 ? 100 : depth;

            if (!applicationId.HasValue)
                return Json(BuildAllGraph(depth), JsonRequestBehavior.AllowGet);

            var root = _applicationService.GetById(applicationId.Value);
            if (root == null)
                return Json(new { success = false, message = "Application not found." }, JsonRequestBehavior.AllowGet);

            // Query each side separately: the sproc's 'Both' mode can surface
            // "sibling" applications (dependents of an upstream app) that are
            // neither upstream nor downstream of the root. Splitting keeps the
            // graph consistent with the Details page lists.
            var chain = new List<DependencyChainResult>();
            if (direction == "Both")
            {
                chain.AddRange(_dependencyService.GetChain(applicationId.Value, queryDepth, "Upstream"));
                chain.AddRange(_dependencyService.GetChain(applicationId.Value, queryDepth, "Downstream"));
            }
            else
            {
                chain.AddRange(_dependencyService.GetChain(applicationId.Value, queryDepth, direction));
            }

            var nodes = BuildNodes(chain, root);
            var edges = chain.Select(c => new GraphEdgeViewModel
            {
                id = "dep-" + c.DependencyId,
                source = "app-" + c.SourceApplicationId,
                target = "app-" + c.TargetApplicationId,
                dependencyId = c.DependencyId,
                type = c.DependencyType,
                criticality = c.CriticalityLevel,
                impact = c.Impact,
                frequency = c.Frequency,
                direction = c.Direction,
                level = c.Depth
            }).ToList();

            var data = new GraphDataViewModel
            {
                nodes = nodes,
                edges = edges,
                rootApplicationId = root.ApplicationId,
                maxDepth = depth
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: /api/applications/summary
        public JsonResult ApplicationSummary(int? id)
        {
            if (!id.HasValue)
                return Json(new { success = false, message = "Application not found." }, JsonRequestBehavior.AllowGet);

            var app = _applicationService.GetById(id.Value);
            if (app == null)
                return Json(new { success = false, message = "Application not found." }, JsonRequestBehavior.AllowGet);

            var upstream = _dependencyService.GetUpstream(id.Value).ToList();
            var downstream = _dependencyService.GetDownstream(id.Value).ToList();

            return Json(new
            {
                success = true,
                application = new
                {
                    id = app.ApplicationId,
                    name = app.Name,
                    version = app.Version,
                    description = app.Description,
                    businessGroup = app.BusinessGroup,
                    businessOwner = app.BusinessOwner,
                    businessOwnerEmail = app.BusinessOwnerEmail,
                    businessBackup = app.BusinessBackup,
                    businessBackupEmail = app.BusinessBackupEmail,
                    technicalOwnerEmail = app.TechnicalOwnerEmail,
                    environment = app.Environment,
                    criticality = app.CriticalityLevel,
                    status = app.Status,
                    category = app.Category?.Name,
                    family = app.FamilyId == null ? null : app.Family.Name,
                    technicalOwnershipTeam = app.TechnicalOwnershipTeamId == null ? null : app.TechnicalOwnershipTeam.Name
                },
                upstreamCount = upstream.Count,
                downstreamCount = downstream.Count,
                detailsUrl = Url.Action("Details", "Applications", new { id })
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Full graph: every application as a node and every dependency as an edge.
        /// Used by the graph page's "All applications" option (no root is highlighted).
        /// </summary>
        private GraphDataViewModel BuildAllGraph(int depth)
        {
            var apps = _applicationService.GetAll().ToList();
            var dependencies = _dependencyService.GetAll().ToList();

            var nodes = apps.Select(a => new GraphNodeViewModel
            {
                id = "app-" + a.ApplicationId,
                applicationId = a.ApplicationId,
                name = DisplayName(a),
                level = 0,
                environment = a.Environment,
                criticality = a.CriticalityLevel,
                status = a.Status,
                category = a.Category?.Name,
                isRoot = false,
                dependencyCount = dependencies.Count(d =>
                    d.SourceApplicationId == a.ApplicationId || d.TargetApplicationId == a.ApplicationId)
            }).OrderBy(n => n.name).ToList();

            var edges = dependencies.Select(d => new GraphEdgeViewModel
            {
                id = "dep-" + d.DependencyId,
                source = "app-" + d.SourceApplicationId,
                target = "app-" + d.TargetApplicationId,
                dependencyId = d.DependencyId,
                type = d.DependencyType,
                criticality = d.CriticalityLevel,
                impact = d.Impact,
                frequency = d.Frequency,
                direction = d.Direction,
                level = 0
            }).ToList();

            return new GraphDataViewModel
            {
                nodes = nodes,
                edges = edges,
                rootApplicationId = 0,
                maxDepth = depth
            };
        }

        private List<GraphNodeViewModel> BuildNodes(IEnumerable<DependencyChainResult> chain, Application root)
        {
            var nodeMap = new Dictionary<string, GraphNodeViewModel>();
            var levelMap = new Dictionary<int, int>();

            var applications = _applicationService.GetAll().ToList();
            var categories = applications.ToDictionary(a => a.ApplicationId, a => a.Category?.Name);
            var versions = applications.ToDictionary(a => a.ApplicationId, a => a.Version);

            string DisplayName(int applicationId, string name)
            {
                string version;
                if (versions.TryGetValue(applicationId, out version) && !string.IsNullOrEmpty(version))
                    return name + " " + version;
                return name;
            }

            // Root node at level 0
            nodeMap["app-" + root.ApplicationId] = new GraphNodeViewModel
            {
                id = "app-" + root.ApplicationId,
                applicationId = root.ApplicationId,
                name = DisplayName(root.ApplicationId, root.Name),
                level = 0,
                environment = root.Environment,
                criticality = root.CriticalityLevel,
                status = root.Status,
                category = root.Category?.Name,
                isRoot = true,
                dependencyCount = chain.Count(c =>
                    c.SourceApplicationId == root.ApplicationId || c.TargetApplicationId == root.ApplicationId)
            };

            foreach (var edge in chain)
            {
                var sourceId = "app-" + edge.SourceApplicationId;
                var targetId = "app-" + edge.TargetApplicationId;

                if (!nodeMap.ContainsKey(sourceId))
                {
                    nodeMap[sourceId] = new GraphNodeViewModel
                    {
                        id = sourceId,
                        applicationId = edge.SourceApplicationId,
                        name = DisplayName(edge.SourceApplicationId, edge.SourceName),
                        level = edge.Depth,
                        environment = edge.SourceEnvironment,
                        criticality = edge.SourceCriticality,
                        status = edge.SourceStatus,
                        category = categories.TryGetValue(edge.SourceApplicationId, out var srcCat) ? srcCat : null,
                        dependencyCount = 0
                    };
                }

                if (!nodeMap.ContainsKey(targetId))
                {
                    nodeMap[targetId] = new GraphNodeViewModel
                    {
                        id = targetId,
                        applicationId = edge.TargetApplicationId,
                        name = DisplayName(edge.TargetApplicationId, edge.TargetName),
                        level = edge.Depth,
                        environment = edge.TargetEnvironment,
                        criticality = edge.TargetCriticality,
                        status = edge.TargetStatus,
                        category = categories.TryGetValue(edge.TargetApplicationId, out var tgtCat) ? tgtCat : null,
                        dependencyCount = 0
                    };
                }
            }

            return nodeMap.Values.OrderBy(n => n.level).ThenBy(n => n.name).ToList();
        }

        /// <summary>
        /// Display name for a node: the application name plus its version when set,
        /// so multiple deployed versions render as distinct, distinguishable nodes.
        /// </summary>
        private static string DisplayName(Application application)
        {
            if (string.IsNullOrEmpty(application.Version))
                return application.Name;
            return application.Name + " " + application.Version;
        }
    }
}
