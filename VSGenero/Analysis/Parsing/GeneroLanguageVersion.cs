/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
        [GeneroLanguageVersion(DocumentationNumber = "2.50.00-")]   // trailing '-' needed to provide correct documentation URL
        V250,
        V300,

        Latest = V300
    }

    public class GeneroLanguageVersionAttribute : Attribute
    {
        public string DocumentationNumber { get; set; }
    }

    public static class GeneroLanguageVersionExtensions
    {
        public static Version ToVersion(this GeneroLanguageVersion version)
        {
            switch(version)
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
                    switch(version.Minor)
                    {
                        case 0: return GeneroLanguageVersion.V300;
                        default: return GeneroLanguageVersion.None;
                    }
                default:
                    return GeneroLanguageVersion.None;
            }
        }
    }
}
