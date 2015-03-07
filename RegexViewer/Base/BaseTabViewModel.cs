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
        private Command _copyCommand;
        private string _header;
        private bool _modified;
        private string _name;
        private Command _pasteCommand;
        private List<T> _selectedContent = new List<T>();
        private int _selectedIndex;
        private Command _selectionChangedCommand;
        private Command _setViewerCommand;
        private string _tag;

        private object _viewer;

        #endregion Private Fields

        #region Public Constructors

        public BaseTabViewModel()
        {
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

        public void CopyExecuted(object contentList)
        {
            try
            {
                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (IFileItem lbi in SelectedContent)
                {
                    htmlFragment.AddClipToList(lbi.Content, lbi.Background, lbi.Foreground);
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
          //  SetStatus("SelectionChangeExecuted:enter");
            if (sender is System.Collections.IList)
            {
                _selectedContent = (sender as IList).Cast<T>().ToList();
            }
            else if (sender is ListBox)
            {
                _selectedContent = (sender as ListBox).SelectedItems.Cast<T>().ToList();
            }
            else if (sender is DataGrid)
            {
          //      SetStatus("SelectionChangeExecuted:datagrid");
                _selectedContent = (sender as DataGrid).SelectedItems.Cast<T>().ToList();
            }
        }

        public void SetViewerExecuted(object sender)
        {
            // cant setstatus here due to recursion
            // cant set when only null

            if (_viewer != sender)
            {
                SetStatus(string.Format("viewer set: {0}", sender.GetType()));
                _viewer = sender;
            }
        }

        #endregion Public Methods
    }
}