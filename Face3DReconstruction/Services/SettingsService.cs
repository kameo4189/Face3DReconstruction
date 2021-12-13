using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Configuration;
using System.Collections.Specialized;

namespace Face3DReconstruction.Services
{
    /// <summary>
    /// A simple <see langword="class"/> that handles the local app settings.
    /// </summary>
    public sealed class SettingsService : ISettingsService
    {
        /// <summary>
        /// The <see cref="IPropertySet"/> with the settings targeted by the current instance.
        /// </summary>
        private readonly Configuration SettingsStorage = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        /// <inheritdoc/>
        public void SetValue(string key, string value)
        {
            var settings = SettingsStorage.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
            SettingsStorage.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(SettingsStorage.AppSettings.SectionInformation.Name);
        }

        /// <inheritdoc/>
        public string GetValue(string key)
        {
            var settings = SettingsStorage.AppSettings.Settings;
            if (settings[key] != null)
            {
                return settings[key].Value;
            }
            return default;
        }
    }
}
