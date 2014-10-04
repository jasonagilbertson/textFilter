using System;

namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItems>
    {
        //   private List<FilterFileItems> contentList;

        #region Public Constructors

        public FilterTabViewModel()
        {
            //   List<FilterFileItems> ContentList = new List<FilterFileItems>();
        }

        #endregion Public Constructors

        //public List<FilterFileItems> ContentList
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
            throw new NotImplementedException();
            //    try
            //    {
            //        Clipboard.Clear();
            //        HtmlFragment htmlFragment = new HtmlFragment();
            //        foreach (FilterFileItems lbi in ContentList)
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