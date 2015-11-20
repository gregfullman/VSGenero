using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VSGenero.ProductivityTools
{
    [Export(typeof(IMouseProcessorProvider))]
    [Name("Genero4glMouseProcessor")]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [TextViewRole(PredefinedTextViewRoles.EmbeddedPeekTextView)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
#if DEV14_OR_LATER
    [TextViewRole(PredefinedTextViewRoles.Printable)]
#endif
    public sealed class Genero4glMouseProcessorProvider : IMouseProcessorProvider
    {
        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            Genero4glMouseProcessor mouseProcessor;
            if(!wpfTextView.Properties.TryGetProperty(typeof(Genero4glMouseProcessor), out mouseProcessor))
            {
                mouseProcessor = new Genero4glMouseProcessor(wpfTextView);
                wpfTextView.Properties.AddProperty(typeof(Genero4glMouseProcessor), mouseProcessor);
            }
            return mouseProcessor;
        }
    }

    public class Genero4glMouseProcessor : MouseProcessorBase
    {
        private readonly IWpfTextView _view;

        public Genero4glMouseProcessor(IWpfTextView view)
        {
            _view = view;
        }

        public int MousePosition { get; private set; }

        public override void PreprocessMouseMove(MouseEventArgs e)
        {
            ITextViewLineCollection textViewLines = _view.TextViewLines;
            if (textViewLines != null)
            {
                Point position = e.GetPosition(this._view.VisualElement);
                position.X += this._view.ViewportLeft;
                position.Y += this._view.ViewportTop;
                ITextViewLine textViewLineContainingYCoordinate = textViewLines.GetTextViewLineContainingYCoordinate(position.Y);
                if(textViewLineContainingYCoordinate != null)
                {
                    MousePosition = textViewLineContainingYCoordinate.Start.Position;
                }
            }

            base.PreprocessMouseMove(e);
        }
    }
}
