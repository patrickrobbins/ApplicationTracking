using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    [AdAuthorize(Roles = "Viewer,Maintenance,Admin")]
    public class DependenciesController : Controller
    {
        private readonly IDependencyService _dependencyService;
        private readonly IApplicationService _applicationService;
        private readonly IRoleProvider _roleProvider;

        public DependenciesController(
            IDependencyService dependencyService,
            IApplicationService applicationService,
            IRoleProvider roleProvider)
        {
            _dependencyService = dependencyService;
            _applicationService = applicationService;
            _roleProvider = roleProvider;
        }

        // GET: /Dependencies
        public ActionResult Index(string dependencyType, string criticality, int? sourceId, int? targetId)
        {
            var deps = _dependencyService.Search(dependencyType, criticality, sourceId, targetId).ToList();
            var apps = _applicationService.GetAll().OrderBy(a => a.Name).ToList();

            var model = new DependencyListViewModel
            {
                DependencyType = dependencyType,
                Criticality = criticality,
                SourceId = sourceId,
                TargetId = targetId,
                CanEdit = IsEditor(),
                TypeOptions = DependencyViewModelFactory.TypeOptions(dependencyType),
                CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(criticality),
                SourceOptions = apps.Select(a => new SelectListItem { Text = ApplicationDisplayName(a), Value = a.ApplicationId.ToString() }),
                TargetOptions = apps.Select(a => new SelectListItem { Text = ApplicationDisplayName(a), Value = a.ApplicationId.ToString() }),
                Dependencies = deps.Select(d => new DependencyListItemViewModel
                {
                    DependencyId = d.DependencyId,
                    SourceApplicationId = d.SourceApplicationId,
                    SourceName = d.SourceApplication?.Name ?? d.SourceApplicationId.ToString(),
                    TargetApplicationId = d.TargetApplicationId,
                    TargetName = d.TargetApplication?.Name ?? d.TargetApplicationId.ToString(),
                    DependencyType = d.DependencyType,
                    Direction = d.Direction,
                    CriticalityLevel = d.CriticalityLevel,
                    Frequency = d.Frequency
                }).ToList()
            };

            return View(model);
        }

        // GET: /Dependencies/Create
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Create(int? sourceId, int? targetId)
        {
            var criticality = "Low";
            string impact = null;

            if (targetId.HasValue)
            {
                var target = _applicationService.GetById(targetId.Value);
                if (target != null)
                {
                    if (!string.IsNullOrEmpty(target.DefaultDependencyCriticality))
                        criticality = target.DefaultDependencyCriticality;
                    impact = target.DefaultDependencyImpact;
                }
            }

            var model = new DependencyFormViewModel
            {
                SourceApplicationId = sourceId ?? 0,
                TargetApplicationId = targetId ?? 0,
                DependencyType = "API",
                Direction = "Upstream",
                CriticalityLevel = criticality,
                Impact = impact,
                ApplicationOptions = BuildApplicationOptions(),
                TypeOptions = DependencyViewModelFactory.TypeOptions(null),
                DirectionOptions = DependencyViewModelFactory.DirectionOptions(null),
                FrequencyOptions = DependencyViewModelFactory.FrequencyOptions(null),
                CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(criticality)
            };
            return View(model);
        }

        // POST: /Dependencies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Create(DependencyFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dependency = new Dependency
                    {
                        SourceApplicationId = model.SourceApplicationId,
                        TargetApplicationId = model.TargetApplicationId,
                        DependencyType = model.DependencyType,
                        Direction = string.IsNullOrEmpty(model.Direction) ? "Upstream" : model.Direction,
                        Description = model.Description,
                        CriticalityLevel = string.IsNullOrEmpty(model.CriticalityLevel) ? "Low" : model.CriticalityLevel,
                        Impact = model.Impact,
                        Frequency = model.Frequency
                    };

                    var created = _dependencyService.Create(dependency, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = "Dependency created successfully.";
                    return RedirectToAction("Details", "Applications", new { id = created.SourceApplicationId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            model.ApplicationOptions = BuildApplicationOptions();
            model.TypeOptions = DependencyViewModelFactory.TypeOptions(model.DependencyType);
            model.DirectionOptions = DependencyViewModelFactory.DirectionOptions(model.Direction);
            model.FrequencyOptions = DependencyViewModelFactory.FrequencyOptions(model.Frequency);
            model.CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(model.CriticalityLevel);
            return View(model);
        }

        // GET: /Dependencies/Edit/5
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var dependency = _dependencyService.GetById(id.Value);
            if (dependency == null)
                return HttpNotFound();

            var model = new DependencyFormViewModel
            {
                DependencyId = dependency.DependencyId,
                SourceApplicationId = dependency.SourceApplicationId,
                TargetApplicationId = dependency.TargetApplicationId,
                DependencyType = dependency.DependencyType,
                Direction = dependency.Direction,
                Description = dependency.Description,
                CriticalityLevel = dependency.CriticalityLevel,
                Impact = dependency.Impact,
                Frequency = dependency.Frequency,
                ApplicationOptions = BuildApplicationOptions(),
                TypeOptions = DependencyViewModelFactory.TypeOptions(dependency.DependencyType),
                DirectionOptions = DependencyViewModelFactory.DirectionOptions(dependency.Direction),
                FrequencyOptions = DependencyViewModelFactory.FrequencyOptions(dependency.Frequency),
                CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(dependency.CriticalityLevel)
            };
            return View(model);
        }

        // POST: /Dependencies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Edit(DependencyFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dependency = new Dependency
                    {
                        DependencyId = model.DependencyId,
                        SourceApplicationId = model.SourceApplicationId,
                        TargetApplicationId = model.TargetApplicationId,
                        DependencyType = model.DependencyType,
                        Direction = model.Direction,
                        Description = model.Description,
                        CriticalityLevel = string.IsNullOrEmpty(model.CriticalityLevel) ? "Low" : model.CriticalityLevel,
                        Impact = model.Impact,
                        Frequency = model.Frequency
                    };

                    _dependencyService.Update(dependency, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = "Dependency updated successfully.";
                    return RedirectToAction("Details", "Applications", new { id = dependency.SourceApplicationId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            model.ApplicationOptions = BuildApplicationOptions();
            model.TypeOptions = DependencyViewModelFactory.TypeOptions(model.DependencyType);
            model.DirectionOptions = DependencyViewModelFactory.DirectionOptions(model.Direction);
            model.FrequencyOptions = DependencyViewModelFactory.FrequencyOptions(model.Frequency);
            model.CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(model.CriticalityLevel);
            return View(model);
        }

        // POST: /Dependencies/TargetDefaults
        [HttpPost]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public JsonResult TargetDefaults(int id)
        {
            var target = _applicationService.GetById(id);
            if (target == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                criticality = target.DefaultDependencyCriticality,
                impact = target.DefaultDependencyImpact
            });
        }

        // POST: /Dependencies/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Delete(int? id, int? returnApplicationId)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _dependencyService.Delete(id.Value, _roleProvider.CurrentUserFullName);
            TempData["SuccessMessage"] = "Dependency deleted successfully.";
            if (returnApplicationId.HasValue)
                return RedirectToAction("Details", "Applications", new { id = returnApplicationId.Value });
            return RedirectToAction("Index");
        }

        private IEnumerable<SelectListItem> BuildApplicationOptions()
        {
            return _applicationService.GetAll()
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Version)
                .Select(a => new SelectListItem { Text = ApplicationDisplayName(a), Value = a.ApplicationId.ToString() });
        }

        private static string ApplicationDisplayName(Application application)
        {
            if (string.IsNullOrEmpty(application.Version))
                return application.Name;
            return application.Name + " " + application.Version;
        }

        private bool IsEditor()
        {
            return _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleMaintenance) ||
                   _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleAdmin);
        }
    }
}
