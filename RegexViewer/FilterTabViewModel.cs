using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel
    {
        #region Private Fields

        private List<DataRow> contentList;

        #endregion Private Fields

        #region Public Constructors

        public FilterTabViewModel()
        {
            List<DataRow> ContentList = new List<DataRow>();
        }

        #endregion Public Constructors

        #region Public Properties

        public List<DataRow> ContentList
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
            throw new NotImplementedException();
        //    try
        //    {
        //        Clipboard.Clear();
        //        HtmlFragment htmlFragment = new HtmlFragment();
        //        foreach (DataRow lbi in ContentList)
        //        {
        //            if (lbi != null && lbi.IsSelected)
        //            //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
        //            {
        //                htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
        //            }
        //        }

        //        htmlFragment.CopyListToClipboard();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Print("Exception:CopyCmdExecute:" + ex.ToString());
        //    }
        }

        #endregion Public Methods
    }
}