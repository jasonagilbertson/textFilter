using System.Collections.ObjectModel;

namespace RegexViewer
{
    public class LogFile : BaseFile<LogFileItem>
    {
        #region Public Constructors
        public class ExportConfigurationInfo
        {
            public bool Index;
            public bool Content = true;
            public bool Group1;
            public bool Group2;
            public bool Group3;
            public bool Group4;
            public string Separator = ",";
            public bool Cancel;
            public bool Copy;
            public bool RemoveEmpty;
        }
        public LogFile()
        {
            this.ContentItems = new ObservableCollection<LogFileItem>();
            this.ExportConfiguration = new ExportConfigurationInfo();
        }

        #endregion Public Constructors

        #region Public Properties

        public override ObservableCollection<LogFileItem> ContentItems { get; set; }

        #endregion Public Properties

        public ExportConfigurationInfo ExportConfiguration { get; set; }
    }
}