using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    [AdAuthorize(Roles = "Viewer,Maintenance,Admin")]
    public class GraphController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IRoleProvider _roleProvider;

        public GraphController(IApplicationService applicationService, IRoleProvider roleProvider)
        {
            _applicationService = applicationService;
            _roleProvider = roleProvider;
        }

        // GET: /Graph
        public ActionResult Index(int? applicationId, string direction = "Both", int depth = 0)
        {
            // 0 (default) means "all levels"; 1-10 limit the chain depth.
            if (depth < 0 || depth > 10)
                depth = 0;

            if (direction != "Upstream" && direction != "Downstream")
                direction = "Both";

            var apps = _applicationService.GetAll()
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Version)
                .ToList();

            var model = new GraphPageViewModel
            {
                Applications = apps,
                SelectedApplicationId = applicationId,
                Direction = direction,
                MaxDepth = depth,
                CanEdit = _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleMaintenance) ||
                          _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleAdmin)
            };

            return View(model);
        }
    }
}
