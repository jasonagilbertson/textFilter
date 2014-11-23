using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogFileProperties : BaseFileProperties<LogFileItem>
    {
        #region Public Constructors

        public LogFileProperties()
        {
            this.Modified = false;
            this.ContentItems = new ObservableCollection<LogFileItem>();
        }

        #endregion Public Constructors

        #region Public Properties

        public override ObservableCollection<LogFileItem> ContentItems { get; set; }

        #endregion Public Properties
    }
}