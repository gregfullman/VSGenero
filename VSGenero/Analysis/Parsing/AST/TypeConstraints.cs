using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    public static class TypeConstraints
    {
        private static object _objLock = new object();
        private static Dictionary<TokenKind, TypeConstraint> _typeConstraints;
        public static Dictionary<TokenKind, TypeConstraint> Constraints
        {
            get
            {
                lock (_objLock)
                {
                    if (_typeConstraints == null)
                    {
                        _typeConstraints = new Dictionary<TokenKind, TypeConstraint>();
                        var charConstraint = new TypeConstraint(TokenKind.CharKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 0, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 0, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 0, true)
                    });
                        _typeConstraints.Add(TokenKind.CharKeyword, charConstraint);
                        _typeConstraints.Add(TokenKind.CharacterKeyword, charConstraint);
                        _typeConstraints.Add(TokenKind.VarcharKeyword, new TypeConstraint(TokenKind.VarcharKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 0, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 0, true),
                       new TypeConstraintPiece(TokenKind.Comma, 1, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 1, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 0, true)
                    }));
                        _typeConstraints.Add(TokenKind.DatetimeKeyword, new TypeConstraint(TokenKind.DatetimeKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenCategory.Keyword, 0, false),
                       new TypeConstraintPiece(TokenKind.ToKeyword, 0, false),
                       new TypeConstraintPiece(TokenCategory.Keyword, 0, false)
                    }));
                        _typeConstraints.Add(TokenKind.FractionKeyword, new TypeConstraint(TokenKind.FractionKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 0, false),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 0, false),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 0, false)
                    }));
                        _typeConstraints.Add(TokenKind.IntervalKeyword, new TypeConstraint(TokenKind.IntervalKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenCategory.Keyword, 0, false),
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 1, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 1, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 1, true),
                       new TypeConstraintPiece(TokenKind.ToKeyword, 0, false),
                       new TypeConstraintPiece(TokenCategory.Keyword, 0, false),
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 1, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 1, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 1, true)
                    }));
                        _typeConstraints.Add(TokenKind.FloatKeyword, new TypeConstraint(TokenKind.FloatKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 0, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 0, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 0, true)
                    }));
                        var decConstraint = new TypeConstraint(TokenKind.DecimalKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 0, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 0, true),
                       new TypeConstraintPiece(TokenKind.Comma, 1, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 1, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 0, true)
                    });
                        _typeConstraints.Add(TokenKind.DecimalKeyword, decConstraint);
                        _typeConstraints.Add(TokenKind.DecKeyword, decConstraint);
                        _typeConstraints.Add(TokenKind.NumericKeyword, decConstraint);
                        _typeConstraints.Add(TokenKind.MoneyKeyword, new TypeConstraint(TokenKind.MoneyKeyword, new[] 
                    {
                       new TypeConstraintPiece(TokenKind.LeftParenthesis, 0, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 0, true),
                       new TypeConstraintPiece(TokenKind.Comma, 1, true),
                       new TypeConstraintPiece(TokenCategory.NumericLiteral, 1, true),
                       new TypeConstraintPiece(TokenKind.RightParenthesis, 0, true)
                    }));
                    }
                }
                return _typeConstraints;
            }
        }
    }

    public class TypeConstraint
    {
        public TokenKind Type { get; private set; }
        public List<TypeConstraintPiece> Pieces { get; private set; }

        public TypeConstraint(TokenKind type, IEnumerable<TypeConstraintPiece> pieces)
        {
            Type = type;
            Pieces = new List<TypeConstraintPiece>(pieces);
        }
    }

    public class TypeConstraintPiece
    {
        public object KindOrCategory { get; private set; }
        public int Group { get; private set; }
        public bool Optional { get; private set; }

        public TypeConstraintPiece(object kindOrCategory, int group, bool optional)
        {
            KindOrCategory = kindOrCategory;
            Group = group;
            Optional = optional;
        }
    }
}
