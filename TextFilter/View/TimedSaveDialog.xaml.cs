// ************************************************************************************
// Assembly: TextFilter
// File: TimedSaveDialog.xaml.cs
// Created: 9/6/2016
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TextFilter
{
    public partial class TimedSaveDialog : Window, INotifyPropertyChanged
    {
        private const int _timerSecs = 1;
        private int _totalTimerSecs = 5;
        private EventHandler _handler;
        private Results _result;
        private ManualResetEvent _timedOut;
        private DispatcherTimer _timer;

        public TimedSaveDialog(string fileName)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            labelDisplay.Content = fileName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public enum Results
        {
            Unknown,
            Disable,
            DontSave,
            DontSaveAll,
            Save,
            SaveAs
        }

        public void Disable()
        {
            this.Hide();
        }

        public void Enable()
        {
            _timedOut = new ManualResetEvent(false);
            StartTimer();
        }

        public Results WaitForResult()
        {
            this.ShowDialog();
            if (_timedOut != null)
            {
                _timedOut.Reset();
            }

            return _result;
        }

        private void buttonDisable_Click(object sender, RoutedEventArgs e)
        {
            _result = Results.Disable;
            OnTimedEvent(null, null);
        }

        private void buttonDontSave_Click(object sender, RoutedEventArgs e)
        {
            _result = Results.DontSave;
            OnTimedEvent(null, null);
        }

        private void buttonDontSaveAll_Click(object sender, RoutedEventArgs e)
        {
            _result = Results.DontSaveAll;
            OnTimedEvent(null, null);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            _result = Results.Save;
            OnTimedEvent(null, null);
        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            _result = Results.SaveAs;
            OnTimedEvent(null, null);
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            if (_totalTimerSecs-- > 0)
            {
                labelTimerLeft.Content = _totalTimerSecs;
            }
            else
            {
                _timer.Tick -= _handler;
                _timer.Stop();
                Disable();
                this.Close();
            }
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _handler = new EventHandler(OnTimedEvent);
            _result = Results.DontSave;
            _timer.Tick += _handler;
            _timer.Interval = TimeSpan.FromSeconds(_timerSecs);
            _timer.Start();
        }
    }
}