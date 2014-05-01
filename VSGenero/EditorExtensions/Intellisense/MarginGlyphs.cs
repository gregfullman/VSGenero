/* ****************************************************************************
 * 
 * Copyright (c) 2014 Greg Fullman 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 * 
 * Contents of this file are based on the MSDN walkthrough here:
 * http://msdn.microsoft.com/en-us/library/ee361745.aspx
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace VSGenero.EditorExtensions.Intellisense
{
    internal class TodoGlyphFactory : IGlyphFactory
    {
        const double m_glyphSize = 16.0;

        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            // Ensure we can draw a glyph for this marker. 
            if (tag == null || !(tag is TodoTag))
            {
                return null;
            }

            System.Windows.Shapes.Ellipse ellipse = new Ellipse();
            ellipse.Fill = Brushes.LightBlue;
            ellipse.StrokeThickness = 2;
            ellipse.Stroke = Brushes.DarkBlue;
            ellipse.Height = m_glyphSize;
            ellipse.Width = m_glyphSize;

            return ellipse;
        }
    }

    [Export(typeof(IGlyphFactoryProvider))]
    [Name("TodoGlyph")]
    [Order(After = "VsTextMarker")]
    [ContentType(VSGeneroConstants.ContentType4GL)]
    [ContentType(VSGeneroConstants.ContentTypePER)]
    [TagType(typeof(TodoTag))]
    internal sealed class TodoGlyphFactoryProvider : IGlyphFactoryProvider
    {
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new TodoGlyphFactory();
        }
    }
}
