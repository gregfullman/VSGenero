using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VSGeneroUnitTesting.Utilities
{
    [TestClass]
    public class GlyphDownloader
    {
        [TestMethod]
        public void DownloadGlyphs()
        {
            string targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), @"Visual Studio Glyphs\");
            Directory.CreateDirectory(targetDir);

            foreach (StandardGlyphGroup g in Enum.GetValues(typeof(StandardGlyphGroup)))
            {
                if (g == StandardGlyphGroup.GlyphGroupUnknown) continue;

                if (!g.ToString().StartsWith("GlyphGroup"))
                {
                    new WebClient().DownloadFile(
                        "http://referencesource-beta.microsoft.com/content/icons/" + (int)g + ".png",
                        targetDir + (int)g + " - " + g.ToString().Replace("Glyph", "") + ".png");
                    continue;
                }

                for (int i = 0; i < 6; i++)
                {
                    int index = (int)g + i;
                    new WebClient().DownloadFile(
                        "http://referencesource-beta.microsoft.com/content/icons/" + index + ".png",
                        targetDir + index + " - " + g.ToString().Replace("GlyphGroup", "") + "-" + ((StandardGlyphItem)i).ToString().Replace("GlyphItem", "") + ".png");
                }
            }
        }
    }
}
