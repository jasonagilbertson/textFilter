using System.Collections.Generic;
using System.ComponentModel;

namespace RegexViewer
{
    public abstract class BaseTabViewModel<T> : INotifyPropertyChanged, ITabViewModel<T>
    {
        #region Private Fields

        private string background;

        private List<T> contentList = new List<T>();

        private Command copyCommand;

        private string header;

        private string name;

        private string tag;

        #endregion Private Fields

        #region Public Constructors

        public BaseTabViewModel()
        {
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

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

        public List<T> ContentList
        {
            get { return contentList; }
            set
            {
                contentList = value;
                OnPropertyChanged("ContentList");
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

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion Public Methods
    }
}