using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace RegexViewer
{
    public abstract class BaseFileManager<T> : RegexViewer.IFileManager<T>
    {
        #region Private Fields

        private static TraceSource _ts = new TraceSource("RegexViewer.LogManager");
        private RegexViewerSettings settings = RegexViewerSettings.Settings;

        #endregion Private Fields
        public  static TraceSource ts = new TraceSource("RegexViewer.LogManager");
        public RegexViewerSettings Settings = RegexViewerSettings.Settings;

        #region Public Constructors

        public BaseFileManager()
        {
            
        }

        #endregion Public Constructors

        #region Public Properties

        public List<IFileProperties<T>> Files { get; set; }

        #endregion Public Properties

        #region Public Methods

        public abstract bool CloseLog(string FileName);

        public abstract IFileProperties<T> OpenFile(string LogName);


        public abstract List<IFileProperties<T>> OpenFiles(string[] files);
        

        #endregion Public Methods
    }
}