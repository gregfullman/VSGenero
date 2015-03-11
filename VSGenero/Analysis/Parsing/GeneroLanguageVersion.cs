using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing
{
    public enum GeneroLanguageVersion
    {
        None = 0,
        V232,
        V240,
        V241,
        V250
    }

    public static class PythonLanguageVersionExtensions
    {
        public static Version ToVersion(this GeneroLanguageVersion version)
        {
            switch(version)
            {
                case GeneroLanguageVersion.V232: return new Version(2, 32);
                case GeneroLanguageVersion.V240: return new Version(2, 40);
                case GeneroLanguageVersion.V241: return new Version(2, 41);
                case GeneroLanguageVersion.V250: return new Version(2, 50);
                default: return null;
            }
        }

        public static GeneroLanguageVersion ToLanguageVersion(this Version version)
        {
            switch (version.Major)
            {
                case 2:
                    switch (version.Minor)
                    {
                        case 32: return GeneroLanguageVersion.V232;
                        case 40: return GeneroLanguageVersion.V240;
                        case 41: return GeneroLanguageVersion.V241;
                        case 50: return GeneroLanguageVersion.V250;
                        default: return GeneroLanguageVersion.None;
                    }
                default:
                    return GeneroLanguageVersion.None;
            }
        }
    }
}
