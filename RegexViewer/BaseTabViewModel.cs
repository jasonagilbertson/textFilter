using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Linq;

namespace RegexViewer
{
    public abstract class BaseTabViewModel<T> : Base, ITabViewModel<T>, INotifyPropertyChanged
    {
        #region Private Fields

        private string background;

        private ObservableCollection<T> contentList = new ObservableCollection<T>();
        private string activeTab;
        private Command copyCommand;
        private Command selectionChangedCommand;
        private string header;
        private string name;
        private Command pasteCommand;
        private string tag;
        private List<T> selectedContent = new List<T>();

        #endregion Private Fields

        

        #region Public Constructors

        public BaseTabViewModel()
        {
            // MainModel = mainModel;
        }

        #endregion Public Constructors
        #region Public Events

    
        #endregion Public Methods

        #region Public Properties

        public string Background
        {
            get
            {
                return background;
            }

            set
            {
                if (background != value)
                {
                    background = value;
                    OnPropertyChanged("Background");
                }
            }
        }

        private bool modified;
        public bool Modified
        {
            get
            {
                return modified;
            }

            set
            {
                if (modified != value)
                {
                    modified = value;
                    OnPropertyChanged("Modified");
                }
            }
        }
        public string ActiveTab
        {
            get
            {
                return activeTab;
            }

            set
            {
                if (activeTab != value)
                {
                    activeTab = value;
                    OnPropertyChanged("ActiveTab");
                    //OnTabChanged("ActiveTab");
                }
            }
        }

        //  public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<T> ContentList
        {
            get { return contentList; }
            set
            {
                contentList = value;
                //   OnPropertyChanged("ContentList");
                //   Modified = true;
            }
        }

        public List<T> SelectedContent
        {
            get { return selectedContent; }
            set
            {
                selectedContent = value;
            }
        }

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

        public Command SelectionChangedCommand
        {
            get
            {
                if (selectionChangedCommand == null)
                {
                    selectionChangedCommand = new Command(SelectionChangedExecuted);
                }
                selectionChangedCommand.CanExecute = true;

                return selectionChangedCommand;
            }
            set { copyCommand = value; }
        }
        public string Header
        {
            get
            {
                return header;
            }

            set
            {
                if (header != value)
                {
                    header = value;
                    OnPropertyChanged("Header");
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Command PasteCommand
        {
            get
            {
                if (pasteCommand == null)
                {
                    pasteCommand = new Command(PasteText);
                }
                pasteCommand.CanExecute = true;

                return pasteCommand;
            }
            set { pasteCommand = value; }
        }

        public string Tag
        {
            get
            {
                return tag;
            }

            set
            {
                if (tag != value)
                {
                    tag = value;
                    OnPropertyChanged("Tag");
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public abstract void CopyExecuted(object sender);
        public void SelectionChangedExecuted(object sender)
        {
            SetStatus("SelectionChangeExecuted:enter");
            if (sender is System.Collections.IList)
            {
                //List<T> items = (sender as IList).Cast<T>().ToList();
                selectedContent = (sender as IList).Cast<T>().ToList();
                //var collection = items;
              //  selectedContent = (sender as List<T>);
            }
        }

        public void PasteText()
        {
        }

        #endregion Public Methods
    }
}