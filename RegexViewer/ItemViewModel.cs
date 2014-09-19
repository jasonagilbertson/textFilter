using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RegexViewer
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private string name;
        private string header;
        private string content;
        private string tag;
        private List<TextBlock> contentList;
        //private int selectedIndex;

        public List<TextBlock> ContentList
        {
            get { return contentList; }
            set 
            {

                    contentList = value;
                    OnPropertyChanged("ContentList");
                
            }
        }
        public ItemViewModel()
        {
            
            List<TextBlock> ContentList = new List<TextBlock>();
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
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

}
