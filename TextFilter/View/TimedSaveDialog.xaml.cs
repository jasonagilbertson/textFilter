using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TextFilter
{
    public partial class TimedSaveDialog : Window, INotifyPropertyChanged
    {
        #region Private Fields

        private const int _timerSecs = 10;

        private EventHandler _handler;
        private Results _result;
        private ManualResetEvent _timedOut;
        private DispatcherTimer _timer;

        #endregion Private Fields

        #region Public Constructors

        public TimedSaveDialog(string fileName)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            labelDisplay.Content = fileName;
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Enums

        public enum Results
        {
            Unknown,
            Save,
            SaveAs,
            DontSave,
            Disable
        }

        #endregion Public Enums

        #region Public Methods

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

        #endregion Public Methods

        #region Private Methods

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
            _timer.Tick -= _handler;
            _timer.Stop();
            Disable();
            this.Close();
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

        #endregion Private Methods
    }
}