using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DependencyTracker.Data.Models;

namespace DependencyTracker.Data.Services
{
    /// <summary>
    /// A candidate dependency produced by scanning a configuration file. One candidate
    /// is produced per (config entry, matched property definition) pair.
    /// </summary>
    public class ConfigScanCandidate
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsUrl { get; set; }
        public ApplicationPropertyDefinition MatchedDefinition { get; set; }
        public string MatchedValue { get; set; }
        public string SuggestedDependencyType { get; set; }
        public int? SuggestedTargetApplicationId { get; set; }
        public string SuggestedTargetName { get; set; }
    }

    /// <summary>
    /// Parses web.config/app.config XML, runs the active property definition scan
    /// patterns (regular expressions) against the extracted keys and values, and
    /// suggests target applications by matching the captured value against the stored
    /// property values of every tracked application.
    /// </summary>
    public class ConfigScannerService : IConfigScannerService
    {
        private static readonly HashSet<string> KnownSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "configSections", "connectionStrings", "appSettings", "startup", "runtime", "mscorlib",
            "system.web", "system.webServer", "system.web.extensions", "system.web.deployment",
            "system.codedom", "system.data", "system.diagnostics", "system.serviceModel",
            "system.runtime.serialization", "entityFramework", "system.net", "system.transactions",
            "system.data.dataset", "location", "system.web.routehandlers"
        };

        public IEnumerable<ConfigScanCandidate> Scan(
            string configXml,
            IEnumerable<ApplicationPropertyDefinition> activeDefinitions,
            IEnumerable<ApplicationPropertyValue> allPropertyValues,
            IEnumerable<Application> applications)
        {
            var candidates = new List<ConfigScanCandidate>();
            if (string.IsNullOrWhiteSpace(configXml))
                return candidates;

            var definitions = (activeDefinitions ?? Enumerable.Empty<ApplicationPropertyDefinition>())
                .Where(d => d != null && !string.IsNullOrWhiteSpace(d.ScanPattern))
                .ToList();
            if (definitions.Count == 0)
                return candidates;

            var document = XDocument.Parse(configXml);

            var values = (allPropertyValues ?? Enumerable.Empty<ApplicationPropertyValue>())
                .Where(v => v != null)
                .ToList();
            var appsById = (applications ?? Enumerable.Empty<Application>())
                .Where(a => a != null)
                .ToDictionary(a => a.ApplicationId);

            foreach (var entry in ExtractEntries(document.Root))
            {
                foreach (var definition in definitions)
                {
                    Regex regex;
                    try
                    {
                        regex = new Regex(definition.ScanPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }

                    var keyMatch = regex.Match(entry.Key ?? string.Empty);
                    var valueMatch = regex.Match(entry.Value ?? string.Empty);
                    if (!keyMatch.Success && !valueMatch.Success)
                        continue;

                    var matchedText = keyMatch.Success
                        ? entry.Value ?? string.Empty
                        : valueMatch.Value;

                    var target = FindTarget(values, definition.PropertyDefinitionId, matchedText, entry.Value);

                    candidates.Add(new ConfigScanCandidate
                    {
                        Section = entry.Section,
                        Key = entry.Key,
                        Value = entry.Value,
                        IsUrl = entry.IsUrl,
                        MatchedDefinition = definition,
                        MatchedValue = matchedText,
                        SuggestedDependencyType = entry.DefaultType,
                        SuggestedTargetApplicationId = target?.ApplicationId,
                        SuggestedTargetName = target != null && appsById.TryGetValue(target.ApplicationId, out var app)
                            ? app.Name
                            : null
                    });
                }
            }

            return candidates;
        }

        private static ApplicationPropertyValue FindTarget(
            List<ApplicationPropertyValue> values,
            int definitionId,
            string matchedText,
            string entryValue)
        {
            var pool = values
                .Where(v => v.PropertyDefinitionId == definitionId && !string.IsNullOrWhiteSpace(v.Value))
                .ToList();
            if (pool.Count == 0)
                return null;

            var hit = pool.FirstOrDefault(v =>
                string.Equals(v.Value, entryValue, StringComparison.OrdinalIgnoreCase));
            if (hit != null)
                return hit;

            hit = pool.FirstOrDefault(v =>
                entryValue != null && entryValue.IndexOf(v.Value, StringComparison.OrdinalIgnoreCase) >= 0);
            if (hit != null)
                return hit;

            if (!string.IsNullOrWhiteSpace(matchedText))
                hit = pool.FirstOrDefault(v =>
                    string.Equals(v.Value, matchedText, StringComparison.OrdinalIgnoreCase));

            return hit;
        }

        private static IEnumerable<ConfigEntry> ExtractEntries(XElement root)
        {
            var entries = new List<ConfigEntry>();

            var connectionStrings = root.Element("connectionStrings");
            if (connectionStrings != null)
            {
                foreach (var add in connectionStrings.Elements("add"))
                    entries.Add(new ConfigEntry
                    {
                        Section = "connectionStrings",
                        Key = (string)add.Attribute("name") ?? string.Empty,
                        Value = (string)add.Attribute("connectionString") ?? string.Empty,
                        DefaultType = "Database"
                    });
            }

            var appSettings = root.Element("appSettings");
            if (appSettings != null)
            {
                foreach (var add in appSettings.Elements("add"))
                    entries.Add(new ConfigEntry
                    {
                        Section = "appSettings",
                        Key = (string)add.Attribute("key") ?? string.Empty,
                        Value = (string)add.Attribute("value") ?? string.Empty,
                        DefaultType = "API"
                    });
            }

            foreach (var section in root.Elements())
            {
                if (KnownSections.Contains(section.Name.LocalName))
                    continue;

                foreach (var setting in section.Descendants("setting"))
                {
                    var name = (string)setting.Attribute("name") ?? setting.Name.LocalName;
                    var valueElement = setting.Element("value");
                    entries.Add(new ConfigEntry
                    {
                        Section = section.Name.LocalName,
                        Key = name,
                        Value = valueElement == null ? string.Empty : valueElement.Value,
                        DefaultType = "API"
                    });
                }
            }

            foreach (var entry in entries)
            {
                if (LooksLikeUrl(entry.Value))
                    entry.IsUrl = true;
            }

            return entries;
        }

        private static bool LooksLikeUrl(string value)
        {
            return value != null
                && (value.IndexOf("http://", StringComparison.OrdinalIgnoreCase) >= 0
                    || value.IndexOf("https://", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private sealed class ConfigEntry
        {
            public string Section { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public string DefaultType { get; set; }
            public bool IsUrl { get; set; }
        }
    }
}
