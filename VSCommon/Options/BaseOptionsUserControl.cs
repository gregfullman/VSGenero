using System.Windows.Forms;

namespace Microsoft.VisualStudio.VSCommon.Options
{
    public class BaseOptionsUserControl : UserControl
    {
        protected bool IsInitializing;
        protected virtual void Initialize() { }

        public void InitializeData()
        {
            IsInitializing = true;
            Initialize();
            IsInitializing = false;
        }
    }
}
