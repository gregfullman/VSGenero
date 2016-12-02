using System;
using System.IO;

namespace VSGenero.Analysis.Parsing
{
    public static class GeneroParserFactory
    {
        public static GeneroParser CreateParser(Type t, TextReader reader, IProjectEntry projEntry = null, string filename = null)
        {
            return CreateParser(t, reader, null, projEntry, filename);
        }

        public static GeneroParser CreateParser(Type t, Stream stream, ParserOptions parserOptions = null, IProjectEntry projEntry = null)
        {
            var options = parserOptions ?? ParserOptions.Default;
            var reader = new StreamReader(stream, true);

            return CreateParser(t, reader, options, projEntry);
        }

        public static GeneroParser CreateParser(Type t, TextReader reader, ParserOptions parserOptions, IProjectEntry projEntry = null, string filename = null)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            string filePath = null;
            if (t == null)
            {
                // Try to figure out the type from the projEntry or filename
                if(projEntry != null && projEntry.FilePath != null)
                {
                    t = EditorExtensions.Extensions.GetParserType(projEntry.FilePath);
                    filePath = projEntry.FilePath;
                }

                if(t == null &&
                   filename != null)
                {
                    t = EditorExtensions.Extensions.GetParserType(filename);
                    filePath = filename;
                }

                if(t == null)
                {
                    throw new ArgumentNullException("t");
                }
            }
            else
            {
                if (projEntry.FilePath != null)
                    filePath = projEntry.FilePath;
                else if (filename != null)
                    filePath = filename;
            }

            if (t != typeof(Genero4glParser) &&
               t != typeof(GeneroPerParser))
            {
                throw new ArgumentException("Invalid type received", "t");
            }

            var options = parserOptions ?? ParserOptions.Default;

            GeneroParser result = null;
            Tokenizer tokenizer = new Tokenizer(options.ErrorSink,
                                                (options.Verbatim ? TokenizerOptions.Verbatim : TokenizerOptions.None) | TokenizerOptions.GroupingRecovery,
                                                (span, text) => options.RaiseProcessComment(result, new CommentEventArgs(span, text)));

            tokenizer.Initialize(null, reader, SourceLocation.MinValue);
            tokenizer.IndentationInconsistencySeverity = options.IndentationInconsistencySeverity;

            if(t == typeof(Genero4glParser))
            {
                result = new Genero4glParser(tokenizer,
                    options.ErrorSink ?? ErrorSink.Null,
                    options.Verbatim,
                    options.BindReferences,
                    options
                );
            }
            else if(t == typeof(GeneroPerParser))
            {
                result = new GeneroPerParser(tokenizer,
                    options.ErrorSink ?? ErrorSink.Null,
                    options.Verbatim,
                    options.BindReferences,
                    options
                );
            }

            if (result != null)
            {
                result._projectEntry = projEntry;
                result._filename = filePath;

                result._sourceReader = reader;
            }
            return result;
        }
    }
}
