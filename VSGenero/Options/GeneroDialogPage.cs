using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
namespace VSGenero.Options
{
    public class GeneroDialogPage : DialogPage
    {
        private readonly string _category;
        private const string _optionsKey = "Options";

        [Obsolete("Designer only", true)]
        public GeneroDialogPage()
        {
        }

        public GeneroDialogPage(string category)
        {
            _category = category;
        }

        internal void SaveBool(string name, bool value) {
            SaveString(name, value.ToString());
        }

        internal void SaveInt(string name, int value) {
            SaveString(name, value.ToString());
        }

        internal void SaveString(string name, string value) {
            SaveString(name, value, _category);
        }

        internal static void SaveString(string name, string value, string cat) {
            using (var pythonKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(VSGeneroConstants.BaseRegistryKey))
            {
                if (pythonKey == null) return;
                using (var optionsKey = pythonKey.CreateSubKey(_optionsKey))
                {
                    if (optionsKey == null) return;
                    using (var categoryKey = optionsKey.CreateSubKey(cat))
                    {
                        if (categoryKey == null) return;
                        categoryKey.SetValue(name, value, Microsoft.Win32.RegistryValueKind.String);
                    }
                }
            }
        }

        internal void SaveEnum<T>(string name, T value) where T : struct {
            SaveString(name, value.ToString());
        }

        internal void SaveDateTime(string name, DateTime value) {
            SaveString(name, value.ToString(CultureInfo.InvariantCulture));
        }

        internal int? LoadInt(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            int val;
            if (int.TryParse(res, out val)) {
                return val;
            }
            return null;
        }

        internal bool? LoadBool(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            bool val;
            if (bool.TryParse(res, out val)) {
                return val;
            }
            return null;
        }

        internal string LoadString(string name) {
            return LoadString(name, _category);
        }

        internal static string LoadString(string name, string cat) {
            using (var pythonKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(VSGeneroConstants.BaseRegistryKey))
            {
                if (pythonKey == null) return null;
                using (var optionsKey = pythonKey.CreateSubKey(_optionsKey))
                {
                    if (optionsKey == null) return null;
                    using (var categoryKey = optionsKey.CreateSubKey(cat)) {
                        return categoryKey?.GetValue(name) as string;
                    }
                }
            }
        }

        internal T? LoadEnum<T>(string name) where T : struct {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            T enumRes;
            if (Enum.TryParse<T>(res, out enumRes)) {
                return enumRes;
            }
            return null;
        }

        internal DateTime? LoadDateTime(string name) {
            string res = LoadString(name);
            if (res == null) {
                return null;
            }

            DateTime dateRes;
            if (DateTime.TryParse(res, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateRes)) {
                return dateRes;
            }
            return null;
        }
    }
}
