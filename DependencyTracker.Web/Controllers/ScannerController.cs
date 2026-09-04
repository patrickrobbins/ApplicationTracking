using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    [AdAuthorize(Roles = "Maintenance,Admin")]
    public class ScannerController : Controller
    {
        private readonly IConfigScannerService _scannerService;
        private readonly IApplicationService _applicationService;
        private readonly IApplicationPropertyService _propertyService;
        private readonly IDependencyService _dependencyService;
        private readonly IRoleProvider _roleProvider;

        public ScannerController(
            IConfigScannerService scannerService,
            IApplicationService applicationService,
            IApplicationPropertyService propertyService,
            IDependencyService dependencyService,
            IRoleProvider roleProvider)
        {
            _scannerService = scannerService;
            _applicationService = applicationService;
            _propertyService = propertyService;
            _dependencyService = dependencyService;
            _roleProvider = roleProvider;
        }

        // GET: /Scanner
        public ActionResult Index()
        {
            return View(BuildInitialModel());
        }

        // POST: /Scanner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ConfigScannerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(PopulateOptions(model));

            try
            {
                var candidates = _scannerService.Scan(
                    model.ConfigXml,
                    _propertyService.GetActiveDefinitions(),
                    _propertyService.GetAllValues(),
                    _applicationService.GetAll()).ToList();

                var application = _applicationService.GetById(model.SourceApplicationId);

                model.Candidates = candidates.Select(c => new ConfigScanCandidateViewModel
                {
                    Section = c.Section,
                    Key = c.Key,
                    Value = c.Value,
                    IsUrl = c.IsUrl,
                    MatchedDefinition = c.MatchedDefinition?.Label ?? c.MatchedDefinition?.Key,
                    MatchedPattern = c.MatchedDefinition?.ScanPattern,
                    MatchedValue = c.MatchedValue,
                    SuggestedTargetName = c.SuggestedTargetName,
                    TargetApplicationId = c.SuggestedTargetApplicationId ?? 0,
                    DependencyType = string.IsNullOrEmpty(c.SuggestedDependencyType) ? "API" : c.SuggestedDependencyType,
                    Direction = "Upstream"
                }).ToList();

                model.SourceApplicationName = application?.Name;
                model.Scanned = true;
                return View(PopulateOptions(model));
            }
            catch (XmlException ex)
            {
                ModelState.AddModelError("ConfigXml", "The pasted configuration is not valid XML: " + ex.Message);
                return View(PopulateOptions(model));
            }
        }

        // POST: /Scanner/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(ConfigScannerViewModel model)
        {
            var user = _roleProvider.CurrentUserFullName;
            var created = 0;
            var duplicates = 0;
            var skipped = 0;

            if (model.Candidates != null)
            {
                foreach (var candidate in model.Candidates.Where(c => c.Include))
                {
                    if (candidate.TargetApplicationId <= 0 || candidate.TargetApplicationId == model.SourceApplicationId)
                    {
                        skipped++;
                        continue;
                    }

                    try
                    {
                        _dependencyService.Create(new Dependency
                        {
                            SourceApplicationId = model.SourceApplicationId,
                            TargetApplicationId = candidate.TargetApplicationId,
                            DependencyType = string.IsNullOrEmpty(candidate.DependencyType) ? "API" : candidate.DependencyType,
                            Direction = string.IsNullOrEmpty(candidate.Direction) ? "Upstream" : candidate.Direction,
                            Description = null,
                            CriticalityLevel = string.IsNullOrEmpty(candidate.CriticalityLevel) ? "Low" : candidate.CriticalityLevel,
                            Frequency = null
                        }, user);
                        created++;
                    }
                    catch (InvalidOperationException)
                    {
                        duplicates++;
                    }
                }
            }

            TempData["SuccessMessage"] =
                $"Config scan created {created} dependenc{(created == 1 ? "y" : "ies")}; " +
                $"{duplicates} already existed; {skipped} skipped (no target selected).";

            return RedirectToAction("Index", "Dependencies", new { sourceId = model.SourceApplicationId });
        }

        private ConfigScannerViewModel BuildInitialModel()
        {
            return new ConfigScannerViewModel
            {
                Candidates = new List<ConfigScanCandidateViewModel>(),
                ApplicationOptions = BuildApplicationOptions(),
                TypeOptions = DependencyViewModelFactory.TypeOptions(null),
                DirectionOptions = DependencyViewModelFactory.DirectionOptions(null),
                CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(null)
            };
        }

        private ConfigScannerViewModel PopulateOptions(ConfigScannerViewModel model)
        {
            model.ApplicationOptions = BuildApplicationOptions();
            model.TypeOptions = DependencyViewModelFactory.TypeOptions(null);
            model.DirectionOptions = DependencyViewModelFactory.DirectionOptions(null);
            model.CriticalityOptions = DependencyViewModelFactory.CriticalityOptions(null);
            return model;
        }

        private IEnumerable<SelectListItem> BuildApplicationOptions()
        {
            return _applicationService.GetAll()
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Version)
                .Select(a => new SelectListItem
                {
                    Text = string.IsNullOrEmpty(a.Version) ? a.Name : a.Name + " " + a.Version,
                    Value = a.ApplicationId.ToString()
                });
        }
    }
}
