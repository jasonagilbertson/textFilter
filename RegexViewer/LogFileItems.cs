using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogFileItems : BaseFileItems<LogFileItem>
    {
        #region Public Constructors

        public LogFileItems()
        {

            this.ContentItems = new ObservableCollection<LogFileItem>();
        }

        #endregion Public Constructors

        #region Public Properties

        public override ObservableCollection<LogFileItem> ContentItems { get; set; }

        #endregion Public Properties
    }
}