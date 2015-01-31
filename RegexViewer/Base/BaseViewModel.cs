using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RegexViewer
{
    public abstract class BaseViewModel<T> : Base, INotifyPropertyChanged, IViewModel<T>
    {
        #region Private Fields

        private Command _closeAllCommand;

        // private ITabViewModel<T> activeTab;
        private Command _closeCommand;

        private Command _newCommand;
        private Command _openCommand;
        private bool _openDialogVisible;
        private Command _saveAsCommand;
        private Command _saveCommand;
        private int _selectedIndex;
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
            get { return _closeAllCommand ?? new Command(CloseAllFiles); }
            set { _closeAllCommand = value; }
        }

        public Command CloseCommand
        {
            get { return _closeCommand ?? new Command(CloseFile); }
            set { _closeCommand = value; }
        }

        public Command DragDropCommand
        {
            get { return _openCommand ?? new Command(OpenDrop); }
            set { _openCommand = value; }
        }

        public Command NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new Command(NewFile);
                }
                _newCommand.CanExecute = true;

                return _newCommand;
            }
            set { _newCommand = value; }
        }

        public Command OpenCommand
        {
            get { return _openCommand ?? new Command(OpenFile); }
            set { _openCommand = value; }
        }

        public bool OpenDialogVisible
        {
            get
            {
                return _openDialogVisible;
            }

            set
            {
                if (_openDialogVisible != value)
                {
                    _openDialogVisible = value;
                    OnPropertyChanged("OpenDialogVisible");
                }
            }
        }

        public Command SaveAsCommand
        {
            get { return _saveAsCommand ?? new Command(SaveFileAs); }
            set { _saveAsCommand = value; }
        }

        public Command SaveCommand
        {
            get { return _saveCommand ?? new Command(SaveFile); }
            set { _saveCommand = value; }
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
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
                //OnPropertyChanged("TabItems");
            }
        }

        public IFileManager<T> ViewManager { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void AddTabItem(ITabViewModel<T> tabItem)
        {
            if (!tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Add(tabItem);
                this.SelectedIndex = tabItems.Count - 1;
            }
        }
      //  public object ViewObject { get; set; }
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
            ITabViewModel<T> tabItem = tabItems[_selectedIndex];
            if (!this.ViewManager.CloseFile(tabItem.Tag))
            {
                return;
            }

            RemoveTabItem(tabItem);
        }

        public abstract void NewFile(object sender);

        public void OpenDrop(object sender)
        {
            SetStatus("OpenDrop: " + sender.GetType().ToString());
            SetStatus("OpenDrop: " + sender.ToString());
            if (sender is string)
            {
                SetStatus("OpenDrop: " + (sender as string));
            }
        }

        public abstract void OpenFile(object sender);

        public void RemoveTabItem(ITabViewModel<T> tabItem)
        {
            if (tabItems.Any(x => String.Compare((string)x.Tag, (string)tabItem.Tag, true) == 0))
            {
                tabItems.Remove(tabItem);
                this.SelectedIndex = tabItems.Count - 1;
            }
        }

        public abstract void RenameTabItem(string newName);

        public abstract void SaveFile(object sender);

        public abstract void SaveFileAs(object sender);

        #endregion Public Methods
    }
}