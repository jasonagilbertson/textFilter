// *********************************************************************** Assembly : RegexViewer
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-25-2015 ***********************************************************************
// <copyright file="WorkerItem.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace TextFilter
{
    public class WorkerItem
    {
        public BackgroundWorker BackGroundWorker = new BackgroundWorker();

        public int FilteredLineCount;

        public ObservableCollection<LogFileItem> FilteredList;

        public FilterFile FilterFile;
        public int FilterGroupCount;
        public FilterNeed FilterNeed;

        public LogFile LogFile;

        public int TotalLineCount;

        public Modification WorkerModification;

        public State WorkerState;
        internal List<FilterFileItem> VerifiedFilterItems;

        public WorkerItem()
        {
            WorkerModification = Modification.Unknown;
            BackGroundWorker.WorkerSupportsCancellation = true;
            Status = new StringBuilder();
        }

        public enum Modification
        {
            Unknown,

            FilterAdded,

            FilterRemoved,

            FilterModified,

            LogAdded,

            LogRemoved,

            LogIndex,

            FilterIndex
        }

        public enum State
        {
            Unknown,

            NotStarted,

            Started,

            Aborted,

            Ready,

            Completed,
        }

        public StringBuilder Status { get; set; }

        public override int GetHashCode()
        {
            return string.Format("{0}:{1}:{2}:{3}:",
                FilterFile == null ? "" : FilterFile.FileName,
                FilterFile == null ? "" : FilterFile.Tag,
                LogFile == null ? "" : LogFile.FileName,
                LogFile == null ? "" : LogFile.Tag).GetHashCode();
        }
    }
}