using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogTabViewModel : BaseTabViewModel<ListBoxItem>
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

        //public List<ListBoxItem> ContentList
        //{
        //    get { return contentList; }
        //    set
        //    {
        //        contentList = value;
        //        OnPropertyChanged("ContentList");
        //    }
        //}

        #region Public Methods

        public override void CopyExecuted(object target)
        {
            try
            {
                Clipboard.Clear();
                HtmlFragment htmlFragment = new HtmlFragment();
                foreach (ListBoxItem lbi in ContentList)
                {
                    if (lbi != null && lbi.IsSelected)
                    //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
                    {
                        htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
                    }
                }

                htmlFragment.CopyListToClipboard();
            }
            catch (Exception ex)
            {
                Debug.Print("Exception:CopyCmdExecute:" + ex.ToString());
            }
        }

        #endregion Public Methods
    }
}