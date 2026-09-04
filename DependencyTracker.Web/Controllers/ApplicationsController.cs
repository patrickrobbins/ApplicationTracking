using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Services;
using DependencyTracker.Data.Utilities;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    [AdAuthorize(Roles = "Viewer,Maintenance,Admin")]
    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IDependencyService _dependencyService;
        private readonly IApplicationPropertyService _propertyService;
        private readonly IApplicationCategoryService _categoryService;
        private readonly IApplicationTechnologyService _technologyService;
        private readonly IAppDiscoveryService _discoveryService;
        private readonly IApplicationDllService _dllService;
        private readonly IRoleProvider _roleProvider;

        public ApplicationsController(
            IApplicationService applicationService,
            IDependencyService dependencyService,
            IApplicationPropertyService propertyService,
            IApplicationCategoryService categoryService,
            IApplicationTechnologyService technologyService,
            IAppDiscoveryService discoveryService,
            IApplicationDllService dllService,
            IRoleProvider roleProvider)
        {
            _applicationService = applicationService;
            _dependencyService = dependencyService;
            _propertyService = propertyService;
            _categoryService = categoryService;
            _technologyService = technologyService;
            _discoveryService = discoveryService;
            _dllService = dllService;
            _roleProvider = roleProvider;
        }

        // GET: /Applications
        public ActionResult Index(string term, string environment, string status, string criticality, int? categoryId,
            string version, int? technologyId, string dllName, string dllOperator, string dllVersion,
            bool showAllVersions = false, bool includeDeleted = false)
        {
            var results = _applicationService.Search(term, environment, status, criticality, categoryId, version, technologyId, includeDeleted).ToList();

            var matchedDlls = new Dictionary<int, List<string>>();
            if (!string.IsNullOrWhiteSpace(dllName))
            {
                if (string.IsNullOrWhiteSpace(dllOperator) && !string.IsNullOrWhiteSpace(dllVersion))
                    dllOperator = "=";

                var dllRows = _dllService.FindByDll(dllName, dllOperator, dllVersion).ToList();
                var matchedApplicationIds = dllRows.Select(d => d.ApplicationId).Distinct().ToHashSet();
                results = results.Where(a => matchedApplicationIds.Contains(a.ApplicationId)).ToList();

                foreach (var row in dllRows)
                {
                    List<string> list;
                    if (!matchedDlls.TryGetValue(row.ApplicationId, out list))
                    {
                        list = new List<string>();
                        matchedDlls[row.ApplicationId] = list;
                    }
                    list.Add(string.IsNullOrWhiteSpace(row.Version) ? row.FileName : row.FileName + " " + row.Version);
                }
            }

            var listItems = results.Select(a => new ApplicationListItemViewModel
            {
                ApplicationId = a.ApplicationId,
                Name = a.Name,
                Version = a.Version,
                Description = a.Description,
                Environment = a.Environment,
                CriticalityLevel = a.CriticalityLevel,
                Status = a.Status,
                Category = a.Category == null ? null : a.Category.Name,
                IsDeleted = a.IsDeleted,
                DependencyCount = _dependencyService.GetForApplication(a.ApplicationId).Count(),
                MatchedDlls = matchedDlls.ContainsKey(a.ApplicationId) ? matchedDlls[a.ApplicationId] : null
            }).ToList();

            var model = new ApplicationSearchViewModel
            {
                Term = term,
                Environment = environment,
                Status = status,
                Criticality = criticality,
                CategoryId = categoryId,
                TechnologyId = technologyId,
                Version = version,
                DllName = dllName,
                DllOperator = dllOperator,
                DllVersion = dllVersion,
                ShowAllVersions = showAllVersions,
                IncludeDeleted = includeDeleted,
                DllOperators = DllOperatorOptions(dllOperator),
                Environments = _applicationService.GetAllEnvironments(),
                Statuses = _applicationService.GetAllStatuses(),
                Criticalities = _applicationService.GetAllCriticalities(),
                Versions = _applicationService.GetAllVersions(),
                Categories = CategoryOptions(categoryId),
                Technologies = TechnologyOptions(technologyId),
                CanEdit = IsEditor(),
                Results = GroupVersions(listItems, showAllVersions, string.IsNullOrWhiteSpace(status))
            };

            return View(model);
        }

        /// <summary>
        /// Groups rows by application name. When <paramref name="showAllVersions"/> is false,
        /// only the highest-version row of each group is kept; otherwise every version is kept
        /// (ordered ascending). Each group head carries its version count and sibling versions.
        /// When <paramref name="activeOnly"/> is true (no explicit status filter on the page),
        /// non-Active rows are excluded from the groups so the list shows only the versions
        /// that are currently active.
        /// </summary>
        private static List<ApplicationListItemViewModel> GroupVersions(
            List<ApplicationListItemViewModel> rows, bool showAllVersions, bool activeOnly)
        {
            var result = new List<ApplicationListItemViewModel>();
            foreach (var group in rows.GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
            {
                var members = activeOnly
                    ? group.Where(r => string.Equals(r.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    : group.AsEnumerable();

                var ordered = members.OrderBy(r => r.Version, VersionComparer.Instance).ToList();
                if (ordered.Count == 0)
                    continue;

                var head = ordered[ordered.Count - 1];

                foreach (var row in ordered)
                {
                    row.VersionCount = ordered.Count;
                    row.IsGroupHead = row.ApplicationId == head.ApplicationId;
                    row.ShowAllVersions = showAllVersions;
                }

                if (showAllVersions)
                {
                    result.AddRange(ordered.OrderByDescending(r => r.Version, VersionComparer.Instance));
                }
                else
                {
                    head.GroupVersions = string.Join(", ", ordered
                        .Where(r => r.ApplicationId != head.ApplicationId)
                        .Select(r => string.IsNullOrWhiteSpace(r.Version) ? "(no version)" : r.Version));
                    result.Add(head);
                }
            }
            return result;
        }

        // GET: /Applications/Details/5
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var application = _applicationService.GetById(id.Value);
            if (application == null)
                return HttpNotFound();

            var otherVersions = _applicationService.GetVersionsByName(application.Name)
                .Where(v => v.ApplicationId != id.Value)
                .OrderBy(v => v.Version, VersionComparer.Instance)
                .Select(v => new VersionLinkViewModel { ApplicationId = v.ApplicationId, Version = v.Version })
                .ToList();

            var model = new ApplicationDetailViewModel
            {
                Application = application,
                UpstreamDependencies = BuildDependencyEntries(_dependencyService.GetChain(id.Value, 10, "Upstream"), isUpstream: true, id.Value),
                DownstreamDependencies = BuildDependencyEntries(_dependencyService.GetChain(id.Value, 10, "Downstream"), isUpstream: false, id.Value),
                CanEdit = IsEditor(),
                CanDelete = _applicationService.CanDelete(id.Value),
                DependencyCount = _dependencyService.GetForApplication(id.Value).Count(),
                OtherVersions = otherVersions,
                Properties = BuildPropertyViewModels(id.Value).ToList(),
                Technologies = _technologyService.GetForApplication(id.Value).ToList(),
                Dlls = _dllService.GetForApplication(id.Value).Select(d => new ApplicationDllViewModel
                {
                    FileName = d.FileName,
                    Version = d.Version,
                    RelativePath = d.RelativePath
                }).ToList()
            };

            return View(model);
        }

        // POST: /Applications/ScanDlls/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult ScanDlls(int id)
        {
            var application = _applicationService.GetById(id);
            if (application == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(application.SourcePath))
            {
                TempData["ErrorMessage"] = "This application has no source path to scan. Set one on the Edit form first.";
                return RedirectToAction("Details", new { id });
            }

            var dlls = _discoveryService.GetDllFiles(application.SourcePath);
            var count = _dllService.SyncFromDiscovery(id, application.Name, dlls, _roleProvider.CurrentUserFullName);

            TempData["SuccessMessage"] = count == 0
                ? "No DLLs were found under '" + application.SourcePath + "'."
                : "Scanned " + count + " DLL(s) from '" + application.SourcePath + "'.";
            return RedirectToAction("Details", new { id });
        }

        // GET: /Applications/Create
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Create()
        {
            var model = new ApplicationFormViewModel
            {
                EnvironmentOptions = ApplicationViewModelFactory.EnvironmentOptions(null),
                CriticalityOptions = ApplicationViewModelFactory.CriticalityOptions(null),
                StatusOptions = ApplicationViewModelFactory.StatusOptions("Active"),
                CategoryOptions = CategoryOptions(null),
                TechnologyOptions = TechnologyOptions(null),
                SelectedTechnologyIds = new int[0]
            };
            return View(model);
        }

        // POST: /Applications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Create(ApplicationFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var application = new Application
                    {
                        Name = model.Name,
                        Version = model.Version,
                        Description = model.Description,
                        BusinessGroup = model.BusinessGroup,
                        BusinessOwner = model.BusinessOwner,
                        BusinessOwnerEmail = model.BusinessOwnerEmail,
                        BusinessBackup = model.BusinessBackup,
                        BusinessBackupEmail = model.BusinessBackupEmail,
                        TechnicalOwner = model.TechnicalOwner,
                        TechnicalOwnerEmail = model.TechnicalOwnerEmail,
                        CategoryId = model.CategoryId,
                        Environment = model.Environment,
                        CriticalityLevel = model.CriticalityLevel,
                        DefaultDependencyCriticality = model.DefaultDependencyCriticality,
                        DefaultDependencyImpact = model.DefaultDependencyImpact,
                        Status = string.IsNullOrEmpty(model.Status) ? "Active" : model.Status,
                        ExternalUrl = model.ExternalUrl,
                        SourcePath = model.SourcePath,
                        ApplicationIDE = model.ApplicationIDE,
                        FrameworkVersion = model.FrameworkVersion,
                        DocumentationLink = model.DocumentationLink,
                        SourceControlLocation = model.SourceControlLocation,
                        HoursOfOperation = model.HoursOfOperation,
                        MaintenanceWindow = model.MaintenanceWindow,
                        MaintenanceNotificationEmail = model.MaintenanceNotificationEmail,
                        ApplicationSummary = model.ApplicationSummary,
                        TriageSteps = model.TriageSteps
                    };

                    _applicationService.Create(application, _roleProvider.CurrentUserFullName);
                    _technologyService.SetForApplication(application.ApplicationId, model.SelectedTechnologyIds, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Application '{application.Name}' created successfully.";
                    return RedirectToAction("Details", new { id = application.ApplicationId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", FriendlySaveErrorMessage(ex));
                }
            }

            model.EnvironmentOptions = ApplicationViewModelFactory.EnvironmentOptions(model.Environment);
            model.CriticalityOptions = ApplicationViewModelFactory.CriticalityOptions(model.CriticalityLevel);
            model.StatusOptions = ApplicationViewModelFactory.StatusOptions(model.Status);
            model.CategoryOptions = CategoryOptions(model.CategoryId);
            model.TechnologyOptions = TechnologyOptions(null);
            return View(model);
        }

        // GET: /Applications/Edit/5
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var application = _applicationService.GetById(id.Value);
            if (application == null)
                return HttpNotFound();

            var model = new ApplicationFormViewModel
            {
                ApplicationId = application.ApplicationId,
                Name = application.Name,
                Version = application.Version,
                Description = application.Description,
                BusinessGroup = application.BusinessGroup,
                BusinessOwner = application.BusinessOwner,
                BusinessOwnerEmail = application.BusinessOwnerEmail,
                BusinessBackup = application.BusinessBackup,
                BusinessBackupEmail = application.BusinessBackupEmail,
                TechnicalOwner = application.TechnicalOwner,
                TechnicalOwnerEmail = application.TechnicalOwnerEmail,
                CategoryId = application.CategoryId,
                Environment = application.Environment,
                CriticalityLevel = application.CriticalityLevel,
                DefaultDependencyCriticality = application.DefaultDependencyCriticality,
                DefaultDependencyImpact = application.DefaultDependencyImpact,
                Status = application.Status,
                ExternalUrl = application.ExternalUrl,
                SourcePath = application.SourcePath,
                ApplicationIDE = application.ApplicationIDE,
                FrameworkVersion = application.FrameworkVersion,
                DocumentationLink = application.DocumentationLink,
                SourceControlLocation = application.SourceControlLocation,
                HoursOfOperation = application.HoursOfOperation,
                MaintenanceWindow = application.MaintenanceWindow,
                MaintenanceNotificationEmail = application.MaintenanceNotificationEmail,
                ApplicationSummary = application.ApplicationSummary,
                TriageSteps = application.TriageSteps,
                EnvironmentOptions = ApplicationViewModelFactory.EnvironmentOptions(application.Environment),
                CriticalityOptions = ApplicationViewModelFactory.CriticalityOptions(application.CriticalityLevel),
                StatusOptions = ApplicationViewModelFactory.StatusOptions(application.Status),
                CategoryOptions = CategoryOptions(application.CategoryId),
                TechnologyOptions = TechnologyOptions(null),
                SelectedTechnologyIds = _technologyService.GetForApplication(application.ApplicationId)
                    .Select(t => t.TechnologyId)
                    .ToList()
            };

            model.Properties = BuildPropertyViewModels(application.ApplicationId).ToList();
            return View(model);
        }

        // POST: /Applications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Edit(ApplicationFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var application = new Application
                    {
                        ApplicationId = model.ApplicationId,
                        Name = model.Name,
                        Version = model.Version,
                        Description = model.Description,
                        BusinessGroup = model.BusinessGroup,
                        BusinessOwner = model.BusinessOwner,
                        BusinessOwnerEmail = model.BusinessOwnerEmail,
                        BusinessBackup = model.BusinessBackup,
                        BusinessBackupEmail = model.BusinessBackupEmail,
                        TechnicalOwner = model.TechnicalOwner,
                        TechnicalOwnerEmail = model.TechnicalOwnerEmail,
                        CategoryId = model.CategoryId,
                        Environment = model.Environment,
                        CriticalityLevel = model.CriticalityLevel,
                        DefaultDependencyCriticality = model.DefaultDependencyCriticality,
                        DefaultDependencyImpact = model.DefaultDependencyImpact,
                        Status = model.Status,
                        ExternalUrl = model.ExternalUrl,
                        SourcePath = model.SourcePath,
                        ApplicationIDE = model.ApplicationIDE,
                        FrameworkVersion = model.FrameworkVersion,
                        DocumentationLink = model.DocumentationLink,
                        SourceControlLocation = model.SourceControlLocation,
                        HoursOfOperation = model.HoursOfOperation,
                        MaintenanceWindow = model.MaintenanceWindow,
                        MaintenanceNotificationEmail = model.MaintenanceNotificationEmail,
                        ApplicationSummary = model.ApplicationSummary,
                        TriageSteps = model.TriageSteps
                    };

                    _applicationService.Update(application, _roleProvider.CurrentUserFullName);
                    _technologyService.SetForApplication(application.ApplicationId, model.SelectedTechnologyIds, _roleProvider.CurrentUserFullName);
                    SavePropertyValues(model);
                    TempData["SuccessMessage"] = $"Application '{application.Name}' updated successfully.";
                    return RedirectToAction("Details", new { id = application.ApplicationId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", FriendlySaveErrorMessage(ex));
                }
            }

            model.EnvironmentOptions = ApplicationViewModelFactory.EnvironmentOptions(model.Environment);
            model.CriticalityOptions = ApplicationViewModelFactory.CriticalityOptions(model.CriticalityLevel);
            model.StatusOptions = ApplicationViewModelFactory.StatusOptions(model.Status);
            model.CategoryOptions = CategoryOptions(model.CategoryId);
            model.TechnologyOptions = TechnologyOptions(null);
            return View(model);
        }

        // GET: /Applications/Delete/5
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var application = _applicationService.GetById(id.Value);
            if (application == null)
                return HttpNotFound();

            var canDelete = _applicationService.CanDelete(id.Value);

            var model = new DeleteApplicationViewModel
            {
                ApplicationId = application.ApplicationId,
                Name = application.Name,
                Environment = application.Environment,
                CriticalityLevel = application.CriticalityLevel,
                DependencyCount = _dependencyService.GetForApplication(id.Value).Count(),
                CanDelete = canDelete,
                IsDeleted = application.IsDeleted
            };
            return View(model);
        }

        // POST: /Applications/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Delete(int? id, bool permanent)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var application = _applicationService.GetById(id.Value);
            if (application == null)
                return HttpNotFound();

            if (permanent)
            {
                if (!_applicationService.CanDelete(id.Value))
                {
                    TempData["ErrorMessage"] = "This application has dependencies and cannot be permanently deleted. Remove dependencies first, or use soft delete.";
                    return RedirectToAction("Details", new { id });
                }

                _applicationService.HardDelete(id.Value, _roleProvider.CurrentUserFullName);
                TempData["SuccessMessage"] = "Application permanently deleted.";
            }
            else
            {
                _applicationService.SoftDelete(id.Value, _roleProvider.CurrentUserFullName);
                TempData["SuccessMessage"] = "Application hidden from the registry. It can be restored from its details page.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Applications/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdAuthorize(Roles = "Maintenance,Admin")]
        public ActionResult Restore(int id)
        {
            var application = _applicationService.Restore(id, _roleProvider.CurrentUserFullName);
            if (application == null)
                return HttpNotFound();

            TempData["SuccessMessage"] = $"Application '{application.Name}' restored.";
            return RedirectToAction("Details", new { id });
        }

        // POST: /Applications/ValidateName
        [HttpPost]
        public JsonResult ValidateName(string name, int applicationId = 0, string version = null)
        {
            var normalizedVersion = string.IsNullOrWhiteSpace(version) ? null : version.Trim();
            var existing = _applicationService.GetAllIncludingDeleted().FirstOrDefault(a =>
                string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Version ?? null, normalizedVersion, StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.ApplicationId != applicationId)
                return Json("An application with this name and version already exists.");

            return Json(true);
        }

        private bool IsEditor()
        {
            return _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleMaintenance) ||
                   _roleProvider.IsCurrentUserInRole(RoleProviderBase.RoleAdmin);
        }

        private IEnumerable<SelectListItem> CategoryOptions(int? selected)
        {
            var active = _categoryService.GetActiveCategories().ToList();
            foreach (var c in active)
                yield return new SelectListItem { Text = c.Name, Value = c.CategoryId.ToString(), Selected = c.CategoryId == selected };

            if (selected.HasValue && !active.Any(c => c.CategoryId == selected.Value))
            {
                var category = _categoryService.GetById(selected.Value);
                if (category != null)
                    yield return new SelectListItem { Text = category.Name + " (inactive)", Value = category.CategoryId.ToString(), Selected = true };
            }
        }

        private IEnumerable<SelectListItem> TechnologyOptions(int? selected)
        {
            var active = _technologyService.GetActiveTechnologies().ToList();
            foreach (var t in active)
                yield return new SelectListItem { Text = t.Name, Value = t.TechnologyId.ToString(), Selected = t.TechnologyId == selected };

            if (selected.HasValue && !active.Any(t => t.TechnologyId == selected.Value))
            {
                var technology = _technologyService.GetById(selected.Value);
                if (technology != null)
                    yield return new SelectListItem { Text = technology.Name + " (inactive)", Value = technology.TechnologyId.ToString(), Selected = true };
            }
        }

        private static IEnumerable<SelectListItem> DllOperatorOptions(string selected)
        {
            yield return new SelectListItem { Text = "Any version", Value = "", Selected = string.IsNullOrEmpty(selected) };
            yield return new SelectListItem { Text = "= equals", Value = "=", Selected = selected == "=" };
            yield return new SelectListItem { Text = "> newer than", Value = ">", Selected = selected == ">" };
            yield return new SelectListItem { Text = ">= at least", Value = ">=", Selected = selected == ">=" };
            yield return new SelectListItem { Text = "< older than", Value = "<", Selected = selected == "<" };
            yield return new SelectListItem { Text = "<= at most", Value = "<=", Selected = selected == "<=" };
        }

        /// <summary>
        /// Maps a save failure to a user-friendly message. The (Name, Version) unique
        /// constraint surfaces as a generic EF update error; detect it and explain it.
        /// </summary>
        private static string FriendlySaveErrorMessage(Exception ex)
        {
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current.Message.IndexOf("UQ_Applications_Name_Version", StringComparison.OrdinalIgnoreCase) >= 0
                    || (current.Message.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0
                        && current.Message.IndexOf("Application", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return "An application with this name and version already exists.";
                }
            }

            return ex.Message;
        }

        /// <summary>
        /// Flattens the transitive chain (from GetDependencyChain) into the upstream/downstream
        /// list. Direct edges (depth 1) are listed individually so each can be removed; indirect
        /// entries (depth &gt; 1) are collapsed per application at their minimum depth. An
        /// application that is already reached directly is not repeated as an indirect row.
        /// Each indirect entry also carries its Chain - the shortest path of applications
        /// from the viewed root through to the indirect app (excluding the root itself).
        /// </summary>
        private static List<DependencyListEntryViewModel> BuildDependencyEntries(
            IEnumerable<DependencyChainResult> chain, bool isUpstream, int rootApplicationId)
        {
            var rows = chain.OrderBy(r => r.Depth).ToList();
            var entries = new List<DependencyListEntryViewModel>();

            int NeighborId(DependencyChainResult r) => isUpstream ? r.TargetApplicationId : r.SourceApplicationId;
            string NeighborName(DependencyChainResult r) => isUpstream ? r.TargetName : r.SourceName;
            string NeighborCategory(DependencyChainResult r) => isUpstream ? r.TargetCategory : r.SourceCategory;

            // For each application, remember the parent (and name) from the lowest-depth
            // row that reaches it, so we can reconstruct the shortest path to the root.
            var parentOf = new Dictionary<int, int>();
            var nameOf = new Dictionary<int, string>();
            foreach (var row in rows)
            {
                int childId = isUpstream ? row.TargetApplicationId : row.SourceApplicationId;
                int parentId = isUpstream ? row.SourceApplicationId : row.TargetApplicationId;
                if (!parentOf.ContainsKey(childId))
                {
                    parentOf[childId] = parentId;
                    nameOf[childId] = NeighborName(row);
                }
            }

            List<DependencyPathStepViewModel> BuildChain(int applicationId)
            {
                var steps = new List<DependencyPathStepViewModel>();
                var visited = new HashSet<int>();
                int current = applicationId;
                while (current != rootApplicationId)
                {
                    // Guard against dependency cycles: the parent map can loop
                    // back to an already-visited application, which would otherwise
                    // hang the request.
                    if (!visited.Add(current))
                        break;

                    string name;
                    int parentId;
                    if (!nameOf.TryGetValue(current, out name) || !parentOf.TryGetValue(current, out parentId))
                        break;
                    steps.Add(new DependencyPathStepViewModel { ApplicationId = current, Name = name });
                    current = parentId;
                }
                steps.Reverse();
                return steps;
            }

            var directNeighbors = rows.Where(r => r.Depth == 1).Select(NeighborId).ToHashSet();

            foreach (var row in rows.Where(r => r.Depth == 1))
            {
                entries.Add(new DependencyListEntryViewModel
                {
                    ApplicationId = NeighborId(row),
                    Name = NeighborName(row),
                    Category = NeighborCategory(row),
                    Depth = 1,
                    IsDirect = true,
                    DependencyTypes = row.DependencyType,
                    CriticalityLevel = row.CriticalityLevel,
                    Impact = row.Impact,
                    Frequency = row.Frequency,
                    DependencyId = row.DependencyId
                });
            }

            foreach (var group in rows
                .Where(r => r.Depth > 1 && !directNeighbors.Contains(NeighborId(r)))
                .GroupBy(NeighborId))
            {
                entries.Add(new DependencyListEntryViewModel
                {
                    ApplicationId = group.Key,
                    Name = NeighborName(group.First()),
                    Category = NeighborCategory(group.First()),
                    Depth = group.Min(r => r.Depth),
                    IsDirect = false,
                    DependencyTypes = string.Join(", ", group.Select(r => r.DependencyType).Distinct().OrderBy(t => t)),
                    CriticalityLevel = group.OrderBy(r => LevelRank(r.CriticalityLevel)).FirstOrDefault()?.CriticalityLevel,
                    Impact = group.OrderBy(r => LevelRank(r.CriticalityLevel)).FirstOrDefault()?.Impact,
                    Frequency = null,
                    DependencyId = 0,
                    Chain = BuildChain(group.Key)
                });
            }

            return entries
                .OrderBy(e => e.Depth)
                .ThenBy(e => e.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private IEnumerable<ApplicationPropertyViewModel> BuildPropertyViewModels(int applicationId)
        {
            var values = _propertyService.GetValuesForApplication(applicationId).ToList();

            return _propertyService.GetActiveDefinitions()
                .Select(d => new ApplicationPropertyViewModel
                {
                    PropertyDefinitionId = d.PropertyDefinitionId,
                    Key = d.Key,
                    Label = d.Label,
                    DataType = d.DataType,
                    Value = values.FirstOrDefault(v => v.PropertyDefinitionId == d.PropertyDefinitionId)?.Value,
                    ScanPattern = d.ScanPattern
                })
                .ToList();
        }

        private static int LevelRank(string level)
        {
            switch (level)
            {
                case "Critical": return 0;
                case "High": return 1;
                case "Medium": return 2;
                case "Low": return 3;
                default: return 4;
            }
        }

        private void SavePropertyValues(ApplicationFormViewModel model)
        {
            if (model.Properties == null || model.Properties.Count == 0)
                return;

            var values = model.Properties
                .Where(p => p.PropertyDefinitionId > 0)
                .GroupBy(p => p.PropertyDefinitionId)
                .ToDictionary(g => g.Key, g => g.First().Value);

            _propertyService.SaveValuesForApplication(model.ApplicationId, values, _roleProvider.CurrentUserFullName);
        }
    }
}
