using System.Collections.Generic;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogFileProperties : BaseFileProperties<ListBoxItem>
    {
        #region Public Constructors

        public LogFileProperties()
        {
            this.Dirty = false;
            this.ContentItems = new List<ListBoxItem>();
        }

        #endregion Public Constructors

        #region Public Properties

        public override List<ListBoxItem> ContentItems { get; set; }

        #endregion Public Properties
    }
}