using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class LanguageBraceCompletionAttribute : RegistrationAttribute
    {
        private readonly string _languageName;
        private bool _enableCompletion;

        public LanguageBraceCompletionAttribute(string languageName)
        {
            _languageName = languageName;
        }

        public bool EnableCompletion
        {
            get { return _enableCompletion; }
            set { _enableCompletion = value; }
        }

        private string RegKey
        {
            get
            {
                return string.Format(@"Languages\Language Services\{0}", _languageName);
            }
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            using (RegistrationAttribute.Key key = context.CreateKey(RegKey))
            {
                key.SetValue("ShowBraceCompletion", _enableCompletion ? 1 : 0);
            }
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
            context.RemoveValue(RegKey, "ShowBraceCompletion");
        }
    }
}
