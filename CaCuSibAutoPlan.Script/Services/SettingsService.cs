using System;
using System.IO;
using System.Xml.Serialization;
using CaCuSibAutoPlan.Models;

namespace CaCuSibAutoPlan.Services
{
    public class SettingsService
    {
        public string SettingsPath { get; private set; }
        public SettingsService()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CaCuSibAutoPlan");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            SettingsPath = Path.Combine(folder, "last-settings.xml");
        }
        public AutoPlanSettings Load()
        {
            if (!File.Exists(SettingsPath)) return new AutoPlanSettings();
            try
            {
                var serializer = new XmlSerializer(typeof(AutoPlanSettings));
                using (var reader = new StreamReader(SettingsPath))
                {
                    var settings = serializer.Deserialize(reader) as AutoPlanSettings;
                    return settings ?? new AutoPlanSettings();
                }
            }
            catch { return new AutoPlanSettings(); }
        }
        public void Save(AutoPlanSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            settings.SavedAt = DateTime.Now;
            var serializer = new XmlSerializer(typeof(AutoPlanSettings));
            using (var writer = new StreamWriter(SettingsPath)) serializer.Serialize(writer, settings);
        }
    }
}
