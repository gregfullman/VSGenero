using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.VisualStudio.VSCommon.Options
{
    public abstract class BaseOptionsDialog : DialogPage
    {
        protected abstract BaseOptions Options { get; }

        protected abstract BaseOptionsUserControl Control { get; }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                return Control;
            }
        }

        protected override void OnDeactivate(CancelEventArgs e)
        {
            // Commit the pending changes. Save is done via SaveSettingsToStorage
            Options.CommitPendingChanges();
            base.OnDeactivate(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cancel pending changes
            Options.ClearPendingChanges();
            // Update the control with the unchanged data
            Control.InitializeData();
            base.OnClosed(e);
        }

        public override void LoadSettingsFromStorage()
        {
            Options.LoadSettings();
        }

        public override void SaveSettingsToStorage()
        {
            Options.SaveSettingsToStorage();
            // Update the control with the saved data
            Control.InitializeData();
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            Control.InitializeData();
            base.OnActivate(e);
        }
    }
}
