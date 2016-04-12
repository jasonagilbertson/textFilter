// ***********************************************************************
// Assembly         : TextFilter
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-31-2015
// ***********************************************************************
// <copyright file="WorkerItem.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.ObjectModel;

namespace TextFilter
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