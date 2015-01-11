using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace RegexViewer
{
    public abstract class BaseViewModel<T> : Base, INotifyPropertyChanged, IViewModel<T>
    {
        #region Private Fields

        // private ITabViewModel<T> activeTab;
        private Command closeCommand;
        private Command closeAllCommand;
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

        public Command CloseAllCommand
        {
            get { return closeAllCommand ?? new Command(CloseAllFiles); }
            set { closeAllCommand = value; }
        }
        public Command CloseCommand
        {
            get { return closeCommand ?? new Command(CloseFile); }
            set { closeCommand = value; }
        }

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

        public Command DragDropCommand
        {
            get { return openCommand ?? new Command(OpenDrop); }
            set { openCommand = value; }
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
                // OnPropertyChanged("TabItems");
            }
        }

        public IFileManager<T> ViewManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        public abstract void AddTabItem(IFile<T> fileProperties);

        public void CloseAllFiles(object sender)
        {
            ObservableCollection<ITabViewModel<T>> items = new ObservableCollection<ITabViewModel<T>>(tabItems);
            foreach (ITabViewModel<T> tabItem in items)
            {
                if (!this.ViewManager.CloseFile(tabItem.Tag))
                {
                    continue;
                }

                RemoveTabItem(tabItem);
            }
        }
        public void CloseFile(object sender)
        {
            ITabViewModel<T> tabItem = tabItems[selectedIndex];
            if (!this.ViewManager.CloseFile(tabItem.Tag))
            {
                return;
            }

            RemoveTabItem(tabItem);
        }

        public abstract void NewFile(object sender);

        public abstract void OpenFile(object sender);

        public void OpenDrop(object sender)
        {
            SetStatus("OpenDrop: " + sender.GetType().ToString());
            SetStatus("OpenDrop: " + sender.ToString());
            if(sender is string)
            {
            SetStatus("OpenDrop: " + (sender as string));
            }

        }
        public void RemoveTabItem(ITabViewModel<T> tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
            }
        }

        public abstract void SaveFile(object sender);

        #endregion Public Methods
    }
}