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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
    internal class Genero4glLanguageInfo
    {
        // Fields
        private readonly ICompletionBroker completionBroker;
        private readonly string filename;
        private static readonly Guid guidGeneroLanguageService = new Guid("c41c558d-4373-4ae1-8424-fb04873a0e9c");
        private const string guidCSharpLanguageServiceString = "c41c558d-4373-4ae1-8424-fb04873a0e9c";
        //private readonly IDECompilerHost host;
        private readonly ITextBuffer subjectBuffer;

        // Methods
        public Genero4glLanguageInfo(/*IDECompilerHost host,*/ ICompletionBroker completionBroker, ITextBuffer subjectBuffer, string filename)
        {
            //this.host = host;
            this.completionBroker = completionBroker;
            this.subjectBuffer = subjectBuffer;
            this.filename = filename;
        }

        public void DismissAllAndCommitActiveOne(ITextView view, char commitChar)
        {
            if (this.completionBroker.IsCompletionActive(view) && this.IsCommitChar(commitChar))
            {
                ReadOnlyCollection<ICompletionSession> sessions = this.completionBroker.GetSessions(view);
                if ((sessions.Count != 1) || !sessions[0].IsStarted)
                {
                    this.completionBroker.DismissAllSessions(view);
                }
                else
                {
                    CompletionSet selectedCompletionSet = sessions[0].SelectedCompletionSet;
                    if (((selectedCompletionSet != null) && (selectedCompletionSet.SelectionStatus != null)) && selectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        sessions[0].Commit();
                    }
                    this.completionBroker.DismissAllSessions(view);
                }
            }
        }

        private string GetIndentationString(IEditorOptions options, int indentationLength)
        {
            if (indentationLength <= 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            int num = 0;
            int num2 = indentationLength;
            if (!options.IsConvertTabsToSpacesEnabled())
            {
                int tabSize = options.GetTabSize();
                num = indentationLength / tabSize;
                num2 -= num * tabSize;
            }
            for (int i = 0; i < num; i++)
            {
                builder.Append('\t');
            }
            for (int j = 0; j < num2; j++)
            {
                builder.Append(' ');
            }
            return builder.ToString();
        }

        private bool IsCommitChar(char commitChar)
        {
            return false;
            //return ((((this.host.LanguageService != null) && 
            //          (this.host.LanguageService.Options != null)
            //         ) && 
            //         (this.host.LanguageService.Options.CompletionCommitCharacters != null)
            //        ) && 
            //        (this.host.LanguageService.Options.CompletionCommitCharacters.IndexOf(commitChar) >= 0));
        }

        public bool IsPossibleTypeVariableDecl(SnapshotPoint point)
        {
            return false;
            //ParseTreeNode node2;
            //ParseTreeNode node3;
            //Position pos = point.ToCSharpPosition(null);
            //ParseTree latestParseTree = this.LatestParseTree;
            //if (latestParseTree == null)
            //{
            //    return false;
            //}
            //ParseTreeNode node = latestParseTree.FindLeafNode(pos);
            //if (node == null)
            //{
            //    return false;
            //}
            //if (node.GetEnclosingNode(NodeGroup.Member, out node2, out node3) && !node2.IsNestedType())
            //{
            //    if (node2.IsIncompleteMember() && node3.IsNamedType())
            //    {
            //        return true;
            //    }
            //    if (node2.IsField() && node3.IsVariableDeclaration())
            //    {
            //        FieldDeclarationNode node4 = node2.AsAnyField();
            //        if (node4.VariableDeclarators.Count != 1)
            //        {
            //            return false;
            //        }
            //        return (node4.VariableDeclarators[0].OptionalArgument == null);
            //    }
            //    if (!node2.IsAnyMethod())
            //    {
            //        return false;
            //    }
            //    MethodBaseNode node5 = node2.AsAnyMethod();
            //    if ((node5.OpenToken < 0) || (node5.OpenToken >= node5.ParseTree.LexData.Tokens.Count))
            //    {
            //        return true;
            //    }
            //    Position startPosition = node5.ParseTree.LexData.Tokens[node5.OpenToken].StartPosition;
            //    return (pos < startPosition);
            //}
            //if (ParseTreeMatch.GetEnclosingNode(node, NodeKind.DelegateDeclaration, out node2, out node3))
            //{
            //    return (node3 != null);
            //}
            //if (node.GetEnclosingNode(NodeGroup.Aggregate, out node2, out node3))
            //{
            //    TypeDeclarationNode node6 = node2.AsAnyAggregate();
            //    if ((node6.OpenToken < 0) || (node6.OpenToken >= node6.ParseTree.LexData.Tokens.Count))
            //    {
            //        return true;
            //    }
            //    Position position3 = node6.ParseTree.LexData.Tokens[node6.OpenToken].StartPosition;
            //    return (pos < position3);
            //}
            //return (ParseTreeMatch.GetEnclosingNode(node, NodeKind.UsingDirective) != null);
        }

        public bool IsValidContext(SnapshotPoint point)
        {
            return true;
            //ParseTree latestParseTree = this.LatestParseTree;
            //if (latestParseTree == null)
            //{
            //    return false;
            //}
            //Position pos = point.ToCSharpPosition(null);
            //if (latestParseTree.IsInsideSkippedPreProcessorRegion(pos))
            //{
            //    return false;
            //}
            //if (latestParseTree.IsRegionStartLine(pos.Line) || latestParseTree.IsRegionEndLine(pos.Line))
            //{
            //    return false;
            //}
            //if (latestParseTree.IsTransitionLine(pos.Line))
            //{
            //    return false;
            //}
            //if (latestParseTree.IsPreprocessorLine(pos.Line))
            //{
            //    return false;
            //}
            //Token tokenAtPosition = latestParseTree.GetTokenAtPosition(pos);
            //return (((tokenAtPosition == null) || !tokenAtPosition.Span.Contains(pos)) || ((!tokenAtPosition.IsComment() && (tokenAtPosition.Kind != TokenKind.StringLiteral)) && ((tokenAtPosition.Kind != TokenKind.VerbatimStringLiteral) && (tokenAtPosition.Kind != TokenKind.CharacterLiteral))));
        }

        public bool TryFormat(SnapshotSpan span)
        {
            return false;
            //IVsTextLayer vsTextLayer = this.subjectBuffer.ToIVsTextBuffer() as IVsTextLayer;
            //IVsLanguageTextOps vsTextOperation = this.VsTextOperation;
            //return (((vsTextOperation != null) && (vsTextLayer != null)) && this.TryFormat(vsTextLayer, vsTextOperation, span));
        }

        private bool TryFormat(IVsTextLayer vsTextLayer, IVsLanguageTextOps languageTextOp, SnapshotSpan span)
        {
            return false;
            //Position position = span.Start.ToCSharpPosition(null);
            //Position position2 = span.End.ToCSharpPosition(null);
            //TextSpan[] ptsSel = new TextSpan[1];
            //TextSpan span2 = new TextSpan
            //{
            //    iStartLine = position.Line,
            //    iStartIndex = position.Character,
            //    iEndLine = position2.Line,
            //    iEndIndex = position2.Character
            //};
            //ptsSel[0] = span2;
            //return ErrorHandler.Succeeded(languageTextOp.Format(vsTextLayer, ptsSel));
        }

        public bool TryGetLineIndentation(ITextSnapshotLine line, out int preferredIndentation)
        {
            IVsTextLayer vsTextLayer = this.subjectBuffer.ToIVsTextBuffer() as IVsTextLayer;
            IVsLanguageLineIndent vsLanguageLineIndent = this.VsLanguageLineIndent;
            if ((vsLanguageLineIndent != null) && (vsTextLayer != null))
            {
                return this.TryGetLineIndentation(vsTextLayer, vsLanguageLineIndent, line.LineNumber, out preferredIndentation);
            }
            preferredIndentation = 0;
            return false;
        }

        public bool TryGetLineIndentation(ITextSnapshotLine line, IEditorOptions options, out string indentationString)
        {
            IVsTextLayer vsTextLayer = this.subjectBuffer.ToIVsTextBuffer() as IVsTextLayer;
            IVsLanguageLineIndent vsLanguageLineIndent = this.VsLanguageLineIndent;
            if ((vsLanguageLineIndent != null) && (vsTextLayer != null))
            {
                return this.TryGetLineIndentation(vsTextLayer, vsLanguageLineIndent, options, line.LineNumber, out indentationString);
            }
            indentationString = string.Empty;
            return false;
        }

        private bool TryGetLineIndentation(IVsTextLayer vsTextLayer, IVsLanguageLineIndent vsLineIdent, int lineNumber, out int preferredIndentation)
        {
            if (ErrorHandler.Failed(vsLineIdent.GetIndentPosition(vsTextLayer, lineNumber, out preferredIndentation)))
            {
                preferredIndentation = 0;
                return false;
            }
            return true;
        }

        private bool TryGetLineIndentation(IVsTextLayer vsTextLayer, IVsLanguageLineIndent vsLineIdent, IEditorOptions options, int lineNumber, out string indentationString)
        {
            int num;
            if (!this.TryGetLineIndentation(vsTextLayer, vsLineIdent, lineNumber, out num))
            {
                indentationString = string.Empty;
                return false;
            }
            indentationString = this.GetIndentationString(options, num);
            return true;
        }

        // Properties
        //private ILangService LanguageService
        //{
        //    get
        //    {
        //        return this.host.LanguageService;
        //    }
        //}

        //public ParseTree LatestParseTree
        //{
        //    get
        //    {
        //        ICSharpTextBuffer buffer = this.LanguageService.FindTextBuffer(this.filename);
        //        if (buffer == null)
        //        {
        //            return null;
        //        }
        //        return buffer.GetDisposableSourceData().AsParseTree(this.host.NameTable);
        //    }
        //}

        private IVsLanguageLineIndent VsLanguageLineIndent
        {
            get
            {
                return (this.VsTextOperation as IVsLanguageLineIndent);
            }
        }

        private IVsLanguageTextOps VsTextOperation
        {
            get
            {
                ServiceProvider globalProvider = ServiceProvider.GlobalProvider;
                if (globalProvider == null)
                {
                    return null;
                }
                return (globalProvider.GetService(guidGeneroLanguageService) as IVsLanguageTextOps);
            }
        }

    }
}
