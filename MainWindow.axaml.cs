using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace DWallSelector
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> _dWallList = new ObservableCollection<string>();
        private List<string> _connectedDWalls;
        private const string _fileName = "dwalls.json";
        public MainWindow()
        {
            InitializeComponent();
            LoadIPAddresses();
            DWallsList.ItemsSource = _dWallList;
            _connectedDWalls = new List<string>();
        }

        private void LoadIPAddresses()
        {
            try
            {
                if(File.Exists(_fileName))
                {
                    var json = File.ReadAllText(_fileName);
                    var list = JsonSerializer.Deserialize<ObservableCollection<string>>(json);
                    if(list != null)
                    {
                        _dWallList = list;
                    }
                } else
                {
                    File.WriteAllText(_fileName, "[]");
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"Error saving IP addresses: {ex.Message}");
            }
        }

        private void SaveIPAddresses()
        {
            try
            {
                var json = JsonSerializer.Serialize(_dWallList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_fileName, json);
            } catch(Exception ex)
            {
                Console.WriteLine($"Error saving IP addresses: {ex.Message}");
            }
        }

        public void OnConnectBtnClick(object sender, RoutedEventArgs e)
        {
            foreach(var dWall in DWallsList.SelectedItems)
            {
                if (dWall is string selectedIP)
                {
                    if (_connectedDWalls.Contains(selectedIP)) continue;
                    Console.WriteLine($"Connecting to {selectedIP}...");

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c telnet {selectedIP}",
                            UseShellExecute = true
                        },
                        EnableRaisingEvents = true
                    };

                    process.Exited += (s, args) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            _connectedDWalls.Remove(selectedIP);
                            if (_connectedDWalls.Count == 0)
                            {
                                ConnectBtn.IsEnabled = true;
                                DWallsList.IsEnabled = true;
                                Console.WriteLine("Telnet window closed.");
                            }
                        });
                    };

                    process.Start();
                    _connectedDWalls.Add(selectedIP);
                }
                else
                {
                    Console.WriteLine("No IP selected");
                }
            }
        }

        public void OnAddNewIPAddress(object sender, RoutedEventArgs e)
        {
            if(IPAddress.TryParse(NewIPAddressInput.Text, out _))
            {
                if(!_dWallList.Contains(NewIPAddressInput.Text))
                {
                    _dWallList.Add(NewIPAddressInput.Text);
                    NewIPAddressInput.Text = string.Empty;
                    SaveIPAddresses();
                }
            } 
            else
            {
                Console.WriteLine("Invalid IP Address");
            }
        }

        public void OnDWallIPDelete(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.Tag is string dWallToDelete) 
            {
                _dWallList.Remove(dWallToDelete);
                SaveIPAddresses();
            }
        }
    }
}