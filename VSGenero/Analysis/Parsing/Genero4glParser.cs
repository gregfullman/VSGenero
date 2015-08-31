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
        private Dictionary<AstNode4gl, Dictionary<object, object>> _attributes = new Dictionary<AstNode4gl, Dictionary<object, object>>();  // attributes for each node, currently just round tripping information

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

        public override IGeneroAst ParseFile()
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
                _codeRegions.Clear();
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
            return null;
        }

        #region Verbatim AST support

        private void AddPreceedingWhiteSpace(AstNode4gl ret)
        {
            AddPreceedingWhiteSpace(ret, _tokenWhiteSpace);
        }

        private Dictionary<object, object> GetNodeAttributes(AstNode4gl node)
        {
            Dictionary<object, object> attrs;
            if (!_attributes.TryGetValue(node, out attrs))
            {
                _attributes[node] = attrs = new Dictionary<object, object>();
            }
            return attrs;
        }

        private void AddVerbatimName(Name name, AstNode4gl ret)
        {
            if (_verbatim && name.RealName != name.VerbatimName)
            {
                GetNodeAttributes(ret)[NodeAttributes.VerbatimImage] = name.VerbatimName;
            }
        }

        private void AddVerbatimImage(AstNode4gl ret, string image)
        {
            if (_verbatim)
            {
                GetNodeAttributes(ret)[NodeAttributes.VerbatimImage] = image;
            }
        }

        private List<string> MakeWhiteSpaceList()
        {
            return _verbatim ? new List<string>() : null;
        }

        private void AddPreceedingWhiteSpace(AstNode4gl ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.PreceedingWhiteSpace] = whiteSpace;
        }

        private void AddSecondPreceedingWhiteSpace(AstNode4gl ret, string whiteSpace)
        {
            if (_verbatim)
            {
                Debug.Assert(_verbatim);
                GetNodeAttributes(ret)[NodeAttributes.SecondPreceedingWhiteSpace] = whiteSpace;
            }
        }

        private void AddThirdPreceedingWhiteSpace(AstNode4gl ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ThirdPreceedingWhiteSpace] = whiteSpace;
        }

        private void AddFourthPreceedingWhiteSpace(AstNode4gl ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.FourthPreceedingWhiteSpace] = whiteSpace;
        }

        private void AddFifthPreceedingWhiteSpace(AstNode4gl ret, string whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.FifthPreceedingWhiteSpace] = whiteSpace;
        }

        private void AddExtraVerbatimText(AstNode4gl ret, string text)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ExtraVerbatimText] = text;
        }

        private void AddCodeRegions(AstNode4gl ret)
        {
            Debug.Assert(_verbatim);
            TokenWithSpan[] arr = new TokenWithSpan[_codeRegions.Count];
            _codeRegions.CopyTo(arr);
            GetNodeAttributes(ret)[NodeAttributes.CodeRegions] = arr;
        }

        private void AddListWhiteSpace(AstNode4gl ret, string[] whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.ListWhiteSpace] = whiteSpace;
        }

        private void AddNamesWhiteSpace(AstNode4gl ret, string[] whiteSpace)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.NamesWhiteSpace] = whiteSpace;
        }

        private void AddVerbatimNames(AstNode4gl ret, string[] names)
        {
            Debug.Assert(_verbatim);
            GetNodeAttributes(ret)[NodeAttributes.VerbatimNames] = names;
        }

        private void AddIsAltForm(AstNode4gl expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.IsAltFormValue] = NodeAttributes.IsAltFormValue;
        }

        private void AddErrorMissingCloseGrouping(AstNode4gl expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.ErrorMissingCloseGrouping] = NodeAttributes.ErrorMissingCloseGrouping;
        }

        private void AddErrorIsIncompleteNode(AstNode4gl expr)
        {
            GetNodeAttributes(expr)[NodeAttributes.ErrorIncompleteNode] = NodeAttributes.ErrorIncompleteNode;
        }

        #endregion
    }
}
