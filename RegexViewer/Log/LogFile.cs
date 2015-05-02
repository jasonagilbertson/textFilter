using System.Collections.ObjectModel;

namespace RegexViewer
{
    public class LogFile : BaseFile<LogFileItem>
    {
        #region Public Constructors

        public LogFile()
        {
            this.ContentItems = new ObservableCollection<LogFileItem>();
            this.ExportConfiguration = new ExportDialog.Results();
        }

        #endregion Public Constructors

        #region Public Properties

        public override ObservableCollection<LogFileItem> ContentItems { get; set; }

        #endregion Public Properties

        public ExportDialog.Results ExportConfiguration { get; set; }
    }
}