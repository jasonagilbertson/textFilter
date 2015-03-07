using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RegexViewer
{
    public abstract class BaseViewModel<T> : Base, INotifyPropertyChanged, IViewModel<T>
    {
        #region Public Fields

        public string _tempTabNameFormat = "*new {0}*";
        public string _tempTabNameFormatPattern = @"\*new [0-9]{1,2}\*";
        #endregion Public Fields

        #region Private Fields
        private Command _closeAllCommand;
        private Command _closeCommand;
        private Command _newCommand;
        private Command _openCommand;
        private Command _gotFocusCommand;
        private bool _openDialogVisible;
        private int _previousIndex = -1;
        private Command _saveAsCommand;
        private Command _saveCommand;
        private int _selectedIndex = -1;
        private RegexViewerSettings settings = RegexViewerSettings.Settings;
        private ObservableCollection<ITabViewModel<T>> tabItems;

        #endregion Private Fields

        #region Public Constructors

        public BaseViewModel()
        {
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

        public Command GotFocusCommand
        {
            get
            {
                if (_gotFocusCommand == null)
                {
                    _gotFocusCommand = new Command(GotFocusExecuted);
                }
                _gotFocusCommand.CanExecute = true;

                return _gotFocusCommand;
            }
            set { _gotFocusCommand = value; }
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

        public int PreviousIndex
        {
            get
            {
                return _previousIndex;
            }
            set
            {
                _previousIndex = value;
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

        public void GotFocusExecuted(object sender)
        {
            
                App.Current.MainWindow.Title = string.Format("{0} {1}", System.AppDomain.CurrentDomain.FriendlyName, CurrentFile().Tag);
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
                    SetStatus(string.Format("BaseViewModel:SelectedIndex changed old index: {0} new index: {1}", _selectedIndex, value));
                    _selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                  //  App.Current.MainWindow.Title = string.Format("{0} {1}", System.AppDomain.CurrentDomain.FriendlyName, CurrentFile().Tag);
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
            if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
            {
                ITabViewModel<T> tabItem = tabItems[_selectedIndex];
                if (!this.ViewManager.CloseFile(tabItem.Tag))
                {
                    return;
                }

                RemoveTabItem(tabItem);
            }
        }

        public IFile<T> CurrentFile()
        {
            if (SelectedIndex >= 0 && SelectedIndex < this.TabItems.Count)
            {
                //SetStatus(string.Format("CurrentFile: SelectedIndex: {0}", SelectedIndex));
                return this.ViewManager.FileManager.FirstOrDefault(x => x.Tag == this.TabItems[SelectedIndex].Tag);
            }

            SetStatus(string.Format("CurrentFile: warning: returning default T SelectedIndex: {0}", SelectedIndex));
            return default(IFile<T>);
        }

        public ITabViewModel<T> CurrentTab()
        {
            if (SelectedIndex >= 0)
            {
                return this.TabItems[SelectedIndex];
            }

            SetStatus(string.Format("CurrentTab: warning: returning default T SelectedTab: {0}", SelectedIndex));
            return default(ITabViewModel<T>);
        }

        public void NewFile(object sender)
        {
            IFile<T> file = default(IFile<T>);
            // add temp name
            for (int i = 0; i < 100; i++)
            {
                string tempTag = string.Format(_tempTabNameFormat, i);
                if (this.TabItems.Any(x => String.Compare((string)x.Tag, tempTag, true) == 0))
                {
                    continue;
                }
                else
                {
                    if (SelectedIndex >= 0 & SelectedIndex < this.TabItems.Count)
                    {
                        file = this.ViewManager.NewFile(tempTag, this.TabItems[SelectedIndex].ContentList);
                    }
                    else
                    {
                        file = this.ViewManager.NewFile(tempTag);
                    }
                    break;
                }
            }

            AddTabItem(file);
        }

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