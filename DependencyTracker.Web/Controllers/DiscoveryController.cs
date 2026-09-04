using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    /// <summary>
    /// Scans an IIS host or network share for applications (by finding web.config /
    /// app.config recursively and, when available, reading applicationHost.config for
    /// IIS site names). Discovered applications are reviewed against existing ones,
    /// dependencies found in their configs are reviewed, and everything is applied in
    /// one pass: new applications are created, matching ones updated, and confirmed
    /// config dependencies become dependency edges.
    /// </summary>
    [AdAuthorize(Roles = "Maintenance,Admin")]
    public class DiscoveryController : Controller
    {
        private readonly IAppDiscoveryService _discoveryService;
        private readonly IConfigScannerService _scannerService;
        private readonly IApplicationService _applicationService;
        private readonly IApplicationPropertyService _propertyService;
        private readonly IDependencyService _dependencyService;
        private readonly IApplicationDllService _dllService;
        private readonly IRoleProvider _roleProvider;

        public DiscoveryController(
            IAppDiscoveryService discoveryService,
            IConfigScannerService scannerService,
            IApplicationService applicationService,
            IApplicationPropertyService propertyService,
            IDependencyService dependencyService,
            IApplicationDllService dllService,
            IRoleProvider roleProvider)
        {
            _discoveryService = discoveryService;
            _scannerService = scannerService;
            _applicationService = applicationService;
            _propertyService = propertyService;
            _dependencyService = dependencyService;
            _dllService = dllService;
            _roleProvider = roleProvider;
        }

        // GET: /Discovery
        public ActionResult Index()
        {
            return View(new DiscoveryIndexViewModel());
        }

        // POST: /Discovery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(DiscoveryIndexViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var credentials = SaveCredentials(model.Username, model.Password);
                var discovered = _discoveryService.Discover(model.BasePath, model.AppHostConfigPath, credentials);

                if (discovered.Count == 0)
                {
                    model.Warning = "No applications found under the given path.";
                    model.Scanned = true;
                    return View(model);
                }

                var existing = _applicationService.GetAll().ToList();
                model.Apps = discovered.Select(d => BuildDiscoveredViewModel(d, existing)).ToList();
                model.Scanned = true;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Discovery failed: " + ex.Message);
                return View(model);
            }
        }

        private DiscoveryCredentials SaveCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Session.Remove("DiscoveryCredentials");
                return null;
            }

            var credentials = new DiscoveryCredentials { Username = username, Password = password };
            Session["DiscoveryCredentials"] = credentials;
            return credentials;
        }

        private DiscoveryCredentials GetStoredCredentials()
        {
            return Session["DiscoveryCredentials"] as DiscoveryCredentials;
        }

        private string ClearCredentials()
        {
            var credentials = Session["DiscoveryCredentials"] as DiscoveryCredentials;
            var username = credentials == null ? null : credentials.Username;
            Session.Remove("DiscoveryCredentials");
            return username;
        }

        // POST: /Discovery/ReviewDependencies
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReviewDependencies(DiscoveryIndexViewModel model)
        {
            if (model.Apps == null || model.Apps.Count == 0)
            {
                TempData["ErrorMessage"] = "No applications were selected to review.";
                return RedirectToAction("Index");
            }

            var selected = model.Apps.Where(a => a.Include && !string.IsNullOrWhiteSpace(a.SourcePath)).ToList();
            if (selected.Count == 0)
            {
                TempData["ErrorMessage"] = "No applications were selected to review.";
                return RedirectToAction("Index");
            }

            var definitions = _propertyService.GetActiveDefinitions().ToList();
            var allValues = _propertyService.GetAllValues().ToList();
            var allApps = _applicationService.GetAll().ToList();

            var review = new DiscoveryReviewViewModel
            {
                BasePath = model.BasePath,
                AppHostConfigPath = model.AppHostConfigPath,
                Apps = new List<DiscoveryAppReviewViewModel>()
            };

            foreach (var app in selected)
            {
                review.Apps.Add(new DiscoveryAppReviewViewModel
                {
                    Name = app.Name,
                    SourcePath = app.SourcePath,
                    SiteName = app.SiteName,
                    MatchedApplicationId = app.MatchedApplicationId,
                    DllCount = _discoveryService.GetDllFiles(app.SourcePath, GetStoredCredentials()).Count,
                    Candidates = BuildCandidates(app, definitions, allValues, allApps)
                });
            }

            return View("DependencyReview", review);
        }

        // POST: /Discovery/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Apply(DiscoveryReviewViewModel model)
        {
            var user = _roleProvider.CurrentUserFullName;
            var created = 0;
            var updated = 0;
            var dependenciesCreated = 0;
            var dependenciesSkipped = 0;
            var dllsSynced = 0;
            var errors = new List<string>();

            if (model.Apps != null)
            {
                foreach (var app in model.Apps)
                {
                    if (string.IsNullOrWhiteSpace(app.Name) || string.IsNullOrWhiteSpace(app.SourcePath))
                    {
                        dependenciesSkipped++;
                        continue;
                    }

                    int sourceId;
                    if (app.MatchedApplicationId > 0)
                    {
                        var existing = _applicationService.GetById(app.MatchedApplicationId);
                        if (existing == null)
                        {
                            sourceId = CreateApplication(app, user, ref created, errors);
                        }
                        else if (string.IsNullOrWhiteSpace(existing.SourcePath)
                                 || !string.Equals(_discoveryService.NormalizePath(existing.SourcePath),
                                     _discoveryService.NormalizePath(app.SourcePath), StringComparison.OrdinalIgnoreCase))
                        {
                            existing.SourcePath = app.SourcePath;
                            try
                            {
                                _applicationService.Update(existing, user);
                                updated++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add("'" + app.Name + "': " + ex.Message);
                            }
                            sourceId = existing.ApplicationId;
                        }
                        else
                        {
                            updated++;
                            sourceId = existing.ApplicationId;
                        }
                    }
                    else
                    {
                        sourceId = CreateApplication(app, user, ref created, errors);
                    }

                    if (sourceId <= 0)
                        continue;

                    var discoveredDlls = _discoveryService.GetDllFiles(app.SourcePath, GetStoredCredentials());
                    if (discoveredDlls.Count > 0)
                        dllsSynced += _dllService.SyncFromDiscovery(sourceId, app.Name, discoveredDlls, user);

                    if (app.Candidates != null)
                    {
                        foreach (var candidate in app.Candidates.Where(c => c.Include))
                        {
                            if (candidate.TargetApplicationId <= 0 || candidate.TargetApplicationId == sourceId)
                            {
                                dependenciesSkipped++;
                                continue;
                            }

                            try
                            {
                                _dependencyService.Create(new Dependency
                                {
                                    SourceApplicationId = sourceId,
                                    TargetApplicationId = candidate.TargetApplicationId,
                                    DependencyType = string.IsNullOrEmpty(candidate.DependencyType) ? "API" : candidate.DependencyType,
                                    Direction = string.IsNullOrEmpty(candidate.Direction) ? "Upstream" : candidate.Direction,
                                    Description = null,
                                    CriticalityLevel = string.IsNullOrEmpty(candidate.CriticalityLevel) ? "Low" : candidate.CriticalityLevel,
                                    Frequency = null
                                }, user);
                                dependenciesCreated++;
                            }
                            catch (InvalidOperationException)
                            {
                                dependenciesSkipped++;
                            }
                        }
                    }
                }
            }

            var message = "Server discovery applied: " + created + " application(s) created, " +
                          updated + " matched/updated, " + dllsSynced + " DLL(s) recorded, " +
                          dependenciesCreated + " dependenc" +
                          (dependenciesCreated == 1 ? "y" : "ies") + " created, " +
                          dependenciesSkipped + " skipped/duplicates.";
            if (errors.Count > 0)
                message += " Errors: " + string.Join("; ", errors);

            ClearCredentials();

            TempData[errors.Count > 0 ? "ErrorMessage" : "SuccessMessage"] = message;
            return RedirectToAction("Index", "Applications");
        }

        private DiscoveredAppViewModel BuildDiscoveredViewModel(DiscoveredApplication d, List<Application> existing)
        {
            var matches = FindMatches(d, existing).ToList();
            var pathMatch = matches.FirstOrDefault(m => m.Score >= MatchScore.Path);

            var options = new List<SelectListItem>
            {
                new SelectListItem { Text = "Create new application", Value = "0", Selected = pathMatch == null }
            };
            foreach (var m in matches)
                options.Add(new SelectListItem
                {
                    Text = m.Application.Name + (pathMatch == m ? " (same path)" : ""),
                    Value = m.Application.ApplicationId.ToString(),
                    Selected = pathMatch == m
                });

            return new DiscoveredAppViewModel
            {
                Include = true,
                Name = d.Name,
                SourcePath = d.SourcePath,
                SiteName = d.SiteName,
                HasConfig = d.HasConfig,
                DllCount = d.DllFiles == null ? 0 : d.DllFiles.Count,
                MatchedApplicationId = pathMatch == null ? 0 : pathMatch.Application.ApplicationId,
                MatchOptions = options
            };
        }

        private IEnumerable<MatchCandidate> FindMatches(DiscoveredApplication d, List<Application> existing)
        {
            var normPath = _discoveryService.NormalizePath(d.SourcePath);
            var leaf = LeafName(d.SourcePath);

            foreach (var app in existing)
            {
                var appPath = _discoveryService.NormalizePath(app.SourcePath);
                var appLeaf = LeafName(app.SourcePath);

                var score = 0;
                if (normPath != null && appPath != null
                    && string.Equals(normPath, appPath, StringComparison.OrdinalIgnoreCase))
                    score = MatchScore.Path;
                else if (!string.IsNullOrWhiteSpace(d.SiteName)
                         && string.Equals(app.Name, d.SiteName, StringComparison.OrdinalIgnoreCase))
                    score = MatchScore.Name;
                else if (!string.IsNullOrWhiteSpace(leaf)
                         && string.Equals(app.Name, leaf, StringComparison.OrdinalIgnoreCase))
                    score = MatchScore.Name;
                else if (!string.IsNullOrWhiteSpace(appLeaf)
                         && string.Equals(leaf, appLeaf, StringComparison.OrdinalIgnoreCase))
                    score = MatchScore.Leaf;

                if (score > 0)
                    yield return new MatchCandidate { Application = app, Score = score };
            }
        }

        private List<ConfigScanCandidateViewModel> BuildCandidates(
            DiscoveredAppViewModel app,
            List<ApplicationPropertyDefinition> definitions,
            List<ApplicationPropertyValue> allValues,
            List<Application> allApps)
        {
            var result = new List<ConfigScanCandidateViewModel>();
            if (definitions.Count == 0)
                return result;

            var credentials = GetStoredCredentials();
            string error;
            DiscoveryImpersonation.TryRun(credentials, () =>
            {
                foreach (var configFile in _discoveryService.GetConfigFilePaths(app.SourcePath, credentials))
                {
                    string xml;
                    try
                    {
                        xml = System.IO.File.ReadAllText(configFile);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    IEnumerable<ConfigScanCandidate> scanned;
                    try
                    {
                        scanned = _scannerService.Scan(xml, definitions, allValues, allApps);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    foreach (var c in scanned)
                    {
                        if (!c.SuggestedTargetApplicationId.HasValue || c.SuggestedTargetApplicationId.Value <= 0)
                            continue;

                        result.Add(new ConfigScanCandidateViewModel
                        {
                            Section = c.Section,
                            Key = c.Key,
                            Value = c.Value,
                            IsUrl = c.IsUrl,
                            MatchedDefinition = c.MatchedDefinition == null ? null
                                : (c.MatchedDefinition.Label ?? c.MatchedDefinition.Key),
                            MatchedValue = c.MatchedValue,
                            SuggestedTargetName = c.SuggestedTargetName,
                            TargetApplicationId = c.SuggestedTargetApplicationId.Value,
                            DependencyType = string.IsNullOrEmpty(c.SuggestedDependencyType) ? "API" : c.SuggestedDependencyType,
                            Direction = "Upstream",
                            Include = true
                        });
                    }
                }
            }, out error);

            return result
                .GroupBy(c => c.TargetApplicationId + "|" + c.DependencyType + "|" + c.CriticalityLevel)
                .Select(g => g.First())
                .OrderBy(c => c.SuggestedTargetName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private int CreateApplication(DiscoveryAppReviewViewModel app, string user, ref int created, List<string> errors)
        {
            try
            {
                var application = _applicationService.Create(new Application
                {
                    Name = app.Name,
                    Version = "1.0.0",
                    SourcePath = app.SourcePath,
                    Status = "Active"
                }, user);
                created++;
                return application.ApplicationId;
            }
            catch (Exception ex)
            {
                errors.Add("'" + app.Name + "': " + ex.Message);
                return 0;
            }
        }

        private static string LeafName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            try
            {
                return Path.GetFileName(path.TrimEnd('\\', '/'));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private sealed class MatchCandidate
        {
            public Application Application { get; set; }
            public int Score { get; set; }
        }

        private static class MatchScore
        {
            public const int Leaf = 1;
            public const int Name = 2;
            public const int Path = 3;
        }
    }
}
