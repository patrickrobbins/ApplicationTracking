using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DependencyTracker.Web.Models.ViewModels
{
    public class ConfigScannerViewModel
    {
        [Required(ErrorMessage = "Select the application whose configuration is being scanned.")]
        public int SourceApplicationId { get; set; }

        [Required(ErrorMessage = "Paste the configuration XML to scan.")]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string ConfigXml { get; set; }

        public bool Scanned { get; set; }
        public string SourceApplicationName { get; set; }
        public List<ConfigScanCandidateViewModel> Candidates { get; set; }

        public IEnumerable<SelectListItem> ApplicationOptions { get; set; }
        public IEnumerable<SelectListItem> TypeOptions { get; set; }
        public IEnumerable<SelectListItem> DirectionOptions { get; set; }
        public IEnumerable<SelectListItem> CriticalityOptions { get; set; }
    }

    public class ConfigScanCandidateViewModel
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsUrl { get; set; }
        public string MatchedDefinition { get; set; }
        public string MatchedPattern { get; set; }
        public string MatchedValue { get; set; }
        public string SuggestedTargetName { get; set; }

        public bool Include { get; set; }
        public int TargetApplicationId { get; set; }
        public string DependencyType { get; set; }
        public string CriticalityLevel { get; set; }
        public string Direction { get; set; }
    }
}
