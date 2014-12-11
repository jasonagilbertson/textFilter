using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        #region Public Constructors

        public FilterTabViewModel()
        {
            //     PopulateColors();
        }

        #endregion Public Constructors


        #region Public Methods

        //public void CopyExecuted(List<ListBoxItem> ContentList)
        //public override void CopyExecuted(object contentList)
        //{
        //    try
        //    {
        //        List<FilterFileItem> list = (List<FilterFileItem>)contentList;


        //        HtmlFragment htmlFragment = new HtmlFragment();
        //        foreach (FilterFileItem lbi in list)
        //        {
        //            if (lbi != null && lbi.IsSelected)
        //            // if (lbi != null && lbi.IsFocused)
        //            //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
        //            {
        //                htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
        //            //htmlFragment.AddClipToList(lbi.Text, lbi.Background, lbi.Foreground);
        //            }
        //        }

        //        htmlFragment.CopyListToClipboard();
        //    }
        //    catch (Exception ex)
        //    {
        //        SetStatus("Exception:CopyCmdExecute:" + ex.ToString());
        //    }
        //}

     
        #endregion Public Methods
    }
}