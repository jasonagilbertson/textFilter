using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace RegexViewer
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private string content;
        private List<ListBoxItem> contentList;
        private string header;
        private string name;
        private string tag;
        private string background;

        #endregion Private Fields

        //private int selectedIndex;

        #region Public Constructors

        public ItemViewModel()
        {
            List<ListBoxItem> ContentList = new List<ListBoxItem>();
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public string Content
        {
            get
            {
                return content;
            }

            set
            {
                if (content != value)
                {
                    content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

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

        public List<ListBoxItem> ContentList
        {
            get { return contentList; }
            set
            {
                contentList = value;
                OnPropertyChanged("ContentList");
            }
        }

        //public int SelectedIndex
        //{
        //    get
        //    {
        //        return selectedIndex;
        //    }

        //    set
        //    {
        //        if (selectedIndex != value)
        //        {
        //            selectedIndex = value;
        //            OnPropertyChanged("Name");
        //        }
        //    }
        //}

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