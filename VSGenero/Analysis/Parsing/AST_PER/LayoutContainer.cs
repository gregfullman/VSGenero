using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Analysis.Parsing.AST_PER
{
    public enum LayoutContainerType
    {
        Layout,
        Form,
        VBox,
        HBox,
        Group,
        Folder,
        Grid,
        ScrollGrid,
        Stack,
        Table,
        Tree,
        Page
    }

    public static class LayoutContainerHelpers
    {
        public static Dictionary<TokenKind, LayoutContainerType> TokenToContainerMapping = new Dictionary<TokenKind, LayoutContainerType>
        {
            { TokenKind.LayoutKeyword, LayoutContainerType.Layout },
            { TokenKind.FormKeyword, LayoutContainerType.Form },
            { TokenKind.VboxKeyword, LayoutContainerType.VBox },
            { TokenKind.HboxKeyword, LayoutContainerType.HBox },
            { TokenKind.GroupKeyword, LayoutContainerType.Group },
            { TokenKind.FolderKeyword, LayoutContainerType.Folder },
            { TokenKind.GridKeyword, LayoutContainerType.Grid },
            { TokenKind.ScrollGridKeyword, LayoutContainerType.ScrollGrid },
            { TokenKind.StackKeyword, LayoutContainerType.Stack },
            { TokenKind.TableKeyword, LayoutContainerType.Table },
            { TokenKind.TreeKeyword, LayoutContainerType.Tree },
            { TokenKind.PageKeyword, LayoutContainerType.Page }
        };

        private static Dictionary<LayoutContainerAttributeType, List<LayoutContainerType>> _attributesToAllowedContainers = new Dictionary<LayoutContainerAttributeType, List<LayoutContainerType>>
        {
            { LayoutContainerAttributeType.Comment, new List<LayoutContainerType>
                {
                    LayoutContainerType.HBox, LayoutContainerType.VBox, LayoutContainerType.Group, LayoutContainerType.Folder, LayoutContainerType.Page,
                    LayoutContainerType.Grid, LayoutContainerType.ScrollGrid, LayoutContainerType.Table
                }
            },
            { LayoutContainerAttributeType.FontPitch, new List<LayoutContainerType>
                {
                    LayoutContainerType.HBox, LayoutContainerType.VBox, LayoutContainerType.Group, LayoutContainerType.Folder, LayoutContainerType.Grid,
                    LayoutContainerType.ScrollGrid, LayoutContainerType.Table
                }
            },
            { LayoutContainerAttributeType.Hidden, new List<LayoutContainerType>
                {
                    LayoutContainerType.HBox, LayoutContainerType.VBox, LayoutContainerType.Group, LayoutContainerType.Folder, LayoutContainerType.Page,
                    LayoutContainerType.Grid, LayoutContainerType.ScrollGrid, LayoutContainerType.Table
                }
            },
            { LayoutContainerAttributeType.Style, new List<LayoutContainerType>
                {
                    LayoutContainerType.HBox, LayoutContainerType.VBox, LayoutContainerType.Group, LayoutContainerType.Folder, LayoutContainerType.Page,
                    LayoutContainerType.Grid, LayoutContainerType.ScrollGrid, LayoutContainerType.Table
                }
            },
            { LayoutContainerAttributeType.Splitter, new List<LayoutContainerType>
                {
                    LayoutContainerType.HBox, LayoutContainerType.VBox
                }
            },
            { LayoutContainerAttributeType.Tag, new List<LayoutContainerType>
                {
                    LayoutContainerType.HBox, LayoutContainerType.VBox, LayoutContainerType.Group, LayoutContainerType.Folder, LayoutContainerType.Page,
                    LayoutContainerType.Grid, LayoutContainerType.ScrollGrid, LayoutContainerType.Table
                }
            },
            { LayoutContainerAttributeType.Text, new List<LayoutContainerType>
                {
                    LayoutContainerType.Group, LayoutContainerType.Page
                }
            },
            { LayoutContainerAttributeType.Action, new List<LayoutContainerType>
                {
                    LayoutContainerType.Page,
                }
            },
            { LayoutContainerAttributeType.Image, new List<LayoutContainerType>
                {
                    LayoutContainerType.Page,
                }
            },
            { LayoutContainerAttributeType.WantFixedPageSize, new List<LayoutContainerType>
                {
                    LayoutContainerType.ScrollGrid,
                }
            },
            { LayoutContainerAttributeType.AggregateText, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.DoubleClick, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.UnhidableColumns, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.UnmovableColumns, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.UnsizableColumns, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.UnsortableColumns, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.Width, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            },
            { LayoutContainerAttributeType.Height, new List<LayoutContainerType>
                {
                    LayoutContainerType.Table,
                }
            }
        };

        private static Dictionary<TokenKind, LayoutContainerAttributeType> _layoutAttributes = new Dictionary<TokenKind, LayoutContainerAttributeType>
        {
            { TokenKind.CommentKeyword, LayoutContainerAttributeType.Comment },
            { TokenKind.FontPitchKeyword, LayoutContainerAttributeType.FontPitch },
            { TokenKind.HiddenKeyword, LayoutContainerAttributeType.Hidden },
            { TokenKind.StyleKeyword, LayoutContainerAttributeType.Style },
            { TokenKind.SplitterKeyword, LayoutContainerAttributeType.Splitter },
            { TokenKind.TagKeyword, LayoutContainerAttributeType.Tag },
            { TokenKind.TextKeyword, LayoutContainerAttributeType.Text },
            { TokenKind.ActionKeyword, LayoutContainerAttributeType.Action },
            { TokenKind.ImageKeyword, LayoutContainerAttributeType.Image },
            { TokenKind.WantFixedPageSizeKeyword, LayoutContainerAttributeType.WantFixedPageSize },
            { TokenKind.AggregateTextKeyword, LayoutContainerAttributeType.AggregateText },
            { TokenKind.DoubleClickKeyword, LayoutContainerAttributeType.DoubleClick },
            { TokenKind.UnhidableColumnsKeyword, LayoutContainerAttributeType.UnhidableColumns },
            { TokenKind.UnmovableColumnsKeyword, LayoutContainerAttributeType.UnmovableColumns },
            { TokenKind.UnsizableColumnsKeyword, LayoutContainerAttributeType.UnsizableColumns },
            { TokenKind.UnsortableColumnsKeyword, LayoutContainerAttributeType.UnsortableColumns },
            { TokenKind.WidthKeyword, LayoutContainerAttributeType.Width },
            { TokenKind.HeightKeyword, LayoutContainerAttributeType.Height }
        };

        public static bool IsAllowedLayoutAttribute(IParser parser, LayoutContainerType container, LayoutContainerAttribute attrib)
        {
            LayoutContainerAttributeType attribType;
            if (_layoutAttributes.TryGetValue(parser.PeekToken().Kind, out attribType))
            {
                parser.NextToken();
                attrib.StartIndex = parser.Token.Span.Start;
                attrib.AttributeType = attribType;

                List<LayoutContainerType> allowedContainers;
                if(!_attributesToAllowedContainers.TryGetValue(attribType, out allowedContainers))
                {
                    parser.ReportSyntaxError("Invalid layout attribute found.");
                    return false;
                }

                if(!allowedContainers.Contains(container))
                {
                    parser.ReportSyntaxError("Layout attribute not allowed in this layout container.");
                    return false;
                }

                return true;
            }
            return false;
        }
    }

    public class LayoutContainer : AstNodePer
    {
        public LayoutContainerType Type { get; private set; }


    }

    public enum LayoutContainerAttributeType
    {
        Comment,
        FontPitch,
        Hidden,
        Style,
        Splitter,
        Tag,
        Text,
        Action,
        Image,
        WantFixedPageSize,
        AggregateText,
        DoubleClick,
        UnhidableColumns,
        UnmovableColumns,
        UnsizableColumns,
        UnsortableColumns,
        Width,
        Height
    }

    public enum FontPitchOption
    {
        Fixed,
        Variable
    }

    public enum HiddenOption
    {
        Visible,
        Hidden,
        HiddenUser
    }

    public enum DimensionScale
    {
        Characters,
        Columns,
        Points,
        Pixels
    }

    public class LayoutContainerAttribute : AstNodePer
    {
        public LayoutContainerAttributeType AttributeType { get; internal set; }

        public StringExpressionNode Comment { get; private set; }
        public FontPitchOption FontPitch { get; private set; }
        public HiddenOption Hidden { get; private set; }
        public StringExpressionNode Style { get; private set; }
        public StringExpressionNode Tag { get; private set; }
        public TokenExpressionNode Action { get; private set; }
        public StringExpressionNode Image { get; private set; }
        public StringExpressionNode Text { get; private set; }
        public bool WantFixedPageSize { get; private set; }
        public StringExpressionNode AggregateText { get; private set; }
        public TokenExpressionNode DoubleClick { get; private set; }
        public DimensionScale Scale { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TokenExpressionNode ParentIdColumn { get; private set; }
        public TokenExpressionNode IdColumn { get; private set; }
        public TokenExpressionNode ExpandedColumn { get; private set; }
        public TokenExpressionNode IsNodeColumn { get; private set; }
        public StringExpressionNode ImageExpanded { get; private set; }
        public StringExpressionNode ImageCollapsed { get; private set; }
        public StringExpressionNode ImageLeaf { get; private set; }


        public static bool TryParseAttribute(IParser parser, LayoutContainerType containingType, out LayoutContainerAttribute attrib)
        {
            attrib = new LayoutContainerAttribute();
            bool result = true;

            if (!LayoutContainerHelpers.IsAllowedLayoutAttribute(parser, containingType, attrib))
                return false;

            switch(parser.PeekToken().Kind)
            {
                case TokenKind.CommentKeyword:
                    
                    break;
                case TokenKind.FontPitchKeyword:
                    
                    break;
                case TokenKind.HiddenKeyword:
                    
                    break;
                case TokenKind.StyleKeyword:
                    
                    break;
                case TokenKind.SplitterKeyword:
                    
                    break;
                case TokenKind.TagKeyword:
                    
                    break;
                case TokenKind.TextKeyword:
                    
                    break;
                case TokenKind.ActionKeyword:
                    
                    break;
                case TokenKind.ImageKeyword:
                    
                    break;
                case TokenKind.WantFixedPageSizeKeyword:
                    
                    break;
                case TokenKind.AggregateTextKeyword:
                    
                    break;
                case TokenKind.DoubleClickKeyword:
                    
                    break;
                case TokenKind.UnhidableColumnsKeyword:
                    
                    break;
                case TokenKind.UnmovableColumnsKeyword:
                    
                    break;
                case TokenKind.UnsizableColumnsKeyword:
                    
                    break;
                case TokenKind.UnsortableColumnsKeyword:
                    
                    break;
                case TokenKind.WidthKeyword:
                    
                    break;
                case TokenKind.HeightKeyword:
                    
                    break;
            }

            return result;
        }
    }
    

    
}
