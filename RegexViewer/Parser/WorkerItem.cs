using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public class WorkerItem
    {
        #region Public Fields

        public int FilteredLineCount;
        public ObservableCollection<LogFileItem> FilteredList;
        public FilterFile FilterFile;
        public FilterNeed FilterNeed;
        public LogFile LogFile;
        public int TotalLineCount;
        public Modification WorkerModification;

        #endregion Public Fields

        #region Public Constructors

        public WorkerItem()
        {
            WorkerModification = Modification.Unknown;
        }

        #endregion Public Constructors

        #region Public Enums

        public enum Modification
        {
            FilterAdded,
            FilterRemoved,
            FilterModified,
            LogAdded,
            LogRemoved,
            Unknown,
            LogIndex,
            FilterIndex
        }

        #endregion Public Enums
    }
}