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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;
using VSGenero.Navigation;
using Microsoft.VisualStudio.VSCommon;
using Microsoft.VisualStudio.Text;

namespace VSGenero.Peek
{
    class PeekResultSource : IPeekResultSource
    {
        private readonly PeekableItemSourceProvider _factory;
        private readonly IEnumerable<LocationInfo> _locations;

        public PeekResultSource(IEnumerable<LocationInfo> locations, PeekableItemSourceProvider factory)
        {
            _locations = locations;
            _factory = factory;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, System.Threading.CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            if(relationshipName == PredefinedPeekRelationships.Definitions.Name)
            {
                foreach (var location in _locations)
                {
                    IPeekResult result = CreatePeekResult(resultCollection, location);
                    if (result != null)
                    {
                        resultCollection.Add(result);
                    }
                }
            }
        }

        private string BuildTitle(LocationInfo location)
        {
            return Path.GetFileName(location.FilePath);
        }

        private string BuildLabel(LocationInfo location)
        {
            return string.Format("{0}, {1}", BuildTitle(location), location.Line);
        }

        private IPeekResult CreatePeekResult(IPeekResultCollection resultCollection, LocationInfo location)
        {
            IPeekResult result = null;
            var path = location.FilePath;
            var fi = new FileInfo(path);
            var displayInfo = new PeekResultDisplayInfo(BuildLabel(location), path, BuildTitle(location), path);
            // TODO: the location stuff doesn't work 100% correctly. This needs to be fixed
            string contentType = null;
            var extension = Path.GetExtension(path);
            if (extension != null)
            {
                switch (extension.ToLower())
                {
                    case ".4gl":
                        contentType = VSGeneroConstants.ContentType4GL;
                        break;
                    case ".inc":
                        contentType = VSGeneroConstants.ContentTypeINC;
                        break;
                    case ".per":
                        contentType = VSGeneroConstants.ContentTypePER;
                        break;
                }
            }
            if(contentType != null)
            {
                ITextDocument textDoc = null;
                int line = location.Line - 1;
                if (line < 0)
                {
                    textDoc = this._factory.TextDocumentFactoryService.CreateAndLoadTextDocument(path, this._factory.ContentTypeRegistryService.GetContentType(contentType));
                    line = textDoc.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(location.Index);
                }
                int character = location.Column - 1;  // start index
                if (character < 0)
                    character = 0;
                //EditorExtensions.EditorExtensions.GetLineAndColumnOfFile(path, location.Position, out line, out character);
                int endLine = line + 10;    // end line
                int endIndex = 0;   // end index
                int positionLine = 0;   // id line
                int positionChar = 0;   // id index
                bool isReadOnly = fi.IsReadOnly;

                // TODO: determine the stuff above.

                result = this._factory.PeekResultFactory.Create(displayInfo, path, line, character, endLine, endIndex, positionLine, positionChar, isReadOnly);
                result.Disposed += (x, y) =>
                    {
                        if (textDoc != null)
                            textDoc.Dispose();
                    };
            }
            return result;
        }
    }
}
