
using System;
using VSGenero.Analysis.Parsing.AST_PER;

namespace VSGenero.Analysis.Parsing
{
    public class GeneroPerParser : GeneroParser
    {
        public GeneroPerParser(Tokenizer tokenizer, ErrorSink errorSink, bool verbatim, bool bindRefs, ParserOptions options)
            : base(tokenizer, errorSink, verbatim, bindRefs, options)
        {
        }

        public override GeneroLanguageVersion LanguageVersion
        {
            get
            {
                return GeneroLanguageVersion.None;
            }
        }

        protected override GeneroAst CreateAst()
        {
            ModuleNode moduleNode = new ModuleNode();
            var ast = new GeneroPerAst(moduleNode, _tokenizer.GetLineLocations(), LanguageVersion, _projectEntry, _filename);
            UpdateNodeAndTree(moduleNode, ast);
            return ast;
        }
    }
}
