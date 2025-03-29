using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using WindowsCalculator.ViewModels;

namespace WindowsCalculator.Models
{
    public class CalculatorSettings
    {
        public bool UseDigitGrouping { get; set; } = true;
        public bool IsStandardMode { get; set; } = true;
        public CalculatorViewModel.NumberBase CurrentBase { get; set; } = CalculatorViewModel.NumberBase.DEC;

        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsCalculator",
            "settings.json");

        public static CalculatorSettings Load()
        {
            try
            {
                // Create directory if it doesn't exist
                string directoryPath = Path.GetDirectoryName(SettingsFilePath);
                Directory.CreateDirectory(directoryPath);

                if (File.Exists(SettingsFilePath))
                {
                    // Read and deserialize the settings file
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<CalculatorSettings>(json) ?? new CalculatorSettings();

                    Debug.WriteLine($"Settings loaded: UseDigitGrouping={settings.UseDigitGrouping}, " +
                                    $"IsStandardMode={settings.IsStandardMode}, CurrentBase={settings.CurrentBase}");

                    return settings;
                }

                Debug.WriteLine("Settings file not found. Using defaults.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new CalculatorSettings();
        }

        public bool Save()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(SettingsFilePath);
                Directory.CreateDirectory(directoryPath);

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsFilePath, json);

                Debug.WriteLine($"Settings saved: UseDigitGrouping={UseDigitGrouping}, " +
                                $"IsStandardMode={IsStandardMode}, CurrentBase={CurrentBase}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }
    }
}