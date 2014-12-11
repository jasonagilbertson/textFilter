using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace RegexViewer
{
    public abstract class BaseViewModel<T> : Base,INotifyPropertyChanged, IViewModel<T>
    {
        #region Private Fields
        // private ITabViewModel<T> activeTab;
        private Command closeCommand;
        private Command newCommand;
        private Command openCommand;
        private bool openDialogVisible;
        private Command saveCommand;
        private int selectedIndex;
        private RegexViewerSettings settings = RegexViewerSettings.Settings;

        private ObservableCollection<ITabViewModel<T>> tabItems;

        #endregion Private Fields

        #region Public Constructors

        public BaseViewModel()
        {
            //   MainModel = mainModel;
            //SetStatusHandler = SetStatus;
            // this.OpenDialogVisible = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public Command CloseCommand
        {
            get { return closeCommand ?? new Command(CloseFile); }
            set { closeCommand = value; }
        }

        //   public event PropertyChangedEventHandler PropertyChanged;
        public IFileManager<T> ViewManager { get; set; }
        //public ITabViewModel<T> ActiveTab
        //{
        //    get
        //    {
        //        return activeTab;
        //    }

        //    set
        //    {
        //        if (activeTab != value)
        //        {
        //            activeTab = value;
        //            OnPropertyChanged("ActiveTab");
        //            //OnTabChanged("ActiveTab");
        //        }
        //    }
        //}

        public Command NewCommand
        {
            get
            {
                if (newCommand == null)
                {
                    newCommand = new Command(NewFile);
                }
                newCommand.CanExecute = true;

                return newCommand;
            }
            set { newCommand = value; }
        }

        public Command OpenCommand
        {
            get { return openCommand ?? new Command(OpenFile); }
            set { openCommand = value; }
        }

        public bool OpenDialogVisible
        {
            get
            {
                return openDialogVisible;
            }

            set
            {
                if (openDialogVisible != value)
                {
                    openDialogVisible = value;
                    OnPropertyChanged("OpenDialogVisible");
                }
            }
        }

        public Command SaveCommand
        {
            get { return saveCommand ?? new Command(SaveFile); }
            set { saveCommand = value; }
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

        public ObservableCollection<ITabViewModel<T>> TabItems
        {
            get
            {
                return this.tabItems;
            }
            set
            {
                tabItems = value;
             //   OnPropertyChanged("TabItems");
            }
        }

        #endregion Public Properties

        #region Public Methods

        public abstract void AddTabItem(IFile<T> fileProperties);

        //public bool CloseLog(TabItem tabItem)
        public void CloseFile(object sender)
        {
            ITabViewModel<T> tabItem = tabItems[selectedIndex];
            if (!this.ViewManager.CloseFile(tabItem.Tag))
            {
                return;
            }

            RemoveTabItem(tabItem);
        }

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

        public abstract void NewFile(object sender);

        public abstract void OpenFile(object sender);

        public void RemoveTabItem(ITabViewModel<T> tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
            }
        }

        public void SaveFile(object sender)
        {
            ITabViewModel<T> tabItem;

            if(sender is TabItem)
            {
                tabItem = (ITabViewModel<T>)(sender as TabItem);
            }
            else
            {
                tabItem = (ITabViewModel<T>)this.TabItems[this.SelectedIndex];
            }
            
            this.ViewManager.SaveFile(tabItem.Tag, tabItem.ContentList);
        }

        #endregion Public Methods
   

    }
}