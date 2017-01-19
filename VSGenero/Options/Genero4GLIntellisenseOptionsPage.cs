using Microsoft.VisualStudio.VSCommon.Options;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace VSGenero.Options
{
    [ComVisible(true)]
    public class Genero4GLIntellisenseOptionsPage : BaseOptionsDialog
    {
        private Genero4GLIntellisenseOptionsControl _control;

        protected override BaseOptions Options { get { return VSGeneroPackage.Instance.IntellisenseOptions4GL; }}

        protected override BaseOptionsUserControl Control
        {
            get
            {
                if (_control == null)
                    _control = new Genero4GLIntellisenseOptionsControl();
                return _control;
            }
        }
    }
}
