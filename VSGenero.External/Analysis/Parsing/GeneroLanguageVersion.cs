using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.External.Interfaces;

namespace VSGenero.External.Analysis.Parsing
{
    public enum GeneroLanguageVersion
    {
        None = 0,
        [GeneroLanguageVersion(VersionString = "2.32", LanguageVersion = V232)]
        V232,
        [GeneroLanguageVersion(VersionString = "2.40", LanguageVersion = V240)]
        V240,
        [GeneroLanguageVersion(VersionString = "2.41", LanguageVersion = V241)]
        V241,
        [GeneroLanguageVersion(VersionString = "2.50", LanguageVersion = V250, DocumentationNumber = "2.50.00-")]   // trailing '-' needed to provide correct documentation URL
        V250,
        [GeneroLanguageVersion(VersionString = "3.00", LanguageVersion = V300)]
        V300,

        Latest = V300 + 1
    }

    public class GeneroLanguageVersionAttribute : Attribute
    {
        public string DocumentationNumber { get; set; }

        public GeneroLanguageVersion LanguageVersion { get; set; }

        public string VersionString { get; set; }
    }

    public static class GeneroLanguageVersionExtensions
    {
        public static Version ToVersion(this GeneroLanguageVersion version)
        {
            switch (version)
            {
                case GeneroLanguageVersion.V232: return new Version(2, 32);
                case GeneroLanguageVersion.V240: return new Version(2, 40);
                case GeneroLanguageVersion.V241: return new Version(2, 41);
                case GeneroLanguageVersion.V250: return new Version(2, 50);
                case GeneroLanguageVersion.V300: return new Version(3, 0);
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
                case 3:
                    switch (version.Minor)
                    {
                        case 0: return GeneroLanguageVersion.V300;
                        default: return GeneroLanguageVersion.None;
                    }
                default:
                    return GeneroLanguageVersion.None;
            }
        }

        public static GeneroLanguageVersion GetLanguageVersion(string filePath, IProgramFileProvider fileProvider)
        {
            if (fileProvider != null)
                return fileProvider.GetLanguageVersion(filePath);
            return GeneroLanguageVersion.None;
        }
    }
}
