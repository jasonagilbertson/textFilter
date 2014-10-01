using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogTabViewModel : BaseTabViewModel
    {
        #region Private Fields

        private List<ListBoxItem> contentList;

        #endregion Private Fields

        #region Public Constructors

        public LogTabViewModel()
        {
            List<ListBoxItem> ContentList = new List<ListBoxItem>();
        }

        #endregion Public Constructors

        #region Public Properties

        public List<ListBoxItem> ContentList
        {
            get { return contentList; }
            set
            {
                contentList = value;
                OnPropertyChanged("ContentList");
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void CopyExecuted(object target)
        {
            try
            {
                Clipboard.Clear();
                //StringBuilder copyContent = new StringBuilder();
                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (ListBoxItem lbi in ContentList)
                {
                    if (lbi != null && lbi.IsSelected)
                        //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
                    {
                        //copyContent.AppendLine(lbi.Content.ToString());
                        htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
                    }
                }

              //  Clipboard.SetText(copyContent.ToString(), TextDataFormat.Text);
                htmlFragment.CopyToClipboard();
              //  HtmlFragment.CopyToClipboard(copyContent.ToString());
                
            }
            catch (Exception ex)
            {
                Debug.Print("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        #endregion Public Methods
    }
}