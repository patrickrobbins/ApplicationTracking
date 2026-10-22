using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace DependencyTracker.Data.Services
{
    /// <summary>
    /// XML parsing helpers used for pasted and discovered configuration files.
    /// Document type definitions, external entity resolution and entity expansion
    /// are disabled so a configuration file cannot be used to read files from the
    /// server or to make the parser consume unbounded amounts of memory.
    /// </summary>
    public static class SafeXml
    {
        /// <summary>Maximum number of characters accepted from a single document.</summary>
        public const int MaxCharactersInDocument = 4 * 1024 * 1024;

        private static XmlReaderSettings CreateSettings()
        {
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersFromEntities = 0,
                MaxCharactersInDocument = MaxCharactersInDocument,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                CloseInput = true
            };
        }

        /// <summary>
        /// Parses XML text. Throws <see cref="XmlException"/> when the text is not
        /// valid XML or contains a document type definition.
        /// </summary>
        public static XDocument Parse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            using (var stringReader = new StringReader(xml))
            using (var reader = XmlReader.Create(stringReader, CreateSettings()))
            {
                return XDocument.Load(reader);
            }
        }

        /// <summary>
        /// Loads an XML file. Throws <see cref="XmlException"/> when the file is not
        /// valid XML or contains a document type definition.
        /// </summary>
        public static XDocument Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            using (var reader = XmlReader.Create(path, CreateSettings()))
            {
                return XDocument.Load(reader);
            }
        }
    }
}
