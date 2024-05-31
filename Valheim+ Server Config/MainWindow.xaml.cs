using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ValheimConfigEditor
{
    public partial class MainWindow : Window
    {
        private string downloadsPath;
        private string configFileName = "valheim_plus.cfg";
        private string configFilePath;
        private ConfigParser configParser;

        public MainWindow()
        {
            InitializeComponent();
            InitializePaths();
            LoadConfiguration();
        }

        private void InitializePaths()
        {
            downloadsPath = KnownFolders.GetPath(KnownFolder.Downloads);
            configFilePath = Path.Combine(downloadsPath, configFileName);
        }

        private void LoadConfiguration()
        {
            configParser = new ConfigParser(configFilePath);
            GenerateUI();
        }

        private void GenerateUI()
        {
            foreach (var section in configParser.ConfigSections)
            {
                var sectionGrid = new Grid
                {
                    Margin = new Thickness(10, 10, 10, 0)
                };
                sectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                sectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                sectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var sectionLabel = new TextBlock
                {
                    Text = section.Key,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 5),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(sectionLabel, 0);
                sectionGrid.Children.Add(sectionLabel);

                var sectionCheckBox = new CheckBox
                {
                    Margin = new Thickness(5, 0, 0, 5),
                    IsChecked = section.Value.ContainsKey("enabled") && section.Value["enabled"].ToLower() == "true",
                    VerticalAlignment = VerticalAlignment.Center
                };
                sectionCheckBox.Checked += (sender, args) => SectionEnabled_Checked(section.Key);
                sectionCheckBox.Unchecked += (sender, args) => SectionEnabled_Unchecked(section.Key);
                Grid.SetColumn(sectionCheckBox, 1);
                sectionGrid.Children.Add(sectionCheckBox);

                MainStackPanel.Children.Add(sectionGrid);

                var sectionStackPanel = new StackPanel
                {
                    Margin = new Thickness(20, 5, 0, 10),
                    Visibility = sectionCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed,
                    Name = section.Key.Replace(" ", "") + "Settings"
                };
                MainStackPanel.Children.Add(sectionStackPanel);

                foreach (var setting in section.Value)
                {
                    if (setting.Key != "enabled")
                    {
                        var settingControl = CreateSettingControl(section.Key, setting.Key, setting.Value);
                        sectionStackPanel.Children.Add(settingControl);
                    }
                }
            }
        }

        private FrameworkElement CreateSettingControl(string sectionKey, string key, string value)
        {
            var grid = new Grid
            {
                Margin = new Thickness(5, 2, 5, 2)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = key,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 0);

            if (value.ToLower() == "true" || value.ToLower() == "false")
            {
                var checkBox = new CheckBox
                {
                    IsChecked = value.ToLower() == "true",
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(checkBox, 1);
                grid.Children.Add(label);
                grid.Children.Add(checkBox);
            }
            else
            {
                var textBox = new TextBox
                {
                    Text = value,
                    Width = 60,  // Adjusted width for text boxes
                    Margin = new Thickness(5, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumn(textBox, 1);
                grid.Children.Add(label);
                grid.Children.Add(textBox);

                /*if (key == "areaRepairRadius" && !IsParentChecked(sectionKey, "enableAreaRepair"))
                {
                    grid.Visibility = Visibility.Collapsed;
                }*/
            }

            return grid;
        }

        private bool IsParentChecked(string sectionKey, string parentKey)
        {
            var sectionStackPanel = (StackPanel)MainStackPanel.Children
                .OfType<StackPanel>()
                .FirstOrDefault(sp => sp.Name == sectionKey.Replace(" ", "") + "Settings");

            if (sectionStackPanel != null)
            {
                var parentCheckBox = sectionStackPanel.Children
                    .OfType<CheckBox>()
                    .FirstOrDefault(cb => cb.Content.ToString() == parentKey);

                return parentCheckBox?.IsChecked ?? false;
            }
            return false;
        }

        private void SectionEnabled_Checked(string sectionKey)
        {
            var sectionStackPanel = (StackPanel)MainStackPanel.Children
                .OfType<StackPanel>()
                .FirstOrDefault(sp => sp.Name == sectionKey.Replace(" ", "") + "Settings");
            if (sectionStackPanel != null)
            {
                sectionStackPanel.Visibility = Visibility.Visible;
            }
        }

        private void SectionEnabled_Unchecked(string sectionKey)
        {
            var sectionStackPanel = (StackPanel)MainStackPanel.Children
                .OfType<StackPanel>()
                .FirstOrDefault(sp => sp.Name == sectionKey.Replace(" ", "") + "Settings");
            if (sectionStackPanel != null)
            {
                sectionStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(configFilePath))
            {
                MessageBox.Show("Configuration file not found!");
                return;
            }

            List<string> newLines = new List<string>();
            foreach (var section in configParser.ConfigSections)
            {
                newLines.Add($"[{section.Key}]");
                var sectionStackPanel = (StackPanel)MainStackPanel.Children
                    .OfType<StackPanel>()
                    .FirstOrDefault(sp => sp.Name == section.Key.Replace(" ", "") + "Settings");
                foreach (var setting in section.Value)
                {
                    if (setting.Key == "enabled")
                    {
                        var sectionCheckBox = (CheckBox)MainStackPanel.Children
                            .OfType<CheckBox>()
                            .FirstOrDefault(cb => cb.Content.ToString() == "Enable " + section.Key);
                        newLines.Add($"enabled = {sectionCheckBox.IsChecked.ToString().ToLower()}");
                    }
                    else
                    {
                        foreach (var control in sectionStackPanel.Children)
                        {
                            if (control is CheckBox checkBox && checkBox.Content.ToString() == setting.Key)
                            {
                                newLines.Add($"{setting.Key} = {checkBox.IsChecked.ToString().ToLower()}");
                            }
                            else if (control is Grid grid)
                            {
                                var textBox = grid.Children.OfType<TextBox>().FirstOrDefault();
                                if (textBox != null)
                                {
                                    newLines.Add($"{setting.Key} = {textBox.Text}");
                                }
                            }
                        }
                    }
                }
                newLines.Add("");
            }
            File.WriteAllLines(configFilePath, newLines);
            MessageBox.Show("Configuration saved successfully!");
        }
    }
}
