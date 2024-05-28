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
                var sectionLabel = new TextBlock
                {
                    Text = section.Key,
                    FontSize = 16,
                    Margin = new Thickness(10)
                };
                MainStackPanel.Children.Add(sectionLabel);

                var sectionCheckBox = new CheckBox
                {
                    Content = "Enable " + section.Key,
                    Margin = new Thickness(10),
                    IsChecked = section.Value.ContainsKey("enabled") && section.Value["enabled"].ToLower() == "true"
                };
                sectionCheckBox.Checked += (sender, args) => SectionEnabled_Checked(section.Key);
                sectionCheckBox.Unchecked += (sender, args) => SectionEnabled_Unchecked(section.Key);
                MainStackPanel.Children.Add(sectionCheckBox);

                var sectionStackPanel = new StackPanel
                {
                    Margin = new Thickness(20),
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
            if (value.ToLower() == "true" || value.ToLower() == "false")
            {
                var checkBox = new CheckBox
                {
                    Content = key,
                    Margin = new Thickness(10),
                    IsChecked = value.ToLower() == "true"
                };

                // Check for dependencies
                if (key == "enableAreaRepair")
                {
                    checkBox.Checked += (sender, args) => ToggleAreaRepairRadiusVisibility(sectionKey, true);
                    checkBox.Unchecked += (sender, args) => ToggleAreaRepairRadiusVisibility(sectionKey, false);
                }

                return checkBox;
            }
            else if (Regex.IsMatch(value, @"^-?\d+(\.\d+)?$"))
            {
                var grid = new Grid
                {
                    Margin = new Thickness(10)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock
                {
                    Text = key,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);

                var textBox = new TextBox
                {
                    Text = value,
                    Width = 100,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumn(textBox, 1);

                var dottedLine = new TextBlock
                {
                    Text = new string('.', 30),
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 0)
                };
                Grid.SetColumn(dottedLine, 1);

                grid.Children.Add(label);
                grid.Children.Add(dottedLine);
                grid.Children.Add(textBox);

                if (key == "areaRepairRadius" && !IsParentChecked(sectionKey, "enableAreaRepair"))
                {
                    grid.Visibility = Visibility.Collapsed;
                }
                return grid;
            }
            else if (Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
            {
                var grid = new Grid
                {
                    Margin = new Thickness(10)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock
                {
                    Text = key,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);

                var textBox = new TextBox
                {
                    Text = value,
                    Width = 100,
                    Margin = new Thickness(10, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumn(textBox, 1);

                var dottedLine = new TextBlock
                {
                    Text = new string('.', 30),
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 0)
                };
                Grid.SetColumn(dottedLine, 1);

                grid.Children.Add(label);
                grid.Children.Add(dottedLine);
                grid.Children.Add(textBox);

                return grid;
            }
            return new TextBlock
            {
                Text = $"{key} = {value}",
                Margin = new Thickness(10)
            };
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

        private void ToggleAreaRepairRadiusVisibility(string sectionKey, bool isVisible)
        {
            var sectionStackPanel = (StackPanel)MainStackPanel.Children
                .OfType<StackPanel>()
                .FirstOrDefault(sp => sp.Name == sectionKey.Replace(" ", "") + "Settings");

            if (sectionStackPanel != null)
            {
                var panel = sectionStackPanel.Children
                    .OfType<Grid>()
                    .FirstOrDefault(g => g.Name == "areaRepairRadiusPanel");

                if (panel != null)
                {
                    panel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
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
