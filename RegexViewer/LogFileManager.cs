using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegexViewer
{
    public class LogFileManager : BaseFileManager<LogFileItem>
    {
        #region Public Constructors

        public LogFileManager()
        {
            this.FileManager = new List<IFile<LogFileItem>>();
        }

        #endregion Public Constructors

        #region Private Fields

        private List<FilterFileItem> _previousFilterFileItems = new List<FilterFileItem>();

        #endregion Private Fields

        #region Public Methods

        public ObservableCollection<LogFileItem> ApplyFilter(LogFile logFile, List<FilterFileItem> filterFileItems)
        {
            ObservableCollection<LogFileItem> filteredItems = new ObservableCollection<LogFileItem>();

            SetStatus("ApplyFilter:" + logFile.Tag);
            int[] countTotals = new int[filterFileItems.Count];

            try
            {
                //foreach (LogFileItem logItem in logFileItems)
                foreach (LogFileItem logItem in ReadFile(logFile.Tag))
                {
                    if (string.IsNullOrEmpty(logItem.Text))
                    {
                        continue;
                    }

                    //Debug.Print(logItem.Text);

                    for (int c = 0; c < filterFileItems.Count; c++)
                    {
                        FilterFileItem fileItem = filterFileItems[c];
                        string pattern = fileItem.Filterpattern;
                        if (!fileItem.Regex)
                        {
                            pattern = Regex.Escape(pattern);
                        }

                        if (!Regex.IsMatch(logItem.Text, pattern, RegexOptions.IgnoreCase))
                        {
                            continue;
                        }

                        if (fileItem.Exclude)
                        {
                            break;
                        }

                        LogFileItem item = new LogFileItem()
                        {
                            Text = logItem.Text,
                            Foreground = fileItem.Foreground,
                            Background = fileItem.Background,
                            FontSize = RegexViewerSettings.Settings.FontSize
                        };

                        countTotals[c] += 1;
                        filteredItems.Add(item);
                        break;
                    }
                }

                // write totals
                for (int i = 0; i < countTotals.Length; i++)
                {
                    filterFileItems[i].Count = countTotals[i];
                }

                return filteredItems;
            }
            catch (Exception e)
            {
                SetStatus("ApplyFilter:exception" + e.ToString());
                return filteredItems;
            }
        }

        public List<FilterFileItem> CleanFilterList(FilterFile filterFile)
        {
            List<FilterFileItem> fileItems = new List<FilterFileItem>();
            // clean up list
            foreach (FilterFileItem fileItem in filterFile.ContentItems.OrderBy(x => x.Index))
            {
                if (!fileItem.Enabled || string.IsNullOrEmpty(fileItem.Filterpattern))
                {
                    continue;
                }

                fileItems.Add(fileItem);
            }

            return fileItems;
        }

        public bool CompareFilterList(List<FilterFileItem> filterFileItems)
        {
            bool retval = false;
            if (_previousFilterFileItems.Count > 0
                && filterFileItems.Count > 0
                && _previousFilterFileItems.Count == filterFileItems.Count)
            {
                int i = 0;
                foreach (FilterFileItem fileItem in filterFileItems.OrderBy(x => x.Index))
                {
                    FilterFileItem previousItem = _previousFilterFileItems[i++];
                    if (previousItem.BackgroundColor != fileItem.BackgroundColor
                        || previousItem.ForegroundColor != fileItem.ForegroundColor
                        || previousItem.Enabled != fileItem.Enabled
                        || previousItem.Exclude != fileItem.Exclude
                        || previousItem.Regex != fileItem.Regex
                        || previousItem.Filterpattern != fileItem.Filterpattern)
                    {
                        retval = false;
                        Debug.Print("returning false");
                        break;
                    }

                    retval = true;
                }
            }

            _previousFilterFileItems.Clear();
            foreach (FilterFileItem item in filterFileItems)
            {
                _previousFilterFileItems.Add((FilterFileItem)item.ShallowCopy());
            }

            Debug.Print("CompareFilterList:returning:" + retval.ToString());
            return retval;
        }

        public override IFile<LogFileItem> OpenFile(string LogName)
        {
            IFile<LogFileItem> logFile = new LogFile();
            if (FileManager.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                SetStatus("file already open:" + LogName);
                return logFile;
            }

            if (File.Exists(LogName))
            {
                logFile.FileName = Path.GetFileName(LogName);
                logFile.Tag = LogName;
                // logFile.ContentItems = ReadFile(LogName);

                FileManager.Add(logFile);
                this.Settings.AddLogFile(LogName);
            }
            else
            {
                // ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                SetStatus("log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return logFile;
        }

        public override List<IFile<LogFileItem>> OpenFiles(string[] files)
        {
            List<IFile<LogFileItem>> textBlockItems = new List<IFile<LogFileItem>>();

            foreach (string file in files)
            {
                LogFile logFile = new LogFile();
                if (String.IsNullOrEmpty((logFile = (LogFile)OpenFile(file)).Tag))
                {
                    continue;
                }

                textBlockItems.Add(logFile);
            }

            return textBlockItems;
        }

        public override List<LogFileItem> ReadFile(string logFile)
        {
            // todo: use mapped file only for large files?
            List<LogFileItem> logFileItems = new List<LogFileItem>();

            using (System.IO.StreamReader sr = new System.IO.StreamReader(logFile))
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
                    logFileItems.Add(logFileItem);
                    //logFile.ContentItems.Add(logFileItem);
                }
            }

            return logFileItems;
        }

        public override bool SaveFile(string FileName, ObservableCollection<LogFileItem> list)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}