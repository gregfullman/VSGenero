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
