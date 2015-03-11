using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VSGenero.EditorExtensions.Intellisense
{
    [Export(typeof(IUIElementProvider<CompletionSet, ICompletionSession>))]
    [Name("Python Completion UI Provider")]
    [Order(Before = "Default Completion Presenter")]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    internal class CompletionUIElementProvider : IUIElementProvider<CompletionSet, ICompletionSession>
    {
        [ImportMany]
        internal List<Lazy<IUIElementProvider<CompletionSet, ICompletionSession>, IOrderableContentTypeMetadata>> UnOrderedCompletionSetUIElementProviders { get; set; }
        private static bool _isPreSp1 = CheckPreSp1();

        private static bool CheckPreSp1()
        {
            var attrs = typeof(VSConstants).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (attrs.Length > 0 && ((AssemblyFileVersionAttribute)attrs[0]).Version == "10.0.30319.1")
            {
                // http://pytools.codeplex.com/workitem/537
                // http://connect.microsoft.com/VisualStudio/feedback/details/550886/visual-studio-2010-crash-when-the-source-file-contains-non-unicode-characters
                // pre-SP1 cannot handle us wrapping this up, so just don't offer this functionality pre-SP1.
                return true;
            }
            return false;
        }

        public CompletionUIElementProvider()
        {
        }

        public UIElement GetUIElement(CompletionSet itemToRender, ICompletionSession context, UIElementType elementType)
        {
            var orderedProviders = Orderer.Order(UnOrderedCompletionSetUIElementProviders);
            foreach (var presenterProviderExport in orderedProviders)
            {

                foreach (var contentType in presenterProviderExport.Metadata.ContentTypes)
                {
                    if (VSGeneroPackage.Instance.ContentType.IsOfType(contentType))
                    {
                        if (presenterProviderExport.Value.GetType() == typeof(CompletionUIElementProvider))
                        {
                            // don't forward to ourselves...
                            continue;
                        }

                        var res = presenterProviderExport.Value.GetUIElement(itemToRender, context, elementType);
                        if (res != null)
                        {
                            if (_isPreSp1)
                            {
                                return res;
                            }

                            return new CompletionControl(res, context);
                        }
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Metadata which includes Ordering and Content Types
    /// 
    /// New in 1.1.
    /// </summary>
    public interface IOrderableContentTypeMetadata : IContentTypeMetadata, IOrderable
    {
    }

    /// <summary>
    /// New in 1.1
    /// </summary>
    public interface IContentTypeMetadata
    {
        IEnumerable<string> ContentTypes { get; }
    }
}
