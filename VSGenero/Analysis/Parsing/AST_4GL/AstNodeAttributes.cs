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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_4GL
{
    internal static class NodeAttributes
    {
        /// <summary>
        /// Value is a string which proceeds a token in the node.
        /// </summary>
        public static readonly object PreceedingWhiteSpace = new object();
        /// <summary>
        /// Value is a string which proceeds a second token in the node.
        /// </summary>
        public static readonly object SecondPreceedingWhiteSpace = new object();

        /// <summary>
        /// Value is a string which proceeds a third token in the node.
        /// </summary>
        public static readonly object ThirdPreceedingWhiteSpace = new object();

        /// <summary>
        /// Value is a string which proceeds a fourth token in the node.
        /// </summary>
        public static readonly object FourthPreceedingWhiteSpace = new object();

        /// <summary>
        /// Value is a string which proceeds a fifth token in the node.
        /// </summary>
        public static readonly object FifthPreceedingWhiteSpace = new object();

        /// <summary>
        /// Value is an array of strings which proceeed items in the node.
        /// </summary>
        public static readonly object ListWhiteSpace = new object();

        /// <summary>
        /// Value is an array of strings which proceeed items names in the node.
        /// </summary>
        public static readonly object NamesWhiteSpace = new object();

        /// <summary>
        /// Value is a string which is the name as it appeared verbatim in the source code (for mangled name).
        /// </summary>
        public static readonly object VerbatimImage = new object();

        /// <summary>
        /// Value is a string which represents extra node specific verbatim text.
        /// </summary>
        public static readonly object ExtraVerbatimText = new object();

        /// <summary>
        /// Value is a list of tokens that represent #region and #endregion comments.
        /// </summary>
        public static readonly object CodeRegions = new object();

        /// <summary>
        /// Value is a list of commented lines that are not #region/#endregion comments
        /// </summary>
        public static readonly object NonCodeRegionComments = new object();

        /// <summary>
        /// The tuple expression was constructed without parenthesis.  The value doesn't matter, only the
        /// presence of the metadata indicates the value is set.
        /// </summary>
        public static readonly object IsAltFormValue = new object();

        /// <summary>
        /// Provides an array of strings which are used for verbatim names when multiple names are
        /// involved (e.g. del, global, import, etc...)
        /// </summary>
        public static readonly object VerbatimNames = new object();

        /// <summary>
        /// The node is missing a closing grouping (close paren, close brace, close bracket).
        /// </summary>
        public static readonly object ErrorMissingCloseGrouping = new object();

        /// <summary>
        /// The node is incomplete.  Where the node ends is on a node-by-node basis but it's
        /// well defined for each individual node.
        /// </summary>
        public static readonly object ErrorIncompleteNode = new object();

        public static readonly object VariableReference = new object();

        public static readonly object Variable = new object();

        public static void AddVariableReference(this AstNode4gl node, Genero4glAst ast, bool bindNames, object reference)
        {
            if (bindNames)
            {
                ast.SetAttribute(node, VariableReference, reference);
            }
        }

        public static string GetProceedingWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.PreceedingWhiteSpace);
        }

        public static string GetProceedingWhiteSpaceDefaultNull(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.PreceedingWhiteSpace, null);
        }

        private static string GetWhiteSpace(AstNode4gl node, Genero4glAst ast, object kind, string defaultValue = " ")
        {
            object whitespace;
            if (ast.TryGetAttribute(node, kind, out whitespace))
            {
                return (string)whitespace;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string GetSecondWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.SecondPreceedingWhiteSpace);
        }

        public static string GetSecondWhiteSpaceDefaultNull(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.SecondPreceedingWhiteSpace, null);
        }

        public static string GetThirdWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.ThirdPreceedingWhiteSpace);
        }

        public static string GetThirdWhiteSpaceDefaultNull(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.ThirdPreceedingWhiteSpace, null);
        }

        public static string GetFourthWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.FourthPreceedingWhiteSpace);
        }

        public static string GetFourthWhiteSpaceDefaultNull(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.FourthPreceedingWhiteSpace, null);
        }

        public static string GetFifthWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.FifthPreceedingWhiteSpace);
        }

        public static string GetExtraVerbatimText(this AstNode4gl node, Genero4glAst ast)
        {
            return GetWhiteSpace(node, ast, NodeAttributes.ExtraVerbatimText, null);
        }

        public static bool IsAltForm(this AstNode4gl node, Genero4glAst ast)
        {
            object dummy;
            if (ast.TryGetAttribute(node, NodeAttributes.IsAltFormValue, out dummy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsMissingCloseGrouping(this AstNode4gl node, Genero4glAst ast)
        {
            object dummy;
            if (ast.TryGetAttribute(node, NodeAttributes.ErrorMissingCloseGrouping, out dummy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsIncompleteNode(this AstNode4gl node, Genero4glAst ast)
        {
            object dummy;
            if (ast.TryGetAttribute(node, NodeAttributes.ErrorIncompleteNode, out dummy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string[] GetListWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            object whitespace;
            if (ast.TryGetAttribute(node, NodeAttributes.ListWhiteSpace, out whitespace))
            {
                return (string[])whitespace;
            }
            else
            {
                return null;
            }
        }

        public static string[] GetNamesWhiteSpace(this AstNode4gl node, Genero4glAst ast)
        {
            object whitespace;
            if (ast.TryGetAttribute(node, NodeAttributes.NamesWhiteSpace, out whitespace))
            {
                return (string[])whitespace;
            }
            else
            {
                return null;
            }
        }

        public static string[] GetVerbatimNames(this AstNode4gl node, Genero4glAst ast)
        {
            object names;
            if (ast.TryGetAttribute(node, NodeAttributes.VerbatimNames, out names))
            {
                return (string[])names;
            }
            else
            {
                return null;
            }
        }

        public static string GetVerbatimImage(this AstNode4gl node, Genero4glAst ast)
        {
            object image;
            if (ast.TryGetAttribute(node, NodeAttributes.VerbatimImage, out image))
            {
                return (string)image;
            }
            else
            {
                return null;
            }
        }
    }
}
