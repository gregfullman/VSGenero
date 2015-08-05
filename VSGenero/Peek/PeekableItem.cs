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

using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Navigation;

namespace VSGenero.Peek
{
    class PeekableItem : IPeekableItem
    {
        private readonly PeekableItemSourceProvider _factory;
        private readonly IEnumerable<LocationInfo> _locations;

        public PeekableItem(IEnumerable<LocationInfo> locations, PeekableItemSourceProvider factory)
        {
            _locations = locations;
            _factory = factory;
        }

        public string DisplayName
        {
            get { return null; }
        }

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new PeekResultSource(this._locations, this._factory);
        }

        public IEnumerable<IPeekRelationship> Relationships
        {
            get { yield return PredefinedPeekRelationships.Definitions; }
        }
    }
}
