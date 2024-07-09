using FileLoaderApp.Models;
using FileLoaderApp.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace TradeDataLoader
{
    public partial class MainWindow : Window
    {
        private TradeDataLoader _dataLoader;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                txtDirectoryPath.Text = selectedPath;

                List<string> txtFiles = GetTxtFiles(selectedPath);

                listBoxTxtFiles.Items.Clear(); 

                foreach (var file in txtFiles)
                {
                    listBoxTxtFiles.Items.Add(file); 
                }
            }
        }

        private List<string> GetTxtFiles(string directoryPath)
        {
            List<string> txtFiles = new List<string>();

            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*.txt");

                foreach (var file in files)
                {
                    txtFiles.Add(Path.GetFileName(file)); 
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Xeta: {ex.Message}");
            }

            return txtFiles;
        }

        private void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = txtDirectoryPath.Text.Trim();

            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
            {
                _dataLoader = new TradeDataLoader(directoryPath, TimeSpan.FromSeconds(5), GetLoaders());
                _dataLoader.DataLoaded += DataLoader_DataLoaded; 
                _dataLoader.Start();
            }
            else
            {
                System.Windows.MessageBox.Show("Uygun bir fayl secin.");
            }
        }

        private List<ITradeLoader> GetLoaders()
        {
            return new List<ITradeLoader>
            {
                new CsvTradeLoader(),
                new TxtTradeLoader()
            };
        }

        private async void DataLoader_DataLoaded(object sender, List<TradeDataItem> e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                dgTradeData.ItemsSource = e;
            });
        }

        public class TradeDataLoader
        {
            private string _directoryPath;
            private TimeSpan _monitoringInterval;
            private List<ITradeLoader> _loaders;

            public event EventHandler<List<TradeDataItem>> DataLoaded;

            public TradeDataLoader(string directoryPath, TimeSpan monitoringInterval, List<ITradeLoader> loaders)
            {
                _directoryPath = directoryPath;
                _monitoringInterval = monitoringInterval;
                _loaders = loaders;
            }

            public void Start()
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        string[] files = Directory.GetFiles(_directoryPath);

                        foreach (string file in files)
                        {
                            ProcessFile(file);
                        }

                        await Task.Delay(_monitoringInterval);
                    }
                });
            }

            private void ProcessFile(string filePath)
            {
                foreach (var loader in _loaders)
                {
                    try
                    {
                        List<TradeDataItem> tradeData = loader.LoadData(filePath);
                        OnDataLoaded(tradeData);
                        break; 
                    }
                    catch (Exception ex)
                    {
  
                        Console.WriteLine($"Xeta: {ex.Message}");
                    }
                }
            }

            protected virtual void OnDataLoaded(List<TradeDataItem> tradeData)
            {
                DataLoaded?.Invoke(this, tradeData);
            }
        }
    }

   
    
}
