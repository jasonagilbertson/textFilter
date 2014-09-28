using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogFileManager : BaseFileManager<ListBoxItem>
    {
        #region Private Fields

        

        #endregion Private Fields

        #region Public Constructors

        public LogFileManager()
        {
            this.Files = new List<IFileProperties<ListBoxItem>>();
        }

        #endregion Public Constructors

        #region Public Properties

     //   public List<IFileProperties<ListBoxItem>> Files { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override bool CloseLog(string FileName)
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

        public override IFileProperties<ListBoxItem> OpenFile(string LogName)
        {
            IFileProperties<ListBoxItem> logProperties = new LogFileProperties();
            if (Files.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                ts.TraceEvent(TraceEventType.Error, 1, "file already open:" + LogName);
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
                        ListBoxItem textBlock = new ListBoxItem();
                        textBlock.Content = line;
                        textBlock.Background = Settings.BackgroundColor;
                        textBlock.Foreground = Settings.FontColor;
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
                ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return logProperties;
        }

        public override List<IFileProperties<ListBoxItem>> OpenFiles(string[] files)
        {
            List<IFileProperties<ListBoxItem>> textBlockItems = new List<IFileProperties<ListBoxItem>>();

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

        #endregion Public Methods
    }
}