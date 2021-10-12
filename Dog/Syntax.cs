using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dog
{
    class Syntax
    {
        public enum Unit
        {
            Unknown,
            Keyword,
            Identifier,
            Primitive,
            Numerical,
            String,
            Character,
            Operator,
            Parenthesis,
            Separator,
            Comment,
        }

        // ReSharper disable once InconsistentNaming
        public string name;
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, Unit> colorPatterns;

        public static Syntax PlainText = new Syntax
        {
            name = "Plain Text",
            colorPatterns = new Dictionary<string, Unit>()
        };
    }
}
