using System.Collections.Generic;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogProperties
    {
        #region Public Constructors

        public LogProperties()
        {
            this.Dirty = false;
            this.TextBlocks = new List<ListBoxItem>();
        }

        #endregion Public Constructors

        #region Public Properties

        public bool Dirty { get; set; }

        public string FileName { get; set; }

        public string Tag { get; set; }

        public List<ListBoxItem> TextBlocks { get; set; }

        #endregion Public Properties
    }
}