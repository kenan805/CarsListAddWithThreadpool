using CarsListAddWithThreadpool.Command;
using CarsListAddWithThreadpool.Models;
using CarsListAddWithThreadpool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CarsListAddWithThreadpool.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<Car> Cars { get; set; } = new ObservableCollection<Car>();
        private CancellationTokenSource _tokenSource;

        private MainWindow _mainWindow;

        public RelayCommand StartCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }
        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            _mainWindow.tglbtnMode.TouchEnter += TglbtnMode_TouchEnter;

            StartCommand = new RelayCommand(act => BtnStart_Click());
            CancelCommand = new RelayCommand(act => BtnCancel_Click());
            ClearCommand = new RelayCommand(act => BtnClearList_Click());
        }

        private void BtnStart_Click()
        {
            if (Cars.Count > 0) Cars.Clear();
            _mainWindow.textTime.Text = "00:00:00.000";

            var timer = new Stopwatch();

            if (_mainWindow.tglbtnMode.IsChecked == false)
            {
                _tokenSource = new CancellationTokenSource();
                ThreadPool.QueueUserWorkItem(delegate
                {
                    timer.Start();
                    DirectoryInfo info = new DirectoryInfo(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json");
                    foreach (var item in info.GetFiles())
                    {
                        var fileDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json\";
                        fileDirectory += item.Name;

                        var json = File.ReadAllText(fileDirectory);
                        var tempCars = JsonSerializer.Deserialize<ObservableCollection<Car>>(json);
                        for (int i = 0; i < tempCars.Count; i++)
                        {
                            _mainWindow.Dispatcher.Invoke(new Action(() => Cars.Add(tempCars[i])));
                            Thread.Sleep(500);

                            if (_tokenSource.Token.IsCancellationRequested) break;
                        }
                        if (_tokenSource.Token.IsCancellationRequested) break;

                    }

                    timer.Stop();
                    if (!_tokenSource.Token.IsCancellationRequested)
                        _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.textTime.Text = timer.Elapsed.ToString(@"hh\:mm\:ss\.fff")));

                });

            }
            else
            {

                DirectoryInfo info = new DirectoryInfo(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json");
                foreach (var item in info.GetFiles())
                {
                    _tokenSource = new CancellationTokenSource();
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        timer.Start();
                        var fileDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + @"\Json\";
                        fileDirectory += item.Name;

                        var json = File.ReadAllText(fileDirectory);
                        var tempCars = JsonSerializer.Deserialize<ObservableCollection<Car>>(json);

                        for (int i = 0; i < tempCars.Count; i++)
                        {
                            _mainWindow.Dispatcher.Invoke(new Action(() => Cars.Add(tempCars[i])));
                            Thread.Sleep(500);
                            if (_tokenSource.Token.IsCancellationRequested) break;
                        }

                        timer.Stop();
                        if (!_tokenSource.Token.IsCancellationRequested)
                            _mainWindow.Dispatcher.Invoke(new Action(() => _mainWindow.textTime.Text = timer.Elapsed.ToString(@"hh\:mm\:ss\.fff")));


                    });
                }
            }
        }

        private void BtnClearList_Click()
        {
            _mainWindow.textTime.Text = "00:00:00.000";
            Cars.Clear();
        }

        private void BtnCancel_Click()
        {
            if (_tokenSource != null)
                _tokenSource.Cancel();
        }

        private void TglbtnMode_TouchEnter(object sender, TouchEventArgs e)
        {
            if (Cars.Count > 0) Cars.Clear();
        }
    }
}
