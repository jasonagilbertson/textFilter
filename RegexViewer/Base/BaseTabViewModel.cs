﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
        private Command _sourceUpdatedCommand;
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
        //public abstract bool Group1Visibility { get; set; }
        //public abstract bool Group2Visibility { get; set; }
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

        public Command SourceUpdatedCommand
        {
            get
            {
                if (_sourceUpdatedCommand == null)
                {
                    _sourceUpdatedCommand = new Command(SourceUpdatedExecuted);
                }
                _sourceUpdatedCommand.CanExecute = true;

                return _sourceUpdatedCommand;
            }
            set { _sourceUpdatedCommand = value; }
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
                foreach (IFileItem lbi in this.SelectedContent)
                {
                    // get all cells
                    if (typeof(T) == typeof(LogFileItem))
                    {
                        LogFileItem lFI = (LogFileItem)lbi;
                        StringBuilder sb = new StringBuilder(string.Format("{0},{1}", lFI.Index, lFI.Content));

                        if (!string.IsNullOrEmpty(lFI.Group1))
                        {
                            sb.Append("," + lFI.Group1);
                        }

                        if (!string.IsNullOrEmpty(lFI.Group2))
                        {
                            sb.Append("," + lFI.Group2);
                        }

                        if (!string.IsNullOrEmpty(lFI.Group3))
                        {
                            sb.Append("," + lFI.Group3);
                        }

                        if (!string.IsNullOrEmpty(lFI.Group4))
                        {
                            sb.Append("," + lFI.Group4);
                        }

                        htmlFragment.AddClipToList(sb.ToString(), lbi.Background, lbi.Foreground);
                    }
                    else
                    {

                        htmlFragment.AddClipToList(lbi.Content, lbi.Background, lbi.Foreground);
                    }
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
            if (sender is System.Collections.IList)
            {
                _selectedContent = (sender as IList).Cast<T>().ToList();
            }
            else if (sender is ListBox)
            {
                ListBox listBox = (sender as ListBox);
                _selectedContent = listBox.SelectedItems.Cast<T>().ToList();
       
            }
            else if (sender is DataGrid)
            {
                _selectedContent = (sender as DataGrid).SelectedItems.Cast<T>().ToList();
            }
        }

        public void SetViewerExecuted(object sender)
        {
            // cant setstatus here due to recursion? cant set when only null should be listbox for logfileData

            if (sender is DataGrid && _viewer != sender)
            {
                SetStatus(string.Format("tab viewer set: {0}", sender.GetType()));
                _viewer = sender;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void SourceUpdatedExecuted(object sender)
        {
            SetStatus("SourceUpdatedExecuted: enter");
            if (sender is DataGrid)
            {
                // set keyboard focus on selecteditem
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    int index = dataGrid.SelectedIndex;
                    if (index >= 0)
                    {
                        SetStatus("SourceUpdatedExecuted: container generated, setting keyboard focus to index:" + index);
                        var item = dataGrid.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
                        if (item != null) item.Focus();
                    }
                }
                else
                {
                    SetStatus("SourceUpdatedExecuted: container NOT generated");
                }
            }
        }

        #endregion Private Methods

    }
}