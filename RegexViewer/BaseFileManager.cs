using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace RegexViewer
{
    public abstract class BaseFileManager<T> : Base, IFileManager<T>
    {
        #region Public Fields
        

        public RegexViewerSettings Settings = RegexViewerSettings.Settings;
       
        #endregion Public Fields

        public BaseFileManager()
        {
          //  MainModel = mainModel;
        }

        #region Public Properties

        public List<IFileItems<T>> ListFileItems { get; set; }
        
        #endregion Public Properties

        //public abstract bool CloseLog(string FileName);

        #region Public Methods

        public bool CloseFile(string FileName)
        {
            if (ListFileItems.Exists(x => String.Compare(x.Tag, FileName, true) == 0))
            {
                SetStatus("file not open:" + FileName);
                ListFileItems.Remove(ListFileItems.Find(x => String.Compare(x.Tag, FileName, true) == 0));
                this.Settings.RemoveLogFile(FileName);
                return true;
            }
            else
            {
                //ts.TraceEvent(TraceEventType.Error, 3, "file not open:" + FileName);
                SetStatus("file not open:" + FileName);
                
                return false;
            }
        }
      
        public abstract IFileItems<T> OpenFile(string LogName);

        public abstract List<IFileItems<T>> OpenFiles(string[] files);

        public abstract bool SaveFile(string FileName, ObservableCollection<T> list);

        #endregion Public Methods
    }
}