using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            DateTime timer = DateTime.Now;
            SetStatus(string.Format("ApplyFilter:log file: {0} start time: {1}", logFile.Tag, timer.ToString("hh:mm:ss.fffffff")));
          
            int[] countTotals = new int[filterFileItems.Count];

            try
            {
                foreach (LogFileItem logItem in logFile.ContentItems)
                {
                    if (string.IsNullOrEmpty(logItem.Content))
                    {
                        continue;
                    }

                    int filterIndex = int.MaxValue;
                    //int matchIndex = int.MaxValue;
                    for (int c = 0; c < filterFileItems.Count; c++)
                    {
                        FilterFileItem fileItem = filterFileItems[c];
                        string pattern = fileItem.Filterpattern;
                        if (!fileItem.Regex)
                        {
                            pattern = Regex.Escape(pattern);
                        }
                    
                        Match match = Regex.Match(logItem.Content, pattern, RegexOptions.IgnoreCase);

                        if(match.Success && filterIndex > c)
                        {
                            //matchIndex = match.Index;
                            filterIndex = c;
                        }
                        
                        
                        if(match.Success)
                        {
                            countTotals[c] += 1;
                        }
                    }

                    if (filterIndex != int.MaxValue && !filterFileItems[filterIndex].Exclude)
                    {
                        logItem.Foreground = filterFileItems[filterIndex].Foreground;
                        logItem.Background = filterFileItems[filterIndex].Background;
                        logItem.FontSize = RegexViewerSettings.Settings.FontSize;
                        filteredItems.Add(logItem);
                    }
                }

                // write totals
                for (int i = 0; i < countTotals.Length; i++)
                {
                    filterFileItems[i].Count = countTotals[i];
                }
                SetStatus(string.Format("ApplyFilter:log file: {0} total time in seconds: {1}", logFile.Tag, DateTime.Now.Subtract(timer).TotalSeconds));
                return filteredItems;
            }
            catch (Exception e)
            {
                SetStatus("ApplyFilter:exception" + e.ToString());
                return filteredItems;
            }
        }

        public ObservableCollection<LogFileItem> ApplyFilterSlow(LogFile logFile, List<FilterFileItem> filterFileItems)
        {
            // not used but keeping for now. this one combines all filters into one search
            // testing shows this method slower than just searching for each filter separately
            ObservableCollection<LogFileItem> filteredItems = new ObservableCollection<LogFileItem>();
            DateTime timer = DateTime.Now;
            SetStatus(string.Format("ApplyFilter:log file: {0} start time: {1}", logFile.Tag, timer.ToString("hh:mm:ss.fffffff")));
            int[] countTotals = new int[filterFileItems.Count];

            StringBuilder filterPattern = new StringBuilder();

            foreach (FilterFileItem item in filterFileItems)
            {

                string pattern = item.Filterpattern;

                if (!item.Regex)
                {
                    pattern = Regex.Escape(pattern);
                }

                filterPattern.Append(string.Format("({0})|", pattern));
            }

            // remove trailing |
            if(!(filterPattern.Length == 0))
            {
                filterPattern.Remove(filterPattern.Length - 1, 1);
            }

            Regex regex = new Regex(filterPattern.ToString(),RegexOptions.Compiled|RegexOptions.IgnoreCase);
            MatchCollection mc;
           // int lowestIndex = int.MaxValue;
            

            try
            {
                foreach (LogFileItem logItem in logFile.ContentItems)
                //  foreach (LogFileItem logItem in ReadFile(logFile.Tag))
                {
                    if (string.IsNullOrEmpty(logItem.Content))
                    {
                        continue;
                    }
                    
                    int filterIndex = int.MaxValue; // -1;

                    // if(regex.IsMatch(logItem.Content))
                    if(((mc = regex.Matches(logItem.Content)).Count) > 0)
                    {
                        //foreach(Match m in mc)
                        //for (int c = 0; c < countTotals.Length & c < mc[0].Groups.Count; c++)
                        //for (int c = 0; c < mc[0].Groups.Count; c++)
                        for (int c = 0; c < mc.Count; c++)
                        {

                            // check for success and index
                            // all success gets counted
                            // match goes to lowest index
                            // exclusions mask any lower matches
                            for (int g = 0; g < mc[c].Groups.Count; g++)
                            {
                                if (mc[c].Groups[g + 1].Success)
                                {
                                    //Group group = mc[c].Groups[g + 1];
                                    countTotals[g] += 1;
                                    //if(group.Index > 0 & group.Index < lowestIndex)
                                    //if (group.Index < lowestIndex)
                                    if (filterIndex > g)
                                    {
                                        // lowestIndex = group.Index;
                                        filterIndex = g;
                                    }
                                }
                            }
                        }

                        if (filterIndex >= 0 && !filterFileItems[filterIndex].Exclude)
                        {
                            logItem.Foreground = filterFileItems[filterIndex].Foreground;
                            logItem.Background = filterFileItems[filterIndex].Background;
                            logItem.FontSize = RegexViewerSettings.Settings.FontSize;
                            filteredItems.Add(logItem);
                        }
                    }
                }

                // write totals
                for (int i = 0; i < countTotals.Length; i++)
                {
                    filterFileItems[i].Count = countTotals[i];
                }

                SetStatus(string.Format("ApplyFilter:log file: {0} total time in seconds: {1}", logFile.Tag, DateTime.Now.Subtract(timer).TotalSeconds));
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
               // logFile.ContentItems = (ObservableCollection<LogFileItem>)(IFile<LogFileItem>)ReadFile(LogName);
                logFile.ContentItems = new ObservableCollection<LogFileItem>(ReadFile(LogName));
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
                    logFileItem.Content = line;
                    logFileItem.Background = Settings.BackgroundColor;
                    logFileItem.Foreground = Settings.ForegroundColor;
                    logFileItem.FontSize = Settings.FontSize;
                    logFileItem.FontFamily = new System.Windows.Media.FontFamily("Courier");
                    //logFileItem.Index = count++;
                    logFileItems.Add(logFileItem);
                   // logFile.ContentItems.Add(logFileItem);
                }
            }

            return logFileItems;
        }
        public override IFile<LogFileItem> NewFile(string LogName)
        {
            throw new NotImplementedException();
        }
        public override bool SaveFile(string FileName, ObservableCollection<LogFileItem> list)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}