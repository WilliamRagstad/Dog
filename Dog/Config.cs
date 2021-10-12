using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dog
{
    /// <summary>
    /// Configuration DTO.
    /// Stores data used to customize Dog and a map of supported
    /// languages together with their extensions and syntax files.
    /// </summary>
    class Config
    {
        // ReSharper disable once InconsistentNaming
        public float version;
        // ReSharper disable once InconsistentNaming
        public bool showLines;
        // ReSharper disable once InconsistentNaming
        public string defaultTheme;
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, string> syntaxes;

        public static Config Default = new Config
        {
            version = 1.0f,
            showLines = false,
            syntaxes = new Dictionary<string, string> {
                {"cs", "C#"},
                {"csx", "C#"},
                {"java", "Java"},
                {"bsh", "Java"},
                {"py", "Python"},
                {"py3", "Python"},
                {"pyw", "Python"},
                {"pyi", "Python"},
                {"pyx", "Python"},
                {"rpy", "Python"},
                {"cpy", "Python"},
                {"gpy", "Python"},
                {"vpy", "Python"},
                {"pxi", "Python"}
            },
            defaultTheme = "Dracula"
        };
    }
}
