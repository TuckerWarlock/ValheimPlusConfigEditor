using System.Collections.Generic;
using System.IO;

namespace ValheimConfigEditor
{
    public class ConfigParser
    {
        public Dictionary<string, Dictionary<string, string>> ConfigSections { get; private set; }

        public ConfigParser(string filePath)
        {
            ConfigSections = new Dictionary<string, Dictionary<string, string>>();
            ParseConfigFile(filePath);
        }

        private void ParseConfigFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            string currentSection = null;
            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(";") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Trim('[', ']');
                    if (!ConfigSections.ContainsKey(currentSection))
                        ConfigSections[currentSection] = new Dictionary<string, string>();
                }
                else if (trimmedLine.Contains("=") && currentSection != null)
                {
                    var parts = trimmedLine.Split('=');
                    if (parts.Length == 2)
                    {
                        ConfigSections[currentSection][parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
        }
    }
}
