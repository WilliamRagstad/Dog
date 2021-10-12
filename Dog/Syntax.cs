using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dog
{
    class Syntax
    {
        // ReSharper disable once InconsistentNaming
        public string name;
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, string> variables;
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, string> contexts;

        public static Syntax PlainText = new Syntax
        {
            name = "Plain Text",
            variables = new Dictionary<string, string>(),
            contexts = new Dictionary<string, string>()
        };
    }
}
