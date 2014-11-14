using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.VS2013_Specific
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class PeekSupportedContentTypeAttribute : RegistrationAttribute
    {
        private string _fileExtension;

        public PeekSupportedContentTypeAttribute(string fileExtension)
        {
            _fileExtension = fileExtension;
        }

        private string RegKey
        {
            get
            {
                return @"Peek\SupportedContentTypes";
            }
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            using(RegistrationAttribute.Key key = context.CreateKey(RegKey))
            {
                key.SetValue(_fileExtension, "");
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
            context.RemoveValue(RegKey, _fileExtension);
        }
    }
}
