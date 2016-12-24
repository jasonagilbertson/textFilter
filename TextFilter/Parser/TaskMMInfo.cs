using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace TextFilter
{
    public class TaskMMFInfo
    {
        #region Fields

        public Int32 length;
        public LogFile logFile;
        public MemoryMappedFile mmf;
        public Int32 position;
        public List<LogFileItem> stringList;
        public WorkerItem workerItem;

        #endregion Fields

        #region Properties

        public ManualResetEvent completedEvent { get; set; }

        public DoWorkEventArgs doWorkEventArgs { get; set; }

        #endregion Properties
    }
}