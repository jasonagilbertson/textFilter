using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RegexViewer
{
    public class MainViewModel : Base, IMainViewModel
    {
        #region Private Fields

        private Command copyCommand;
        private FilterViewModel filterViewModel;
        private LogViewModel logViewModel;
        private RegexViewerSettings settings;
        private ObservableCollection<string> status = new ObservableCollection<string>();

        #endregion Private Fields

        #region Public Constructors

        public MainViewModel()
        {
            //SetStatusHandler = SetStatus;
            Base.MainModel = this;
            settings = RegexViewerSettings.Settings;

            logViewModel = new LogViewModel();
            filterViewModel = new FilterViewModel();
        }

        #endregion Public Constructors

        #region Public Properties

        public Command CopyCommand
        {
            get
            {
                if (copyCommand == null)
                {
                    copyCommand = new Command(CopyExecuted);
                }
                copyCommand.CanExecute = true;

                return copyCommand;
            }
            set { copyCommand = value; }
        }

        public FilterViewModel FilterViewModel
        {
            get { return filterViewModel; }
            set { filterViewModel = value; }
        }

        //   public event PropertyChangedEventHandler PropertyChanged;
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

        public ObservableCollection<string> Status
        {
            get
            {
                return status;
            }
            //private set
            //{
            //        status = value;
            //        OnPropertyChanged("Status");
            //}
        }

        #endregion Public Properties

        #region Public Methods

        public void CopyExecuted(object contentList)
        {
            try
            {
                List<ListBoxItem> ContentList = (List<ListBoxItem>)contentList;

                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (ListBoxItem lbi in ContentList)
                {
                    if (lbi != null && lbi.IsSelected)
                    //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
                    {
                        htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
                    }
                }

                htmlFragment.CopyListToClipboard();
            }
            catch (Exception ex)
            {
                MainModel.SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        public void SetStatus(string statusData)
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                while (this.status.Count > 1000)
                {
                    this.status.RemoveAt(0);
                }

                this.status.Add(statusData);
                OnPropertyChanged("Status");
            }));
        }

        #endregion Public Methods

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

        #region Internal Methods

        internal void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.Save();
        }

        #endregion Internal Methods
    }
}