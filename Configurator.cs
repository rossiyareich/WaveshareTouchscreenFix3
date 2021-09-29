using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace WaveshareTouchscreenFix3
{
    public class Configurator
    {
        public bool LoadFile()
        {
            string fileName = "config.json";
            string jsonString;
            try
            {
                jsonString = File.ReadAllText(fileName);
            }
            catch(FileNotFoundException)
            {
                CreateEmptyConfiguration();
                MessageBox.Show("Configuration file was not found, check the created config.json configuration file in the executable's folder and edit in your configuration. Application will close once you press OK or close the dialouge box.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid configuration, Exception Messege: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Configuration configuration = JsonSerializer.Deserialize<Configuration>(jsonString);
            Form1.CurrentConfiguration = configuration;
            return true;
        }
        public static void CreateEmptyConfiguration()
        {
            Configuration emptyConfiguration = new Configuration()
            {
                MapDisplay = false,
                DisplaySize = new RectSize()
                {
                    X = 0,
                    Y = 0
                },
                DigitizerSize = new RectSize()
                {
                    X = 0,
                    Y = 0
                },
                DeviceName = "",
                HoldMs = 1000
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string emptyConfigJson = JsonSerializer.Serialize(emptyConfiguration, options);
            File.WriteAllText("config.json", emptyConfigJson);
        }
    }
}
