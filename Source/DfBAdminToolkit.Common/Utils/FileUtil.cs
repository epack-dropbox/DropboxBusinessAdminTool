﻿using System.Configuration;
using System.Linq;
using System.Reflection;

namespace DfBAdminToolkit.Common.Utils {

    public static class FileUtil {

        public static string FormatFileSize(long size) {
            int[] limits = new int[] { 1024 * 1024 * 1024, 1024 * 1024, 1024 };
            string[] units = new string[] { "GB", "MB", "KB" };

            for (int i = 0; i < limits.Length; i++) {
                if (size >= limits[i]) {
                    return string.Format("{0:#,##0.##} " + units[i], ((double)size / limits[i]));
                }
            }

            return string.Format("{0} bytes", size);
        }

        public static decimal FormatFileSizeMB(decimal size)
        {
            //returns in MB to 2 decimal places
            decimal limits = 1048576;
            decimal newsize = 0;

            newsize =  (size / limits);
            newsize = System.Math.Round(newsize, 2);

            return newsize;
        }

        public static void EncryptAppSettings(string section) {
            Configuration objConfig = ConfigurationManager.OpenExeConfiguration(GetAppPath() + "DfBAdminToolkit.exe");
            AppSettingsSection objAppsettings = (AppSettingsSection)objConfig.GetSection(section);
            if (!objAppsettings.SectionInformation.IsProtected) {
                objAppsettings.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                objAppsettings.SectionInformation.ForceSave = true;
                objConfig.Save(ConfigurationSaveMode.Modified);
            }
        }

        public static string GetAppPath() {
            System.Reflection.Module[] modules = System.Reflection.Assembly.GetExecutingAssembly().GetModules();
            string location = System.IO.Path.GetDirectoryName(modules[0].FullyQualifiedName);
            if ((location != "") && (location[location.Length - 1] != '\\'))
                location += '\\';
            return location;
        }

        public static void UpdateKey(string keyName, string newValue) {
            Configuration config = ConfigurationManager.OpenExeConfiguration(GetAppPath() + "DfBAdminToolkit.exe");
            config.AppSettings.Settings[keyName].Value = newValue;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public static bool TokenCheck() {
            bool result = true;
            string newValue = "";
            Configuration config = ConfigurationManager.OpenExeConfiguration(GetAppPath() + "DfBAdminToolkit.exe");
            newValue = config.AppSettings.Settings["DefaultAccessToken"].Value;
            if (newValue.StartsWith("ENTER")) {
                result = false;
            }
            return result;
        }

        public static void ResetConfigMechanism() {
            typeof(ConfigurationManager)
                .GetField("s_initState", BindingFlags.NonPublic |
                                         BindingFlags.Static)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", BindingFlags.NonPublic |
                                            BindingFlags.Static)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName ==
                            "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", BindingFlags.NonPublic |
                                       BindingFlags.Static)
                .SetValue(null, null);
        }
    }
}