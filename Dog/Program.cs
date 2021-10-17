using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ArgumentsUtil;
using ANSIConsole;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Dog
{
    class Program
    {
        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        private static readonly ISerializer YamlSerializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        private static readonly string ExeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        private static readonly string SyntaxDir = Path.Combine(ExeDir, "Syntaxes");
        private static readonly string ThemesDir = Path.Combine(ExeDir, "Themes");
        private static readonly string ConfigFile = Path.Combine(ExeDir, "config.yaml");
        private static readonly float MinSupportedConfigVersion = 1.0f;
        private static Config _config;

        static void Main(string[] args)
        {
            Arguments a = Arguments.Parse(args, (char)KeySelector.Linux);
            if (a.Keyless.Count > 0)
            {
                LoadConfig();
                bool showLines = (a.ContainsKey("l") || a.ContainsKey("-lines") || _config.showLines) && !a.ContainsKey("-no-lines");
                string themeName = a.ContainsKey("t") ? a["t"][0] : a.ContainsKey("-theme") ? a["-theme"][0] : _config.defaultTheme;
                Theme theme = FindTheme(themeName);
                foreach (string file in a.Keyless) PrettyPrint(file, showLines, theme);
            }
        }

        static void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                var yaml = File.ReadAllText(ConfigFile);
                _config = YamlDeserializer.Deserialize<Config>(yaml);
                if (_config.version < MinSupportedConfigVersion) throw new ArgumentOutOfRangeException($"Version", "Outdated configuration file");
            }
            else
            {
                _config = Config.Default;
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
                var yaml = YamlSerializer.Serialize(_config);
                File.WriteAllText(ConfigFile, YamlSerializer.Serialize(_config));
            }
            Debug.Assert(_config != null);
        }

        static Theme FindTheme(string name)
        {
            string themeFile = Path.Combine(ThemesDir, name + ".yaml");
            if (!File.Exists(themeFile)) throw new FileNotFoundException("Missing theme file " + name, themeFile);
            var yaml = File.ReadAllText(themeFile);
            return YamlDeserializer.Deserialize<Theme>(yaml);
        }
        static Syntax FindLanguageSyntax(string file)
        {
            string ext = Path.GetExtension(file).Remove(0,1); // Remove the .
            if (!_config.syntaxes.ContainsKey(ext)) return Syntax.PlainText;
            string languageSyntaxName = _config.syntaxes[ext];
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
                    case "variables":
                    {
                        languageSyntax.colorPatterns = new Dictionary<string, Syntax.Unit>();
                        foreach (var child in ((YamlMappingNode) property.Value).Children)
                            languageSyntax.colorPatterns.Add(child.Value.ToString(), DeduceUnitType(child.Value.ToString()));
                        break;
                    }
                    case "contexts":
                    {
                        // TODO: Implement this
                        break;
                    }
                }
            }

            return languageSyntax;
        }

        static Syntax.Unit DeduceUnitType(string name)
        {
            name = name.ToLower();
            if (name.Contains("digit") || name.Contains("integer") || name.Contains("float"))
                return Syntax.Unit.Numerical;
            if (name.Contains("char"))
                return Syntax.Unit.Character;
            if (name.Contains("text") || name.Contains("string"))
                return Syntax.Unit.String;
            if (name.Contains("reserved") || name.Contains("illegal_names"))
                return Syntax.Unit.Keyword;
            if (name.Contains("primitive") || name.Contains("base_type"))
                return Syntax.Unit.Primitive;
            if (name.Contains("bracket") || name.Contains("parenthesis"))
                return Syntax.Unit.Parenthesis;
            return Syntax.Unit.Unknown;
        }

        static void PrettyPrint(string file, bool showLines, Theme theme)
        {
            Syntax syntax = FindLanguageSyntax(file);
            using var reader = File.OpenText(file);
            while (!reader.EndOfStream)
            {
                string rawLine = reader.ReadLine();
                string formattedLine = rawLine;
                if (rawLine == null) break;

                while (rawLine.Trim().Length > 0)
                {
                    List<(Match, Color)> totalMatches = new List<(Match, Color)>();
                    foreach (var regex in syntax.colorPatterns)
                    {
                        if (rawLine.Trim().Length == 0) break;
                        MatchCollection matches = new Regex(regex.Key).Matches(rawLine);
                        for (int i = matches.Count - 1; i >= 0; i--)
                        {
                            Match match = matches[i];
                            if (match.Value.Trim().Length == 0) continue;
                            totalMatches.Add((matches[i], theme.colors[regex.Value]));
                            // string formattedMatch = match.Color(theme.colors[regex.Value]).ToString();
                            // formattedLine = formattedLine.Replace(match, formattedMatch);
                            // rawLine = rawLine.Remove(matches[i].Index, matches[i].Length);
                        }
                    }
                    if (totalMatches.Count == 0) break;
                    totalMatches.Sort(new MatchesComparer());
                    var longestMatch = totalMatches.First();
                    if (!rawLine.Contains(longestMatch.Item1.Value)) continue;
                    string formattedMatch = longestMatch.Item1.Value.Color(longestMatch.Item2).ToString();
                    string substr = formattedMatch.Substring(longestMatch.Item1.Index);
                    substr = substr.Replace(longestMatch.Item1.Value, formattedMatch);
                    formattedLine = formattedLine.Insert(longestMatch.Item1.Index, substr);
                    rawLine = rawLine.Remove(longestMatch.Item1.Index, longestMatch.Item1.Length);
                }

                Console.WriteLine(formattedLine);
            }

        }

        class MatchesComparer : IComparer<(Match, Color)>
        {
            public int Compare((Match, Color) x, (Match, Color) y) => -x.Item1.Length.CompareTo(y.Item1.Length);
        }
    }
}
