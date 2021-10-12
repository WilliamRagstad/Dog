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
        public Version Version;
        public string TestField;
        public Dictionary<string, string> Syntaxes;

        public static Config Default = new Config
        {
            Version = new Version(1, 0),
            TestField = "This is a test field",
            Syntaxes = new Dictionary<string, string> {
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
            }
        };
    }
}
