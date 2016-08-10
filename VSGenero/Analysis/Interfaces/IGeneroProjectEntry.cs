using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis.Parsing;

namespace VSGenero.Analysis
{
    public interface IGeneroProjectEntry : IProjectEntry
    {
        IEnumerable<IGeneroProjectEntry> GetIncludedFiles();

        bool IsOpen { get; set; }

        IGeneroProject ParentProject { get; }

        string ModuleName { get; }

        /// <summary>
        /// Returns the last parsed AST.
        /// </summary>
        GeneroAst Analysis
        {
            get;
        }

        event EventHandler<EventArgs> OnNewParseTree;
        event EventHandler<EventArgs> OnNewAnalysis;

        /// <summary>
        /// Informs the project entry that a new tree will soon be available and will be provided by
        /// a call to UpdateTree.  Calling this method will cause WaitForCurrentTree to block until
        /// UpdateTree has been called.
        /// 
        /// Calls to BeginParsingTree should be balanced with calls to UpdateTree.
        /// 
        /// This method is thread safe.
        /// </summary>
        void BeginParsingTree();

        void UpdateTree(GeneroAst ast, IAnalysisCookie fileCookie);
        void GetTreeAndCookie(out GeneroAst ast, out IAnalysisCookie cookie);
        void UpdateIncludesAndImports(string filename, GeneroAst ast);
        bool DetectCircularImports();
        /// <summary>
        /// Returns the current tree if no parsing is currently pending, otherwise waits for the 
        /// current parse to finish and returns the up-to-date tree.
        /// </summary>
        GeneroAst WaitForCurrentTree(int timeout = -1);

        void SetProject(IGeneroProject project);

        IAnalysisCookie Cookie { get; }

        bool CanErrorCheck { get; }
        bool PreventErrorCheck { get; set; }

        string GetFunctionInfo(string functionName);
        void SetFunctionInfo(string functioName, string info);
    }
}
