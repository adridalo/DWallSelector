using System;
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
        private const string _fileName = "dwalls.json";
        public MainWindow()
        {
            InitializeComponent();
            LoadIPAddresses();
            DWallsList.ItemsSource = _dWallList;
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
            if(DWallsList.SelectedItem is string selectedIP)
            {
                Console.WriteLine($"Connecting to {selectedIP}...");
                ConnectBtn.IsEnabled = false;
                DWallsList.IsEnabled = false;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k telnet {selectedIP}",
                        UseShellExecute = true
                    },
                    EnableRaisingEvents = true
                };

                process.Exited += (s, args) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        ConnectBtn.IsEnabled = true;
                        DWallsList.IsEnabled = true;
                        Console.WriteLine("Telnet window closed.");
                    });
                };

                process.Start();
            } 
            else
            {
                Console.WriteLine("No IP selected");
            }
        }

        public void OnAddNewIPAddress(object sender, RoutedEventArgs e)
        {
            if(IPAddress.TryParse(NewIPAddressInput.Text, out _))
            {
                if(!_dWallList.Contains(NewIPAddressInput.Text))
                {
                    _dWallList.Add(NewIPAddressInput.Text);
                    SaveIPAddresses();
                }
            } 
            else
            {
                Console.WriteLine("Invalid IP Address");
            }
        }
    }
}