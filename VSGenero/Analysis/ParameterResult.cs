using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis
{
    public class ParameterResult : IEquatable<ParameterResult>
    {
        public string Name { get; private set; }
        public string Documentation { get; private set; }
        public string Type { get; private set; }

        public ParameterResult(string name)
            : this(name, String.Empty, "object")
        {
        }
        public ParameterResult(string name, string doc)
            : this(name, doc, "object")
        {
        }
        public ParameterResult(string name, string doc, string type)
        {
            Name = name;
            Documentation = doc;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ParameterResult);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                (Type ?? "").GetHashCode();
        }

        public bool Equals(ParameterResult other)
        {
            return other != null &&
                Name == other.Name &&
                Documentation == other.Documentation &&
                Type == other.Type;
        }
    }
}
