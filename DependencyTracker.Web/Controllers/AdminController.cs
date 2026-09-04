using System;
using System.Linq;
using System.Web.Mvc;
using DependencyTracker.Data.Models;
using DependencyTracker.Data.Services;
using DependencyTracker.Web.Models;
using DependencyTracker.Web.Models.ViewModels;

namespace DependencyTracker.Web.Controllers
{
    [AdAuthorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IRoleProvider _roleProvider;
        private readonly IApplicationPropertyService _propertyService;
        private readonly IApplicationCategoryService _categoryService;
        private readonly IApplicationTechnologyService _technologyService;

        public AdminController(IAdminService adminService, IRoleProvider roleProvider,
            IApplicationPropertyService propertyService, IApplicationCategoryService categoryService,
            IApplicationTechnologyService technologyService)
        {
            _adminService = adminService;
            _roleProvider = roleProvider;
            _propertyService = propertyService;
            _categoryService = categoryService;
            _technologyService = technologyService;
        }

        // GET: /Admin
        public ActionResult Index()
        {
            var groups = _adminService.GetAllGroups().ToList();

            var model = new AdminIndexViewModel
            {
                Groups = groups,
                RoleAssignments = _adminService.GetRoleAssignments(),
                RecentActivity = _adminService.GetActivityLog(10).ToList(),
                TotalGroups = groups.Count,
                ActiveGroups = groups.Count(g => g.IsActive)
            };
            return View(model);
        }

        // GET: /Admin/AdGroups
        public ActionResult AdGroups()
        {
            var groups = _adminService.GetAllGroups().OrderBy(g => g.Role).ThenBy(g => g.GroupName).ToList();
            return View(groups);
        }

        // GET: /Admin/AdGroup/Create
        public ActionResult AdGroupCreate()
        {
            var model = new AdGroupViewModel
            {
                IsActive = true,
                RoleOptions = AdminViewModelFactory.RoleOptions(null)
            };
            return View(model);
        }

        // POST: /Admin/AdGroup/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdGroupCreate(AdGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = new AdGroup
                    {
                        GroupName = model.GroupName,
                        Role = model.Role,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };
                    _adminService.CreateGroup(group, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"AD group '{group.GroupName}' added.";
                    return RedirectToAction("AdGroups");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            model.RoleOptions = AdminViewModelFactory.RoleOptions(model.Role);
            return View(model);
        }

        // GET: /Admin/AdGroup/Edit/5
        public ActionResult AdGroupEdit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var group = _adminService.GetGroupById(id.Value);
            if (group == null)
                return HttpNotFound();

            var model = new AdGroupViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Role = group.Role,
                Description = group.Description,
                IsActive = group.IsActive,
                RoleOptions = AdminViewModelFactory.RoleOptions(group.Role)
            };
            return View(model);
        }

        // POST: /Admin/AdGroup/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdGroupEdit(AdGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var group = new AdGroup
                    {
                        GroupId = model.GroupId,
                        GroupName = model.GroupName,
                        Role = model.Role,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };
                    _adminService.UpdateGroup(group, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"AD group '{group.GroupName}' updated.";
                    return RedirectToAction("AdGroups");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            model.RoleOptions = AdminViewModelFactory.RoleOptions(model.Role);
            return View(model);
        }

        // POST: /Admin/AdGroup/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdGroupToggle(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _adminService.ToggleGroupActive(id.Value, _roleProvider.CurrentUserFullName);
            return RedirectToAction("AdGroups");
        }

        // POST: /Admin/AdGroup/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdGroupDelete(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _adminService.DeleteGroup(id.Value, _roleProvider.CurrentUserFullName);
            TempData["SuccessMessage"] = "AD group removed.";
            return RedirectToAction("AdGroups");
        }

        // GET: /Admin/ActivityLog
        public ActionResult ActivityLog(string entityType, string action, string user)
        {
            var model = new ActivityLogViewModel
            {
                EntityType = entityType,
                Action = action,
                User = user,
                Entries = _adminService.SearchActivityLog(entityType, action, user).ToList()
            };
            return View(model);
        }

        // GET: /Admin/DatabaseInfo
        public ActionResult DatabaseInfo()
        {
            ViewBag.Server = GetServerInfo();
            return View();
        }

        // GET: /Admin/PropertyDefinitions
        public ActionResult PropertyDefinitions()
        {
            return View(_propertyService.GetDefinitions().ToList());
        }

        // GET: /Admin/PropertyDefinition/Create
        public ActionResult PropertyDefinitionCreate()
        {
            var model = new PropertyDefinitionViewModel
            {
                IsActive = true,
                DataTypeOptions = AdminViewModelFactory.DataTypeOptions(null)
            };
            return View(model);
        }

        // POST: /Admin/PropertyDefinition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PropertyDefinitionCreate(PropertyDefinitionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var definition = new ApplicationPropertyDefinition
                    {
                        Key = model.Key,
                        Label = model.Label,
                        DataType = model.DataType,
                        ScanPattern = model.ScanPattern,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        SortOrder = model.SortOrder
                    };
                    _propertyService.CreateDefinition(definition, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Property definition '{definition.Key}' added.";
                    return RedirectToAction("PropertyDefinitions");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            model.DataTypeOptions = AdminViewModelFactory.DataTypeOptions(model.DataType);
            return View(model);
        }

        // GET: /Admin/PropertyDefinition/Edit/5
        public ActionResult PropertyDefinitionEdit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var definition = _propertyService.GetDefinitionById(id.Value);
            if (definition == null)
                return HttpNotFound();

            var model = new PropertyDefinitionViewModel
            {
                PropertyDefinitionId = definition.PropertyDefinitionId,
                Key = definition.Key,
                Label = definition.Label,
                DataType = definition.DataType,
                ScanPattern = definition.ScanPattern,
                Description = definition.Description,
                IsActive = definition.IsActive,
                SortOrder = definition.SortOrder,
                DataTypeOptions = AdminViewModelFactory.DataTypeOptions(definition.DataType)
            };
            return View(model);
        }

        // POST: /Admin/PropertyDefinition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PropertyDefinitionEdit(PropertyDefinitionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var definition = new ApplicationPropertyDefinition
                    {
                        PropertyDefinitionId = model.PropertyDefinitionId,
                        Key = model.Key,
                        Label = model.Label,
                        DataType = model.DataType,
                        ScanPattern = model.ScanPattern,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        SortOrder = model.SortOrder
                    };
                    _propertyService.UpdateDefinition(definition, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Property definition '{definition.Key}' updated.";
                    return RedirectToAction("PropertyDefinitions");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            model.DataTypeOptions = AdminViewModelFactory.DataTypeOptions(model.DataType);
            return View(model);
        }

        // POST: /Admin/PropertyDefinition/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PropertyDefinitionToggle(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _propertyService.ToggleDefinitionActive(id.Value, _roleProvider.CurrentUserFullName);
            return RedirectToAction("PropertyDefinitions");
        }

        // POST: /Admin/PropertyDefinition/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PropertyDefinitionDelete(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _propertyService.DeleteDefinition(id.Value, _roleProvider.CurrentUserFullName);
            TempData["SuccessMessage"] = "Property definition removed.";
            return RedirectToAction("PropertyDefinitions");
        }

        // GET: /Admin/Categories
        public ActionResult Categories()
        {
            return View(_categoryService.GetCategories().ToList());
        }

        // GET: /Admin/Category/Create
        public ActionResult CategoryCreate()
        {
            var model = new CategoryViewModel { IsActive = true };
            return View(model);
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryCreate(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new ApplicationCategory
                    {
                        Name = model.Name,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        SortOrder = model.SortOrder
                    };
                    _categoryService.Create(category, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Category '{category.Name}' added.";
                    return RedirectToAction("Categories");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Admin/Category/Edit/5
        public ActionResult CategoryEdit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var category = _categoryService.GetById(id.Value);
            if (category == null)
                return HttpNotFound();

            var model = new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder
            };
            return View(model);
        }

        // POST: /Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryEdit(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new ApplicationCategory
                    {
                        CategoryId = model.CategoryId,
                        Name = model.Name,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        SortOrder = model.SortOrder
                    };
                    _categoryService.Update(category, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Category '{category.Name}' updated.";
                    return RedirectToAction("Categories");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        // POST: /Admin/Category/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryToggle(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _categoryService.ToggleActive(id.Value, _roleProvider.CurrentUserFullName);
            return RedirectToAction("Categories");
        }

        // POST: /Admin/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryDelete(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _categoryService.Delete(id.Value, _roleProvider.CurrentUserFullName);
            TempData["SuccessMessage"] = "Category removed; affected applications are now uncategorized.";
            return RedirectToAction("Categories");
        }

        // GET: /Admin/Technologies
        public ActionResult Technologies()
        {
            return View(_technologyService.GetTechnologies().ToList());
        }

        // GET: /Admin/Technology/Create
        public ActionResult TechnologyCreate()
        {
            var model = new TechnologyViewModel { IsActive = true };
            return View(model);
        }

        // POST: /Admin/Technology/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TechnologyCreate(TechnologyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var technology = new ApplicationTechnology
                    {
                        Name = model.Name,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        SortOrder = model.SortOrder
                    };
                    _technologyService.Create(technology, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Technology '{technology.Name}' added.";
                    return RedirectToAction("Technologies");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Admin/Technology/Edit/5
        public ActionResult TechnologyEdit(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            var technology = _technologyService.GetById(id.Value);
            if (technology == null)
                return HttpNotFound();

            var model = new TechnologyViewModel
            {
                TechnologyId = technology.TechnologyId,
                Name = technology.Name,
                Description = technology.Description,
                IsActive = technology.IsActive,
                SortOrder = technology.SortOrder
            };
            return View(model);
        }

        // POST: /Admin/Technology/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TechnologyEdit(TechnologyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var technology = new ApplicationTechnology
                    {
                        TechnologyId = model.TechnologyId,
                        Name = model.Name,
                        Description = model.Description,
                        IsActive = model.IsActive,
                        SortOrder = model.SortOrder
                    };
                    _technologyService.Update(technology, _roleProvider.CurrentUserFullName);
                    TempData["SuccessMessage"] = $"Technology '{technology.Name}' updated.";
                    return RedirectToAction("Technologies");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        // POST: /Admin/Technology/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TechnologyToggle(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _technologyService.ToggleActive(id.Value, _roleProvider.CurrentUserFullName);
            return RedirectToAction("Technologies");
        }

        // POST: /Admin/Technology/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TechnologyDelete(int? id)
        {
            if (!id.HasValue)
                return HttpNotFound();

            _technologyService.Delete(id.Value, _roleProvider.CurrentUserFullName);
            TempData["SuccessMessage"] = "Technology removed; applications no longer carry the tag.";
            return RedirectToAction("Technologies");
        }

        private string GetServerInfo()
        {
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["DependencyTracker"].ConnectionString))
                {
                    conn.Open();
                    return $"{conn.DataSource} / {conn.Database}";
                }
            }
            catch
            {
                return "Unable to connect to database.";
            }
        }
    }
}
