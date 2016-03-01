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

using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    /// <summary>
    /// This is the class on which all other AST nodes are based.
    /// It provides a SnapshotSpan of itself.
    /// </summary>
    public abstract class AstNode4gl : AstNode
    {
        private Dictionary<string, List<PreprocessorNode>> _includeFiles;
        public Dictionary<string, List<PreprocessorNode>> IncludeFiles
        {
            get
            {
                if (_includeFiles == null)
                    _includeFiles = new Dictionary<string, List<PreprocessorNode>>(StringComparer.OrdinalIgnoreCase);
                return _includeFiles;
            }
        }

        protected bool BindPrepareCursorFromIdentifier(PrepareStatement prepStmt)
        {
            // If the prepare statement uses a variable from prepare from, that variable should have been encountered
            // prior to the prepare statement. So we'll do a binary search in the children of this function to look for
            // a LetStatement above the prepare statement where the prepare statement's from identifier was assigned. 
            // If it can't be found, then we have to assume that the identifier was assigned outside of this function,
            // and we have no real way to determining the cursor SQL text.
            bool result = false;

            if (prepStmt.Children.Count == 1)
            {
                StringExpressionNode strExpr = prepStmt.Children[prepStmt.Children.Keys[0]] as StringExpressionNode;
                if (strExpr != null)
                {
                    prepStmt.SetSqlStatement(strExpr.LiteralValue);
                }
                else
                {
                    FglNameExpression exprNode = prepStmt.Children[prepStmt.Children.Keys[0]] as FglNameExpression;
                    if (exprNode != null)
                    {
                        string ident = exprNode.Name;

                        List<int> keys = Children.Select(x => x.Key).ToList();
                        int searchIndex = keys.BinarySearch(prepStmt.StartIndex);
                        if (searchIndex < 0)
                        {
                            searchIndex = ~searchIndex;
                            if (searchIndex > 0)
                                searchIndex--;
                        }

                        LetStatement letStmt = null;
                        while (searchIndex >= 0 && searchIndex < keys.Count)
                        {
                            int key = keys[searchIndex];
                            letStmt = Children[key] as LetStatement;
                            if (letStmt != null)
                            {
                                // check for the LetStatement's identifier
                                if (ident.Equals(letStmt.Variable.Name, StringComparison.OrdinalIgnoreCase))
                                    break;
                                else
                                    letStmt = null;
                            }
                            searchIndex--;
                        }

                        if (letStmt != null)
                        {
                            // we have a match, bind the let statement's value
                            prepStmt.SetSqlStatement(letStmt.GetLiteralValue());
                            result = true;
                        }
                    }
                }
            }
            return result;
        }
    }
}
