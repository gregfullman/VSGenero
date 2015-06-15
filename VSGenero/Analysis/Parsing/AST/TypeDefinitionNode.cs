using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST
{
    /// <summary>
    /// identifier 
    ///     <see cref="TypeReference"/> [<see cref="AttributeSpecifier"/>]
    ///     |
    ///     <see cref="RecordDefinitionNode"/>
    /// 
    /// For more info, see: http://www.4js.com/online_documentation/fjs-fgl-manual-html/index.html#c_fgl_user_types_003.html
    /// </summary>
    public class TypeDefinitionNode : AstNode, IAnalysisResult
    {
        public string Identifier { get; private set; }

        private bool _isPublic;
        public bool IsPublic { get { return _isPublic; } }

        public bool CanGetValueFromDebugger
        {
            get { return false; }
        }

        public static bool TryParseDefine(IParser parser, out TypeDefinitionNode defNode, bool isPublic = false)
        {
            defNode = null;
            bool result = false;
            if (parser.PeekToken(TokenCategory.Identifier) || parser.PeekToken(TokenCategory.Keyword))
            {
                defNode = new TypeDefinitionNode();
                result = true;
                parser.NextToken();
                defNode.StartIndex = parser.Token.Span.Start;
                defNode._location = parser.TokenLocation;
                defNode.Identifier = parser.Token.Token.Value.ToString();
                defNode._isPublic = isPublic;

                TypeReference typeRef;
                if (TypeReference.TryParseNode(parser, out typeRef, false, isPublic))
                {
                    defNode.Children.Add(typeRef.StartIndex, typeRef);
                    if (defNode.Children.All(x => x.Value.IsComplete))
                    {
                        defNode.EndIndex = defNode.Children.Last().Value.EndIndex;
                        defNode.IsComplete = true;
                    }
                }
                else
                {
                    parser.ReportSyntaxError("Invalid syntax found in type definition.");
                }
            }
            return result;
        }

        private string _scope;
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
            }
        }

        public string Name
        {
            get { return Identifier; }
        }

        public override string Documentation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Scope))
                {
                    sb.AppendFormat("({0}) ", Scope);
                }
                sb.Append(Name);
                if (Children.Count == 1 && Children[Children.Keys[0]] is TypeReference)
                {
                    sb.AppendFormat(" {0}", (Children[Children.Keys[0]] as TypeReference).ToString());
                }
                return sb.ToString();
            }
        }


        public int LocationIndex
        {
            get { return StartIndex; }
        }

        private LocationInfo _location;
        public LocationInfo Location { get { return _location; } }

        public IAnalysisResult GetMember(string name, GeneroAst ast, out IGeneroProject definingProject, out IProjectEntry projEntry)
        {
            definingProject = null;
            projEntry = null;
            if (Children.Count == 1 &&
               Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).GetMember(name, ast, out definingProject, out projEntry);
            }
            return null;
        }

        public IEnumerable<MemberResult> GetMembers(GeneroAst ast, MemberType memberType)
        {
            if(Children.Count == 1 &&
               Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).GetMembers(ast, memberType);
            }
            return null;
        }

        public bool HasChildFunctions(GeneroAst ast)
        {
            if (Children.Count == 1 &&
               Children[Children.Keys[0]] is TypeReference)
            {
                return (Children[Children.Keys[0]] as TypeReference).HasChildFunctions(ast);
            }
            return false;
        }
    }
}
