using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Web.Models.ViewModels
{
    public class DependencyListItemViewModel
    {
        public int DependencyId { get; set; }
        public int SourceApplicationId { get; set; }
        public string SourceName { get; set; }
        public int TargetApplicationId { get; set; }
        public string TargetName { get; set; }
        public string DependencyType { get; set; }
        public string Direction { get; set; }
        public string CriticalityLevel { get; set; }
        public string Frequency { get; set; }
    }

    public class DependencyListViewModel
    {
        public IEnumerable<DependencyListItemViewModel> Dependencies { get; set; }
        public string DependencyType { get; set; }
        public string Criticality { get; set; }
        public int? SourceId { get; set; }
        public int? TargetId { get; set; }
        public IEnumerable<SelectListItem> TypeOptions { get; set; }
        public IEnumerable<SelectListItem> CriticalityOptions { get; set; }
        public IEnumerable<SelectListItem> SourceOptions { get; set; }
        public IEnumerable<SelectListItem> TargetOptions { get; set; }
        public bool CanEdit { get; set; }
    }

    public class DependencyFormViewModel
    {
        public int DependencyId { get; set; }

        [Required]
        public int SourceApplicationId { get; set; }

        [Required]
        public int TargetApplicationId { get; set; }

        [Required]
        public string DependencyType { get; set; }

        public string Direction { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(20)]
        public string CriticalityLevel { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        public string Impact { get; set; }

        public string Frequency { get; set; }

        public IEnumerable<SelectListItem> ApplicationOptions { get; set; }
        public IEnumerable<SelectListItem> TypeOptions { get; set; }
        public IEnumerable<SelectListItem> DirectionOptions { get; set; }
        public IEnumerable<SelectListItem> FrequencyOptions { get; set; }
        public IEnumerable<SelectListItem> CriticalityOptions { get; set; }
    }

    public static class DependencyViewModelFactory
    {
        public static IEnumerable<SelectListItem> TypeOptions(string selected)
        {
            string[] types = { "API", "Database", "File", "Message", "UI", "Infrastructure" };
            foreach (var t in types)
                yield return new SelectListItem { Text = t, Value = t, Selected = t == selected };
        }

        public static IEnumerable<SelectListItem> DirectionOptions(string selected)
        {
            string[] dirs = { "Upstream", "Downstream", "Bidirectional" };
            foreach (var d in dirs)
                yield return new SelectListItem { Text = d, Value = d, Selected = d == selected };
        }

        public static IEnumerable<SelectListItem> FrequencyOptions(string selected)
        {
            string[] freqs = { "Real-time", "Hourly", "Daily", "Weekly", "Monthly" };
            foreach (var f in freqs)
                yield return new SelectListItem { Text = f, Value = f, Selected = f == selected };
        }

        public static IEnumerable<SelectListItem> CriticalityOptions(string selected)
        {
            string[] levels = { "Critical", "High", "Medium", "Low" };
            foreach (var l in levels)
                yield return new SelectListItem { Text = l, Value = l, Selected = l == selected };
        }
    }
}
