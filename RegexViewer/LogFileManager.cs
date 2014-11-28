using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace RegexViewer
{
    public class LogFileManager : BaseFileManager<LogFileItem>
    {
        #region Public Constructors

        public LogFileManager()
        {
            this.ListFileItems = new List<IFileItems<LogFileItem>>();
        }

        #endregion Public Constructors

        #region Public Methods

        public override IFileItems<LogFileItem> OpenFile(string LogName)
        {
            IFileItems<LogFileItem> logFileItems = new LogFileItems();
            if (ListFileItems.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                
                SetStatus("file already open:" + LogName);
                return logFileItems;
            }

            if (File.Exists(LogName))
            {
                logFileItems.FileName = Path.GetFileName(LogName);
                logFileItems.Tag = LogName;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(LogName))
                {
                    string line;
                    //Int64 count = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        LogFileItem logFileItem = new LogFileItem();
                        //logFileItem.Content = line;
                        logFileItem.Text = line;
                        logFileItem.Background = Settings.BackgroundColor;
                        logFileItem.Foreground = Settings.ForegroundColor;
                        logFileItem.FontSize = Settings.FontSize;
                        logFileItem.FontFamily = new System.Windows.Media.FontFamily("Courier");
                        //logFileItem.Index = count++;
                        logFileItems.ContentItems.Add(logFileItem);
                    }
                }

                ListFileItems.Add(logFileItems);
                this.Settings.AddLogFile(LogName);
            }
            else
            {
                // ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                SetStatus("log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return logFileItems;
        }

        public override List<IFileItems<LogFileItem>> OpenFiles(string[] files)
        {
            List<IFileItems<LogFileItem>> textBlockItems = new List<IFileItems<LogFileItem>>();

            foreach (string file in files)
            {
                LogFileItems logProperties = new LogFileItems();
                if (String.IsNullOrEmpty((logProperties = (LogFileItems)OpenFile(file)).Tag))
                {
                    continue;
                }

                textBlockItems.Add(logProperties);
            }

            return textBlockItems;
        }

        public override bool SaveFile(string FileName, ObservableCollection<LogFileItem> list)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}