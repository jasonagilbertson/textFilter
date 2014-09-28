using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegexViewer
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private string content;
        private List<ListBoxItem> contentList;
//        private List<ListBoxItem> selectedItems;
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
            //copyCommand = new Command(CopyExecuted);
  //          List<ListBoxItem> SelectedItems = new List<ListBoxItem>();
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

    

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

        
        public void CopyExecuted(object target)
        {
            try
            {
                Clipboard.Clear();
                StringBuilder copyContent = new StringBuilder();
                HtmlFragment htmlFragment = new HtmlFragment();
                //foreach (ListBoxItem lbi in SelectedItems)
                foreach (ListBoxItem lbi in ContentList)
                {
                    if (lbi != null && lbi.IsSelected
                        && copyContent.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
                    {
                       // copyContent.AppendLine(lbi.Content.ToString());
                        //string test = HtmlConverter.ToHtml(lbi.Content.ToString());
                        htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
                        //copyContent.AppendLine(test);
                    }
                }

                //Clipboard.SetText(copyContent.ToString());
                //Clipboard.SetText(copyContent.ToString(),TextDataFormat.Html);
               // Clipboard.SetData("HTML Format", copyContent);
                htmlFragment.CopyToClipboard();
                
            }
            catch (Exception ex)
            {
                Debug.Print("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        private Command copyCommand;
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
        public List<ListBoxItem> ContentList
        {
            get { return contentList; }
            set
            {
                contentList = value;
                OnPropertyChanged("ContentList");
            }
        }

        //public List<ListBoxItem> SelectedItems
        //{
        //    get { return selectedItems; }
        //    set
        //    {
        //        selectedItems = value;
        //        OnPropertyChanged("SelectedItems");
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