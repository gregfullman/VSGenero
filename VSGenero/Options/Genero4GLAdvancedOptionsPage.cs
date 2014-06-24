using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Options
{
    [ComVisible(true)]
    public class Genero4GLAdvancedOptionsPage : GeneroDialogPage
    {
        private bool _showFunctionParametersChanged;
        private Genero4GLAdvancedOptionsControl _window;
        private bool _showFunctionParameters;

        public Genero4GLAdvancedOptionsPage()
            : base("Advanced")
        {
            _showFunctionParametersChanged = false;
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                if (_window == null)
                {
                    _window = new Genero4GLAdvancedOptionsControl();
                }
                return _window;
            }
        }

        public bool ShowFunctionParametersInList
        {
            get { return _showFunctionParameters; }
            set 
            {
                if (_showFunctionParameters != value)
                {
                    _showFunctionParameters = value;
                    _showFunctionParametersChanged = true;
                }
            }
        }

        public bool ShowFunctionParametersChanged
        {
            get
            {
                return _showFunctionParametersChanged;
            }
        }

        public void SetChangesApplied()
        {
            _showFunctionParametersChanged = false;
        }

        public override void ResetSettings()
        {
            _showFunctionParameters = true;
        }

        private const string ShowFunctionParametersSetting = "ShowFunctionParametersInList";

        public override void LoadSettingsFromStorage()
        {
            _showFunctionParameters = LoadBool(ShowFunctionParametersSetting) ?? true;
            _showFunctionParametersChanged = false;
        }

        public override void SaveSettingsToStorage()
        {
            SaveBool(ShowFunctionParametersSetting, _showFunctionParameters);
        }
    }
}
