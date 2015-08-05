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

namespace VSGenero.Peek
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
