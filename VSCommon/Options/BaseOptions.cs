using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.VisualStudio.VSCommon.Options
{
    public class OptionsChangedEventArgs : EventArgs
    {
        public HashSet<string> ChangedOptions { get; set; }
    }

    public abstract class BaseOptions
    {
        public event EventHandler<OptionsChangedEventArgs> OptionsChanged;

        private readonly string _category;
        private readonly string _baseKey;
        private const string _optionsKey = "Options";

        public bool SettingsLoaded { get; private set; }

        protected BaseOptions(string baseKey, string category, bool loadSettings = true)
        {
            _baseKey = baseKey;
            _category = category;
            if(loadSettings)
                LoadSettings();
        }

        private Dictionary<string, object> _pendingChanges = new Dictionary<string, object>();
        private HashSet<string> _committedChanges = new HashSet<string>();
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        private Dictionary<string, Func<object, object>> _saveTransforms;
        public Dictionary<string, Func<object, object>> SaveTransforms
        {
            get
            {
                if (_saveTransforms == null)
                    _saveTransforms = new Dictionary<string, Func<object, object>>();
                return _saveTransforms;
            }
        }

        private Dictionary<string, Func<object, object>> _loadTransforms;
        public Dictionary<string, Func<object, object>> LoadTransforms
        {
            get
            {
                if (_loadTransforms == null)
                    _loadTransforms = new Dictionary<string, Func<object, object>>();
                return _loadTransforms;
            }
        }

        public T GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default(T);
            var value = this.GetValue(key);
            if (value is T)
                return (T)value;
            return default(T);
        }

        private object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (this._properties.ContainsKey(key))
                return this._properties[key];
            return null;
        }

        public void SetPendingValue(string key, object value)
        {
            if (!this._pendingChanges.ContainsKey(key))
                this._pendingChanges.Add(key, value);
            else
                this._pendingChanges[key] = value;
        }

        public void CommitPendingChanges()
        {
            foreach (var change in _pendingChanges)
                if (_properties.ContainsKey(change.Key))
                {
                    _properties[change.Key] = change.Value;
                    _committedChanges.Add(change.Key);
                }
            ClearPendingChanges();
        }

        public void ClearPendingChanges()
        {
            _pendingChanges.Clear();
        }

        protected abstract void LoadSettingsFromStorage();

        public void LoadSettings()
        {
            if(!SettingsLoaded)
            {
                LoadSettingsFromStorage();
                SettingsLoaded = true;
            }
        }
        #region Save methods
        public virtual void SaveSettingsToStorage()
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            foreach(var prop in _properties)
            {
                object propVal = prop.Value;
                Func<object, object> saveTransform;
                if (SaveTransforms.TryGetValue(prop.Key, out saveTransform))
                    propVal = saveTransform(propVal);

                if (propVal is bool)
                {
                    SaveBool(prop.Key, (bool)propVal);
                }
                else if(propVal is int)
                {
                    SaveInt(prop.Key, (int)propVal);
                }
                else if(propVal is DateTime)
                {
                    SaveDateTime(prop.Key, (DateTime)propVal);
                }
                else if(propVal != null)
                {
                    // save as string
                    SaveString(prop.Key, propVal.ToString());
                }
            }

            SettingsLoaded = false;
            LoadSettings();

            if (OptionsChanged != null)
                OptionsChanged(this, new OptionsChangedEventArgs { ChangedOptions = _committedChanges });
            _committedChanges.Clear();
        }

        public void SaveBool(string name, bool value)
        {
            SaveString(name, value.ToString());
        }

        public void SaveInt(string name, int value)
        {
            SaveString(name, value.ToString());
        }

        public void SaveString(string name, string value)
        {
            SaveString(name, value, _category);
        }

        private void SaveString(string name, string value, string cat)
        {
            using (var regKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(_baseKey))
            {
                if (regKey == null) return;
                using (var optionsKey = regKey.CreateSubKey(_optionsKey))
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

        public void SaveEnum<T>(string name, T value) where T : struct
        {
            SaveString(name, value.ToString());
        }

        public void SaveDateTime(string name, DateTime value)
        {
            SaveString(name, value.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region Load methods

        private void AddOrUpdateProperty(string name, object value)
        {
            Func<object, object> loadTransform;
            if (LoadTransforms.TryGetValue(name, out loadTransform))
                value = loadTransform(value);
            _properties[name] = value;
        }

        public void LoadInt(string name, int defaultValue = 0)
        {
            string res = LoadStringInternal(name, _category);
            if (res != null)
            {
                int val;
                if (int.TryParse(res, out val))
                {
                    AddOrUpdateProperty(name, val);
                    return;
                }
            }
            AddOrUpdateProperty(name, defaultValue);
        }

        public void LoadBool(string name, bool defaultValue = false)
        {
            string res = LoadStringInternal(name, _category);
            if (res != null)
            {
                bool val;
                if (bool.TryParse(res, out val))
                {
                    AddOrUpdateProperty(name, val);
                    return;
                }
            }
            AddOrUpdateProperty(name, defaultValue);
        }

        public void LoadString(string name, string defaultValue = null)
        {
            var str = LoadStringInternal(name, _category);
            if (!string.IsNullOrEmpty(str))
                AddOrUpdateProperty(name, str);
            else
                AddOrUpdateProperty(name, defaultValue);
        }

        private string LoadStringInternal(string name, string cat)
        {
            using (var regKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).CreateSubKey(_baseKey))
            {
                if (regKey == null) return null;
                using (var optionsKey = regKey.CreateSubKey(_optionsKey))
                {
                    if (optionsKey == null) return null;
                    using (var categoryKey = optionsKey.CreateSubKey(cat))
                    {
                        return categoryKey?.GetValue(name) as string;
                    }
                }
            }
        }

        public void LoadEnum<T>(string name, T defaultValue = default(T)) where T : struct
        {
            string res = LoadStringInternal(name, _category);
            if (res != null)
            {
                T enumRes;
                if (Enum.TryParse<T>(res, out enumRes))
                {
                    AddOrUpdateProperty(name, enumRes);
                    return;
                }
            }
            AddOrUpdateProperty(name, defaultValue);
        }

        public void LoadDateTime(string name, DateTime defaultValue = default(DateTime))
        {
            string res = LoadStringInternal(name, _category);
            if (res != null)
            {
                DateTime dateRes;
                if (DateTime.TryParse(res, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateRes))
                {
                    AddOrUpdateProperty(name, dateRes);
                    return;
                }
            }
            AddOrUpdateProperty(name, defaultValue);
        }

        #endregion
    }
}
