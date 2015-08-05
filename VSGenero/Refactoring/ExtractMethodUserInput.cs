using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VSGenero.Refactoring
{
    class ExtractMethodUserInput : IExtractMethodInput
    {
        private readonly IServiceProvider _serviceProvider;

        public ExtractMethodUserInput(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool ShouldExpandSelection()
        {
            var res = MessageBox.Show(@"The selected text does not cover an entire expression.

Would you like the selection to be extended to a valid expression?",
                        "Expand extract method selection?",
                        MessageBoxButton.YesNo
                    );

            return res == MessageBoxResult.Yes;
        }


        public ExtractMethodRequest GetExtractionInfo(ExtractedMethodCreator previewer)
        {
            var requestView = new ExtractMethodRequestView(_serviceProvider, previewer);
            var dialog = new ExtractMethodDialog(requestView);

            bool res = dialog.ShowModal() ?? false;
            if (res)
            {
                return requestView.GetRequest();
            }

            return null;
        }

        public void CannotExtract(string reason)
        {
            MessageBox.Show(reason, "Cannot extract method", MessageBoxButton.OK);
        }
    }
}
