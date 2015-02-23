using System.Collections.ObjectModel;

namespace RegexViewer
{
    public class Worker
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

        public Worker()
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