using Microsoft.VisualStudio.Text;
using System;
using System.IO;

namespace VSGenero.External
{
    public static class GeneroExtensions
    {
        public static bool IsGenero4GLContent(ITextBuffer buffer)
        {
            return buffer.ContentType.IsOfType(GeneroConstants.ContentType4GL);
        }

        public static bool IsGenero4GLContent(ITextSnapshot buffer)
        {
            return buffer.ContentType.IsOfType(GeneroConstants.ContentType4GL);
        }

        public static bool IsGeneroPERContent(ITextBuffer buffer)
        {
            return buffer.ContentType.IsOfType(GeneroConstants.ContentTypePER);
        }

        public static bool IsGeneroPERContent(ITextSnapshot buffer)
        {
            return buffer.ContentType.IsOfType(GeneroConstants.ContentTypePER);
        }

        public static bool IsGeneroFile(string filename)
        {
            var ext = Path.GetExtension(filename);
            return string.Equals(ext, GeneroConstants.FileExtension4GL, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(ext, GeneroConstants.FileExtensionPER, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(ext, GeneroConstants.FileExtensionINC, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(ext, GeneroConstants.FileExtension4RP, StringComparison.OrdinalIgnoreCase);

        }
    }
}
