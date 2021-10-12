using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ArgumentsUtil;
using ANSIConsole;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel;

namespace Dog
{
    class Program
    {
        private static readonly bool DebugMode = true;

        private static readonly string ExeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        private static readonly string SyntaxDir = Path.Combine(ExeDir, "syntax");
        private static readonly string ConfigFile = Path.Combine(ExeDir, "config.json");
        private static readonly Version MinSupportedConfigVersion = new Version(1, 0);
        private static Config _config;

        static void Main(string[] args)
        {
            Arguments a = Arguments.Parse(args, (char)KeySelector.Linux);
            bool showLines = (a.ContainsKey("l") || a.ContainsKey("-lines")) && !a.ContainsKey("-no-lines");
            if (a.Keyless.Count > 0)
            {
                LoadConfig();
                foreach (string file in a.Keyless) PrettyPrint(file, showLines);
            }
        }

        static void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile));
                if (_config.Version < MinSupportedConfigVersion) throw new ArgumentOutOfRangeException($"Version", "Outdated configuration file");
            }
            else
            {
                _config = Config.Default;
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            Debug.Assert(_config != null);
        }

        static Syntax FindLanguageSyntax(string file)
        {
            string ext = Path.GetExtension(file).Remove(0,1); // Remove the .
            if (!_config.Syntaxes.ContainsKey(ext)) return Syntax.PlainText;
            string languageSyntaxName = _config.Syntaxes[ext];
            string languageSyntaxFile = Path.Combine(SyntaxDir, languageSyntaxName + ".yaml");
            if (!File.Exists(languageSyntaxFile)) throw new FileNotFoundException("Missing syntax file for language " + languageSyntaxName, languageSyntaxFile);

            using var reader = new StreamReader(languageSyntaxFile);
            var yaml = new YamlStream();
            yaml.Load(reader);
            Syntax languageSyntax = new Syntax();

            var properties = ((YamlMappingNode)yaml.Documents[0].RootNode).Children;
            foreach (var property in properties)
            {
                switch (((YamlScalarNode) property.Key).Value)
                {
                    case "name":
                    {
                        languageSyntax.name = property.Value.ToString();
                        break;
                    }
                    case "scope":
                    {
                        languageSyntax.scope = property.Value.ToString();
                        break;
                    }
                    case "variables":
                    {
                        languageSyntax.variables = new Dictionary<string, string>();
                        foreach (var child in ((YamlMappingNode) property.Value).Children)
                            languageSyntax.variables.Add(child.Key.ToString(), child.Value.ToString());
                        break;
                    }
                    case "contexts":
                    {
                        languageSyntax.variables = new Dictionary<string, string>();
                        break;
                    }
                }
            }

            return languageSyntax;
        }

        static void PrettyPrint(string file, bool showLines)
        {
            Syntax syntax = FindLanguageSyntax(file);
        }
    }
}
