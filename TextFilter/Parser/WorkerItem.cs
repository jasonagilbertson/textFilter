// ************************************************************************************
// Assembly: TextFilter
// File: WorkerItem.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TextFilter
{
    public class WorkerItem
    {
        public BackgroundWorker BackGroundWorker = new BackgroundWorker();

        public int FilteredLineCount;

        public ObservableCollection<LogFileItem> FilteredList;

        public FilterFile FilterFile;

        public FilterNeed FilterNeed;

        public LogFile LogFile;

        public int TotalLineCount;

        public Modification WorkerModification;

        public State WorkerState;

        public WorkerItem()
        {
            WorkerModification = Modification.Unknown;
            BackGroundWorker.WorkerSupportsCancellation = true;
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
    }
}