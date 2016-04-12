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

    public class LayoutContainer : AstNodePer
    {
        private static Dictionary<TokenKind, LayoutContainerType> _tokenToContainerMapping = new Dictionary<TokenKind, LayoutContainerType>
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

        private static Dictionary<LayoutContainerType, List<TokenKind>> _containerTypeMappings = new Dictionary<LayoutContainerType, List<TokenKind>>
        {
            { LayoutContainerType.Layout, new List<TokenKind>
                {
                    TokenKind.FormKeyword, TokenKind.VboxKeyword, TokenKind.HboxKeyword, TokenKind.GroupKeyword, TokenKind.FolderKeyword,
                    TokenKind.GridKeyword, TokenKind.ScrollGridKeyword, TokenKind.StackKeyword, TokenKind.TableKeyword, TokenKind.TreeKeyword
                }
            },
            { LayoutContainerType.HBox, new List<TokenKind>
                {
                    TokenKind.VboxKeyword, TokenKind.HboxKeyword, TokenKind.GroupKeyword, TokenKind.FolderKeyword,
                    TokenKind.GridKeyword, TokenKind.ScrollGridKeyword, TokenKind.StackKeyword, TokenKind.TableKeyword, TokenKind.TreeKeyword
                }
            },
            { LayoutContainerType.VBox, new List<TokenKind>
                {
                    TokenKind.VboxKeyword, TokenKind.HboxKeyword, TokenKind.GroupKeyword, TokenKind.FolderKeyword,
                    TokenKind.GridKeyword, TokenKind.ScrollGridKeyword, TokenKind.StackKeyword, TokenKind.TableKeyword, TokenKind.TreeKeyword
                }
            },
            { LayoutContainerType.Group, new List<TokenKind>
                {
                    TokenKind.VboxKeyword, TokenKind.HboxKeyword, TokenKind.FolderKeyword, TokenKind.GridKeyword,
                    TokenKind.ScrollGridKeyword, TokenKind.StackKeyword, TokenKind.TableKeyword, TokenKind.TreeKeyword
                }
            },
            { LayoutContainerType.Folder, new List<TokenKind> { TokenKind.PageKeyword } },
            { LayoutContainerType.Page, new List<TokenKind>
                {
                    TokenKind.VboxKeyword, TokenKind.HboxKeyword, TokenKind.GroupKeyword, TokenKind.FolderKeyword,
                    TokenKind.GridKeyword, TokenKind.ScrollGridKeyword, TokenKind.TableKeyword, TokenKind.TreeKeyword
                }
            }
        };
    }


    

    
}
