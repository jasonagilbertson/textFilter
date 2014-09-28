using System.ComponentModel;
using System.Diagnostics;

namespace RegexViewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private FilterViewModel filterViewModel;
        private LogViewModel logViewModel;
        private RegexViewerSettings settings;
        private TraceSource ts = new TraceSource("RegexViewer:MainViewModel");

        #endregion Private Fields

        #region Public Constructors

        public MainViewModel()
        {
            // LogCollection = _LogManager.GetLogs();
            // FilterCollection = _FilterManager.GetFilters();
            //Settings = new RegexViewerSettings();
            //   BackgroundColor = Color.Black;
            //_Settings.BackgroundColor = Color.Black;
            //_Settings.FontColor = Color.White;
            settings = RegexViewerSettings.Settings;

            logViewModel = new LogViewModel();
            filterViewModel = new FilterViewModel();
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public FilterViewModel FilterViewModel
        {
            get { return filterViewModel; }
            set { filterViewModel = value; }
        }

        public LogViewModel LogViewModel
        {
            get { return logViewModel; }
            set { logViewModel = value; }
        }

        public RegexViewerSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.Save();
        }

        #endregion Internal Methods
    }
}