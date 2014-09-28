using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace RegexViewer
{
    public class FilterFileManager : BaseFileManager<DataRow>
    {
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

        public override IFileProperties<DataRow> OpenFile(string LogName)
        {
            IFileProperties<DataRow> filterProperties = new FilterFileProperties();
            if (Files.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                ts.TraceEvent(TraceEventType.Error, 1, "file already open:" + LogName);
                return filterProperties;
            }

            if (File.Exists(LogName))
            {
                filterProperties.FileName = Path.GetFileName(LogName);
                filterProperties.Tag = LogName;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(LogName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // DataRow textBlock = new DataRow();
                        //textBlock.Content = line;
                        //textBlock.Background = Settings.BackgroundColor;
                        //textBlock.Foreground = Settings.FontColor;
                        //textBlock.FontSize = Settings.FontSize;
                        //textBlock.FontFamily = new System.Windows.Media.FontFamily("Courier");
                        //  filterProperties.ContentItems.Add(textBlock);
                    }
                }

                Files.Add(filterProperties);
                this.Settings.AddLogFile(LogName);
            }
            else
            {
                ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return filterProperties;
        }

        public override List<IFileProperties<DataRow>> OpenFiles(string[] files)
        {
            List<IFileProperties<DataRow>> textBlockItems = new List<IFileProperties<DataRow>>();

            foreach (string file in files)
            {
                FilterFileProperties logProperties = new FilterFileProperties();
                if (String.IsNullOrEmpty((logProperties = (FilterFileProperties)OpenFile(file)).Tag))
                {
                    continue;
                }

                textBlockItems.Add(logProperties);
            }

            return textBlockItems;
        }


    }
}