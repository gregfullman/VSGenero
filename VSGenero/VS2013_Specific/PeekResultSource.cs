/* ****************************************************************************
 * Copyright (c) 2014 Greg Fullman 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Navigation;

namespace VSGenero.VS2013_Specific
{
    class PeekResultSource : IPeekResultSource
    {
        private readonly PeekableItemSourceProvider _factory;
        private readonly GoToDefinitionLocation _location;

        public PeekResultSource(GoToDefinitionLocation location, PeekableItemSourceProvider factory)
        {
            _location = location;
            _factory = factory;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, System.Threading.CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            if(relationshipName == PredefinedPeekRelationships.Definitions.Name)
            {
                IPeekResult result = CreatePeekResult(resultCollection, _location);
                if(result != null)
                {
                    resultCollection.Add(result);
                }
            }
        }

        private string BuildTitle(GoToDefinitionLocation location)
        {
            // TODO: build the title
            return Path.GetFileName(location.Filename);
        }

        private IPeekResult CreatePeekResult(IPeekResultCollection resultCollection, GoToDefinitionLocation location)
        {
            string path = location.Filename;
            FileInfo fi = new FileInfo(path);
            PeekResultDisplayInfo displayInfo = new PeekResultDisplayInfo("Test", path, BuildTitle(location), path);
            // TODO: the location stuff doesn't work 100% correctly. This needs to be fixed
            int line = location.LineNumber - 1;   // start line
            if (line < 0)
                line = 0;
            int character = location.ColumnNumber - 1;  // start index
            //EditorExtensions.EditorExtensions.GetLineAndColumnOfFile(path, location.Position, out line, out character);
            int endLine = line + 10;    // end line
            int endIndex = 0;   // end index
            int positionLine = 0;   // id line
            int positionChar = 0;   // id index
            bool isReadOnly = fi.IsReadOnly;

            // TODO: determine the stuff above.

            return this._factory.PeekResultFactory.Create(displayInfo, path, line, character, endLine, endIndex, positionLine, positionChar, isReadOnly);
        }
    }
}
