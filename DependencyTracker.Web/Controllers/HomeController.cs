using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Repositories;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IDependencyRepository _dependencyRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IRoleProvider _roleProvider;

        public HomeController(
            IApplicationRepository applicationRepository,
            IDependencyRepository dependencyRepository,
            IAdminRepository adminRepository,
            IRoleProvider roleProvider)
        {
            _applicationRepository = applicationRepository;
            _dependencyRepository = dependencyRepository;
            _adminRepository = adminRepository;
            _roleProvider = roleProvider;
        }

        public ActionResult Index()
        {
            var viewModel = new DashboardViewModel();
            var all = _applicationRepository.GetAll().ToList();

            viewModel.TotalApplications = all.Count;
            viewModel.ActiveApplications = all.Count(a => a.Status == "Active");
            viewModel.RetiredApplications = all.Count(a => a.Status == "Retired");

            var allDeps = _dependencyRepository.GetAll().ToList();
            viewModel.TotalDependencies = allDeps.Count;
            viewModel.CriticalDependencies = allDeps.Count(d => d.CriticalityLevel == "Critical");

            var dependentAppIds = new HashSet<int>(
                allDeps.Select(d => d.SourceApplicationId)
                    .Concat(allDeps.Select(d => d.TargetApplicationId)));
            viewModel.ApplicationsWithNoDependencies = all.Count(a => !dependentAppIds.Contains(a.ApplicationId));

            viewModel.RecentApplications = all
                .OrderByDescending(a => a.ModifiedDate)
                .Take(6)
                .ToList();

            viewModel.RecentActivity = _adminRepository.GetActivityLog(8).ToList();

            viewModel.ByEnvironment = all
                .Where(a => !string.IsNullOrEmpty(a.Environment))
                .GroupBy(a => a.Environment)
                .ToDictionary(g => g.Key, g => g.Count());

            viewModel.ByCriticality = all
                .Where(a => !string.IsNullOrEmpty(a.CriticalityLevel))
                .GroupBy(a => a.CriticalityLevel)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.CanEdit = _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleMaintenance) ||
                              _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleAdmin);
            ViewBag.IsAdmin = _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleAdmin);
            return View(viewModel);
        }

        public ActionResult Dashboard()
        {
            return RedirectToAction("Index");
        }
    }
}
