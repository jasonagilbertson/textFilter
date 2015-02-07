using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace RegexViewer
{
    public abstract class BaseTabViewModel<T> : Base, ITabViewModel<T>, INotifyPropertyChanged
    {

        #region Private Fields

        private string _background;

        private ObservableCollection<T> _contentList = new ObservableCollection<T>();

        // private string activeTab;
        private Command _copyCommand;

        private string _header;
        private bool _modified;
        private string _name;
        private Command _pasteCommand;
        private List<T> _selectedContent = new List<T>();

        // private Command _selectedIndexChangedCommand;
        private int _selectedIndex;

        private Command _selectionChangedCommand;
        private Command _setViewerCommand;
        private string _tag;
        //private T _selectedIndexItem;
        private object _viewer;

        #endregion Private Fields

        #region Public Constructors

        public BaseTabViewModel()
        {
            // MainModel = mainModel;
        }

        #endregion Public Constructors

        #region Public Properties

        public string Background
        {
            get
            {
                return _background;
            }

            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged("Background");
                }
            }
        }

        public ObservableCollection<T> ContentList
        {
            get { return _contentList; }
            set
            {
                if (_contentList != value)
                {
                    _contentList = value;
                    OnPropertyChanged("ContentList");
                    Modified = true;
                }
            }
        }

        public Command CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new Command(CopyExecuted);
                }
                _copyCommand.CanExecute = true;

                return _copyCommand;
            }
            set { _copyCommand = value; }
        }

        public string Header
        {
            get
            {
                return _header;
            }

            set
            {
                if (_header != value)
                {
                    _header = value;
                    OnPropertyChanged("Header");
                }
            }
        }

        //        return _selectedIndexChangedCommand;
        //    }
        //    set { _selectedIndexChangedCommand = value; }
        //}
        public bool Modified
        {
            get
            {
                return _modified;
            }

            set
            {
                if (_modified != value)
                {
                    _modified = value;
                    OnPropertyChanged("Modified");
                }
            }
        }

        //public Command SelectedIndexChangedCommand
        //{
        //    get
        //    {
        //        if (_selectedIndexChangedCommand == null)
        //        {
        //            _selectedIndexChangedCommand = new Command(SelectedIndexChangedExecuted);
        //        }
        //        _selectedIndexChangedCommand.CanExecute = true;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Command PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                {
                    _pasteCommand = new Command(PasteText);
                }
                _pasteCommand.CanExecute = true;

                return _pasteCommand;
            }
            set { _pasteCommand = value; }
        }

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
        //   public List<T> UnFilteredContentList { get; set; }
        public List<T> SelectedContent
        {
            get { return _selectedContent; }
            set
            {
                _selectedContent = value;
            }
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

        public Command SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new Command(SelectionChangedExecuted);
                }
                _selectionChangedCommand.CanExecute = true;

                return _selectionChangedCommand;
            }
            set { _selectionChangedCommand = value; }
        }

        public Command SetViewerCommand
        {
            get
            {
                if (_setViewerCommand == null)
                {
                    _setViewerCommand = new Command(SetViewerExecuted);
                }
                _setViewerCommand.CanExecute = true;

                return _setViewerCommand;
            }
            set { _setViewerCommand = value; }
        }
        public string Tag
        {
            get
            {
                return _tag;
            }

            set
            {
                if (_tag != value)
                {
                    _tag = value;
                    OnPropertyChanged("Tag");
                }
            }
        }

        //public T SelectedIndexItem
        //{
        //    get
        //    {
        //        return _selectedIndexItem;
        //    }
        //    set
        //    {
        //        // if (_selectedItemIndex != value)
        //        // {
        //        _selectedIndexItem = value;
        //        OnPropertyChanged("SelectedItemIndex");
        //        // }
        //    }
        //}
        public object Viewer
        {
            get
            {
                return _viewer;
            }
            set
            {
                _viewer = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        //public abstract void CopyExecuted(object sender);
        public void CopyExecuted(object contentList)
        {
            try
            {
                // List<LogFileItem> ContentList = (List<LogFileItem>)contentList;

                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (IFileItem lbi in SelectedContent)
                {
                    //if (lbi != null && lbi.IsSelected)
                    // if (lbi != null && lbi.IsFocused)
                    //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
                    //{
                    //    htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
                    htmlFragment.AddClipToList(lbi.Content, lbi.Background, lbi.Foreground);
                    //}
                }

                htmlFragment.CopyListToClipboard();
            }
            catch (Exception ex)
            {
                SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        public void PasteText()
        {
        }
        public void SelectionChangedExecuted(object sender)
        {
            SetStatus("SelectionChangeExecuted:enter");
            if (sender is System.Collections.IList)
            {
                _selectedContent = (sender as IList).Cast<T>().ToList();
            }
            else if (sender is ListBox)
            {
                //   this.SelectedIndexItem = (T)(sender as ListBox).SelectedItem;

                _selectedContent = (sender as ListBox).SelectedItems.Cast<T>().ToList();
            }
            else if (sender is DataGrid)
            {
                SetStatus("SelectionChangeExecuted:datagrid");
                // this.SelectedIndexItem = (T)(sender as DataGrid).SelectedItem;
                _selectedContent = (sender as DataGrid).SelectedItems.Cast<T>().ToList();
            }
        }

        public void SetViewerExecuted(object sender)
        {
         
            if(_viewer == null)
            {
                SetStatus("viewer set");
                _viewer = sender;
            }
        }

        #endregion Public Methods
    }
}