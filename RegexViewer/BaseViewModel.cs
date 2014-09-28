using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace RegexViewer
{
    public abstract class BaseViewModel<T> : INotifyPropertyChanged, IViewModel<T>
    {
        #region Private Fields

        private Command closeCommand;
        public IFileManager<T> FileManager { get; set; }

        private Command openCommand;
        private int selectedIndex;
        private RegexViewerSettings settings = RegexViewerSettings.Settings;

        private ObservableCollection<ITabViewModel> tabItems;

        private TraceSource ts = new TraceSource("RegexViewer:BaseViewModel");

        #endregion Private Fields

        #region Public Constructors

        public BaseViewModel()
        { }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public Command CloseCommand
        {
            get { return closeCommand ?? new Command(CloseFile); }
            set { closeCommand = value; }
        }

        public Command OpenCommand
        {
            get { return openCommand ?? new Command(OpenFile); }
            set { openCommand = value; }
        }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        public RegexViewerSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public ObservableCollection<ITabViewModel> TabItems
        {
            get
            {
                return this.tabItems;
            }
            set
            {
                tabItems = value;
                OnPropertyChanged("TabItems");
            }
        }

        #endregion Public Properties

        #region Public Methods

        public abstract void AddTabItem(IFileProperties<T> fileProperties);

        //public bool CloseLog(TabItem tabItem)
        public void CloseFile(object sender)
        {
            ITabViewModel tabItem = tabItems[selectedIndex];
            if (!this.FileManager.CloseLog(tabItem.Tag))
            {
                return;
            }

            RemoveTabItem(tabItem);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public abstract void OpenFile(object sender);

        public void RemoveTabItem(ITabViewModel tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
            }
        }

        #endregion Public Methods
    }
}