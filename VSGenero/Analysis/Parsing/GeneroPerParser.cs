
using VSGenero.Analysis.Parsing.AST_PER;
using VSGenero.External.Analysis.Parsing;

namespace VSGenero.Analysis.Parsing
{
    public class GeneroPerParser : GeneroParser
    {
        public GeneroPerParser(Tokenizer tokenizer, ErrorSink errorSink, bool verbatim, bool bindRefs, ParserOptions options)
            : base(tokenizer, errorSink, verbatim, bindRefs, options)
        {
        }

        protected override GeneroAst CreateAst()
        {
            ModuleNode moduleNode = new ModuleNode();
            var ast = new GeneroPerAst(moduleNode, _tokenizer.GetLineLocations(), GeneroLanguageVersion.None, _projectEntry, _filename);
            UpdateNodeAndTree(moduleNode, ast);
            return ast;
        }
    }
}
