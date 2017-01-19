using System.Windows.Forms;

namespace Microsoft.VisualStudio.VSCommon.Options
{
    public abstract class BaseOptionsUserControl : UserControl
    {
        protected bool IsInitializing;
        protected abstract void Initialize();

        public void InitializeData()
        {
            IsInitializing = true;
            Initialize();
            IsInitializing = false;
        }
    }
}
