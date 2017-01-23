using System;

namespace VSGenero.External
{
    public static class GeneroConstants
    {
        public const string LanguageName = "Genero";
        public const string LanguageName4GL = "Genero4GL";
        public const string LanguageNamePER = "GeneroPER";
        public const string LanguageNameINC = "GeneroINC";
        public const string FileExtension4GL = ".4gl";
        public const string FileExtensionPER = ".per";
        public const string FileExtensionINC = ".inc";
        public const string FileExtension4RP = ".4rp";

        public const string ContentType4GL = LanguageName4GL;
        public const string ContentTypePER = LanguageNamePER;
        public const string ContentTypeINC = LanguageNameINC;

        public const string guidGenero4glLanguageService = "c41c558d-4373-4ae1-8424-fb04873a0e9c";
        public static readonly Guid guidGenero4glLanguageServiceGuid = new Guid(guidGenero4glLanguageService);
    }
}
