using System;
using System.Collections.Generic;

namespace RegexViewer
{
    public class LogTabViewModel : BaseTabViewModel<LogFileItem>
    {
        #region Public Constructors

        public LogTabViewModel()
        {
            List<LogFileItem> ContentList = new List<LogFileItem>();
        }

        #endregion Public Constructors

        //public List<LogFileItem> ContentList
        //{
        //    get { return contentList; }
        //    set
        //    {
        //        contentList = value;
        //        OnPropertyChanged("ContentList");
        //    }
        //}

        //private Command copyCommand;
        //public Command CopyCommand
        //{
        //    get
        //    {
        //        if (copyCommand == null)
        //        {
        //            copyCommand = new Command(CopyExecuted);
        //        }
        //        copyCommand.CanExecute = true;

        //        return copyCommand;
        //    }
        //    set { copyCommand = value; }
        //}

        #region Public Methods

        //public void CopyExecuted(List<ListBoxItem> ContentList)
        //public override void CopyExecuted(object contentList)
        //{
        //    try
        //    {
        //        //   List<LogFileItem> ContentList = (List<LogFileItem>)contentList;


        //        HtmlFragment htmlFragment = new HtmlFragment();
        //        foreach (LogFileItem lbi in SelectedContent)
        //        {
        //            //if (lbi != null && lbi.IsSelected)
        //            // if (lbi != null && lbi.IsFocused)
        //            //&& htmlFragment.Length < (copyContent.MaxCapacity - lbi.Content.ToString().Length))
        //            //{
        //            //    htmlFragment.AddClipToList(lbi.Content.ToString(), lbi.Background, lbi.Foreground);
        //            htmlFragment.AddClipToList(lbi.Content, lbi.Background, lbi.Foreground);
        //            //}
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