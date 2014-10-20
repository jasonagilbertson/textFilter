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
            this.Files = new List<IFileProperties<LogFileItem>>();
        }

        #endregion Public Constructors

        #region Public Methods

        public override IFileProperties<LogFileItem> OpenFile(string LogName)
        {
            IFileProperties<LogFileItem> logProperties = new LogFileProperties();
            if (Files.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                // ts.TraceEvent(TraceEventType.Error, 1, "file already open:" + LogName);
                MainModel.SetStatus("file already open:" + LogName);
                return logProperties;
            }

            if (File.Exists(LogName))
            {
                logProperties.FileName = Path.GetFileName(LogName);
                logProperties.Tag = LogName;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(LogName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        LogFileItem textBlock = new LogFileItem();
                        textBlock.Content = line;
                        textBlock.Background = Settings.BackgroundColor;
                        textBlock.Foreground = Settings.ForegroundColor;
                        textBlock.FontSize = Settings.FontSize;
                        textBlock.FontFamily = new System.Windows.Media.FontFamily("Courier");
                        logProperties.ContentItems.Add(textBlock);
                    }
                }

                Files.Add(logProperties);
                this.Settings.AddLogFile(LogName);
            }
            else
            {
                // ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                MainModel.SetStatus("log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return logProperties;
        }

        public override List<IFileProperties<LogFileItem>> OpenFiles(string[] files)
        {
            List<IFileProperties<LogFileItem>> textBlockItems = new List<IFileProperties<LogFileItem>>();

            foreach (string file in files)
            {
                LogFileProperties logProperties = new LogFileProperties();
                if (String.IsNullOrEmpty((logProperties = (LogFileProperties)OpenFile(file)).Tag))
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