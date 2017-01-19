using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VSCommon.Options;
using System.Runtime.InteropServices;
using System;

namespace VSGenero.Options
{
    [ComVisible(true)]
    public class Genero4GLAdvancedOptionsPage : BaseOptionsDialog
    {
        private Genero4GLAdvancedOptionsControl _control;

        protected override BaseOptionsUserControl Control
        {
            get
            {
                if (_control == null)
                    _control = new Genero4GLAdvancedOptionsControl();
                return _control;
            }
        }

        protected override BaseOptions Options
        {
            get
            {
                return VSGeneroPackage.Instance.AdvancedOptions4GL;
            }
        }
    }
}
