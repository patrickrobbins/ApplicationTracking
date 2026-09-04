using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Web.Models.ViewModels
{
    public class AdGroupViewModel
    {
        public int GroupId { get; set; }

        [Required]
        [StringLength(200)]
        public string GroupName { get; set; }

        [Required]
        public string Role { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<SelectListItem> RoleOptions { get; set; }
    }

    public class AdminIndexViewModel
    {
        public IEnumerable<AdGroup> Groups { get; set; }
        public IDictionary<string, int> RoleAssignments { get; set; }
        public IEnumerable<ActivityLogEntry> RecentActivity { get; set; }
        public int TotalGroups { get; set; }
        public int ActiveGroups { get; set; }
    }

    public class ActivityLogViewModel
    {
        public IEnumerable<ActivityLogEntry> Entries { get; set; }
        public string EntityType { get; set; }
        public string Action { get; set; }
        public string User { get; set; }
    }

    public class PropertyDefinitionViewModel
    {
        public int PropertyDefinitionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; }

        [Required]
        [StringLength(200)]
        public string Label { get; set; }

        [Required]
        public string DataType { get; set; }

        [StringLength(500)]
        public string ScanPattern { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        public IEnumerable<SelectListItem> DataTypeOptions { get; set; }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }

    public class TechnologyViewModel
    {
        public int TechnologyId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }

    public static class AdminViewModelFactory
    {
        public static IEnumerable<SelectListItem> RoleOptions(string selected)
        {
            string[] roles = { "Admin", "Maintenance", "Viewer" };
            foreach (var r in roles)
                yield return new SelectListItem { Text = r, Value = r, Selected = r == selected };
        }

        public static IEnumerable<SelectListItem> DataTypeOptions(string selected)
        {
            string[] types = { "Text", "Url", "Multiline", "Number" };
            foreach (var t in types)
                yield return new SelectListItem { Text = t, Value = t, Selected = t == selected };
        }
    }
}
