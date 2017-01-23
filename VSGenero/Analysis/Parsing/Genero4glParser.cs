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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing.AST_4GL;
using VSGenero.External.Analysis.Parsing;

namespace VSGenero.Analysis.Parsing
{
    public class Genero4glParser : GeneroParser
    {
        public readonly FglStatementFactory StatementFactory;

        #region Construction

        public Genero4glParser(Tokenizer tokenizer, ErrorSink errorSink, bool verbatim, bool bindRefs, ParserOptions options)
            : base(tokenizer, errorSink, verbatim, bindRefs, options)
        {
            StatementFactory = new FglStatementFactory();
        }

        #endregion

        protected override GeneroAst CreateAst()
        {
            ModuleNode moduleNode = null;
            if (ModuleNode.TryParseNode(this, out moduleNode))
            {
                var ast = new Genero4glAst(moduleNode, 
                                           _tokenizer.GetLineLocations(), 
                                           GeneroLanguageVersionExtensions.GetLanguageVersion(_filename, VSGeneroPackage.Instance.ProgramFileProvider), 
                                           _projectEntry, 
                                           _filename);
                UpdateNodeAndTree(moduleNode, ast);
                return ast;
            }
            return null;
        }

        public bool ParseSingleFunction(out FunctionBlockNode functionNode, bool abbreviatedParse, out string remainingReadText, out int[] lineLocations)
        {
            StartParsing();
            functionNode = null;
            remainingReadText = null;
            int bufPos;
            bool result = FunctionBlockNode.TryParseNode(this, out functionNode, out bufPos, null, abbreviatedParse, false);
            remainingReadText = _tokenizer.GetRemainingReadText(bufPos);
            lineLocations = _tokenizer.GetLineLocations();
            return result;
        }
    }
}
