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

    }
}