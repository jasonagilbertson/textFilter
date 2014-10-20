using System.Collections.ObjectModel;

namespace RegexViewer
{
    public abstract class BaseTabViewModel<T> : Base, ITabViewModel<T>
    {
        #region Private Fields

        private string background;

        private ObservableCollection<T> contentList = new ObservableCollection<T>();

        private Command copyCommand;
        private string header;
        private string name;
        private Command pasteCommand;
        private string tag;

        #endregion Private Fields

        //  IMainViewModel MainModel;

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

        //  public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<T> ContentList
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

        public void PasteText()
        {
        }

        #endregion Public Methods
    }
}