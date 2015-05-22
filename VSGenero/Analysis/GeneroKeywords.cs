using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.Analysis
{
    public static class GeneroKeywords
    {
        /// <summary>
        /// Returns true if the specified identifier is a keyword in a
        /// particular version of Genero.
        /// </summary>
        public static bool IsKeyword(
            string keyword,
            GeneroLanguageVersion version = GeneroLanguageVersion.None
        )
        {
            return All(version).Contains(keyword, StringComparer.Ordinal);
        }

        /// <summary>
        /// Returns true if the specified identifier is a statement keyword in a
        /// particular version of Genero.
        /// </summary>
        public static bool IsStatementKeyword(
            string keyword,
            GeneroLanguageVersion version = GeneroLanguageVersion.None
        )
        {
            return Statement(version).Contains(keyword, StringComparer.Ordinal);
        }

        /// <summary>
        /// Returns true if the specified identifier is a statement keyword and
        /// never an expression in a particular version of Genero.
        /// </summary>
        public static bool IsOnlyStatementKeyword(
            string keyword,
            GeneroLanguageVersion version = GeneroLanguageVersion.None
        )
        {
            return Statement(version)
                .Except(Expression(version))
                .Contains(keyword, StringComparer.Ordinal);
        }

        /// <summary>
        /// Returns a sequence of all keywords in a particular version of
        /// Genero.
        /// </summary>
        public static IEnumerable<string> All(GeneroLanguageVersion version = GeneroLanguageVersion.None)
        {
            return Expression(version).Union(Statement(version));
        }

        /// <summary>
        /// Returns a sequence of all keywords usable in an expression in a
        /// particular version of Genero.
        /// </summary>
        public static IEnumerable<string> Expression(GeneroLanguageVersion version = GeneroLanguageVersion.None)
        {
            yield return null;
            // TODO:
        }

        /// <summary>
        /// Retuns a sequence of all keywords usable as a statement in a
        /// particular version of Genero.
        /// </summary>
        public static IEnumerable<string> Statement(GeneroLanguageVersion version = GeneroLanguageVersion.None)
        {
            return GeneroAst.ValidStatementKeywords.Select(x => Tokens.TokenKinds[x]);
        }

        /// <summary>
        /// Returns a sequence of all keywords that are invalid outside of
        /// function definitions in a particular version of Genero.
        /// </summary>
        public static IEnumerable<string> InvalidOutsideFunction(
            GeneroLanguageVersion version = GeneroLanguageVersion.None
        )
        {
            yield return null;
            // TODO:
        }
    }
}
