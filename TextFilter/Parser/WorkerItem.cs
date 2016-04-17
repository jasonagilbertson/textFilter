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

namespace TextFilter
{
    public class WorkerItem
    {
        #region Public Fields

        public BackgroundWorker BackGroundWorker = new BackgroundWorker();

        public int FilteredLineCount;

        public ObservableCollection<LogFileItem> FilteredList;

        public int FilterGroupCount;

        public FilterFile FilterFile;

        public FilterNeed FilterNeed;

        public LogFile LogFile;

        public int TotalLineCount;

        public Modification WorkerModification;

        public State WorkerState;
        internal List<FilterFileItem> VerifiedFilterItems;

        #endregion Public Fields

        #region Public Constructors

        public WorkerItem()
        {
            WorkerModification = Modification.Unknown;
            BackGroundWorker.WorkerSupportsCancellation = true;
        }

        #endregion Public Constructors

        #region Public Enums

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

        #endregion Public Enums
    }
}