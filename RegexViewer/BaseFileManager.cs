using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RegexViewer
{
    public abstract class BaseFileManager<T> : RegexViewer.IFileManager<T>
    {
        #region Public Fields

        public static TraceSource ts = new TraceSource("RegexViewer.LogManager");
        public RegexViewerSettings Settings = RegexViewerSettings.Settings;

        #endregion Public Fields

        #region Public Constructors

        public BaseFileManager()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        public List<IFileProperties<T>> Files { get; set; }

        #endregion Public Properties

        //public abstract bool CloseLog(string FileName);

        #region Public Methods

        public bool CloseFile(string FileName)
        {
            if (Files.Exists(x => String.Compare(x.Tag, FileName, true) == 0))
            {
                Files.Remove(Files.Find(x => String.Compare(x.Tag, FileName, true) == 0));
                this.Settings.RemoveLogFile(FileName);
                return true;
            }
            else
            {
                ts.TraceEvent(TraceEventType.Error, 3, "file not open:" + FileName);
                return false;
            }
        }

        public abstract IFileProperties<T> OpenFile(string LogName);

        public abstract List<IFileProperties<T>> OpenFiles(string[] files);

        public abstract bool SaveFile(string FileName, List<T> list);

        #endregion Public Methods
    }
}