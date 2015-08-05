using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Refactoring
{
    /// <summary>
    /// Provides inputs/UI to the extract method refactoring.  Enables driving of the refactoring programmatically
    /// or via UI.
    /// </summary>
    interface IExtractMethodInput
    {
        /// <summary>
        /// Returns true if the user wants us to expand the selection to cover an entire expression.
        /// </summary>
        bool ShouldExpandSelection();

        /// <summary>
        /// Returns null or an ExtractMethodRequest instance which specifies the options for extracting the method.
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        ExtractMethodRequest GetExtractionInfo(ExtractedMethodCreator creator);

        /// <summary>
        /// Reports that we cannot extract the method and provides a specific reason why the extraction failed.
        /// </summary>
        void CannotExtract(string reason);
    }
}
