/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * 
 * This file is basically unmodified from the original taken from PythonTools.
 * The Guids were modified for Genero.
 * It should not need editing.
 * 
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

// Guids.cs
// MUST match guids.h
using System;

namespace VSGenero
{
    static class GuidList
    {
        public const string guidVSGeneroPkgString = "18274d18-91c9-420c-a121-2ffe4f920b4e";
        public const string guidVSGeneroCmdSetString = "730fb573-e079-4739-aec6-45c02012ac76";

        public static readonly Guid guidVSGeneroCmdSet = new Guid(guidVSGeneroCmdSetString);
    };
}