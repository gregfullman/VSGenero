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

namespace VSGenero.Analysis.Parsing
{
    public class Genero4glParser : GeneroParser
    {
        public readonly FglStatementFactory StatementFactory;

        #region Construction

        private Genero4glParser(Tokenizer tokenizer, ErrorSink errorSink, bool verbatim, bool bindRefs, ParserOptions options)
            : base(tokenizer, errorSink, verbatim, bindRefs, options)
        {
            StatementFactory = new FglStatementFactory();
        }

        public static Genero4glParser CreateParser(TextReader reader, IProjectEntry projEntry = null, string filename = null)
        {
            return CreateParser(reader, null, projEntry, filename);
        }

        public static Genero4glParser CreateParser(TextReader reader, ParserOptions parserOptions, IProjectEntry projEntry = null, string filename = null)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var options = parserOptions ?? ParserOptions.Default;

            Genero4glParser result = null;
            Tokenizer tokenizer = new Tokenizer(options.ErrorSink,
                                                (options.Verbatim ? TokenizerOptions.Verbatim : TokenizerOptions.None) | TokenizerOptions.GroupingRecovery,
                                                (span, text) => options.RaiseProcessComment(result, new CommentEventArgs(span, text)));

            tokenizer.Initialize(null, reader, SourceLocation.MinValue);
            tokenizer.IndentationInconsistencySeverity = options.IndentationInconsistencySeverity;

            result = new Genero4glParser(tokenizer,
                options.ErrorSink ?? ErrorSink.Null,
                options.Verbatim,
                options.BindReferences,
                options
            );
            result._projectEntry = projEntry;
            result._filename = filename;

            result._sourceReader = reader;
            return result;
        }

        /// <summary>
        /// Creates a new parser from a seekable stream including scanning the BOM or looking for a # coding: comment to detect the appropriate coding.
        /// </summary>
        public static Genero4glParser CreateParser(Stream stream, ParserOptions parserOptions = null, IProjectEntry projEntry = null)
        {
            var options = parserOptions ?? ParserOptions.Default;
            var reader = new StreamReader(stream, true);

            return CreateParser(reader, options, projEntry);
        }

        #endregion

        public override GeneroAst ParseFile()
        {
            return ParseFileWorker();
        }

        private Genero4glAst CreateAst(AstNode4gl node)
        {
            var ast = new Genero4glAst(node, _tokenizer.GetLineLocations(), GeneroLanguageVersion.None, _projectEntry, _filename);
            node.PropagateSyntaxTree(ast);
            if (_verbatim)
            {
                if (_lookahead.Token != null)
                {
                    AddExtraVerbatimText(node, _lookaheadWhiteSpace + _lookahead.Token.VerbatimImage);
                }
                AddCodeRegions(node);
                AddNonCodeRegionComments(node);
                _codeRegions.Clear();
                _nonCodeRegionComments.Clear();
            }
            foreach (var keyValue in _attributes)
            {
                foreach (var nodeAttr in keyValue.Value)
                {
                    ast.SetAttribute(keyValue.Key, nodeAttr.Key, nodeAttr.Value);
                }
            }
            return ast;
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

        private Genero4glAst ParseFileWorker()
        {
            StartParsing();

            ModuleNode moduleNode = null;
            if (ModuleNode.TryParseNode(this, out moduleNode))
            {
                return CreateAst(moduleNode);
            }
            _codeRegions.Clear();
            _nonCodeRegionComments.Clear();
            return null;
        }
    }
}
