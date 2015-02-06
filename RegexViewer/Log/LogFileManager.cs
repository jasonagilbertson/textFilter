using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RegexViewer
{
    public class LogFileManager : BaseFileManager<LogFileItem>
    {
        #region Public Methods

        public override bool SaveFile(string FileName, ObservableCollection<LogFileItem> list)
        {
              try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }

                using (StreamWriter writer = File.CreateText(FileName))
                {
                    foreach (LogFileItem item in list)
                    {
                        writer.WriteLine(item.Content);
                    }

                    writer.Close();
                }

                SetStatus("saving file:" + FileName);
                return true;
            }
              catch (Exception e)
              {
                  SetStatus("SaveFile:exception: " + e.ToString());
                  return false;
              }


        }

        #endregion Public Methods

        #region Public Constructors

        public LogFileManager()
        {
            this.FileManager = new List<IFile<LogFileItem>>();
        }

        #endregion Public Constructors

        public ObservableCollection<LogFileItem> ApplyFilter(LogFile logFile, List<FilterFileItem> filterFileItems, bool onlyHighlight)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            // todo: move to filter class
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
                        if (onlyHighlight)
                        {
                            filteredItems.Add(logItem);
                        }

                        continue;
                    }

                    int filterIndex = int.MaxValue;
                    bool exclude = false;
                    for (int c = 0; c < filterFileItems.Count; c++)
                    {
                        FilterFileItem fileItem = filterFileItems[c];
                        string pattern = fileItem.Filterpattern;
                        if (!fileItem.Regex)
                        {
                            pattern = Regex.Escape(pattern);
                        }

                        if (!Regex.IsMatch(logItem.Content, pattern, RegexOptions.IgnoreCase))
                        {
                            continue;
                        }

                        if (filterIndex > c)
                        {
                            if (filterFileItems[c].Exclude)
                            {
                                exclude = true;
                            }

                            filterIndex = c;
                        }

                        countTotals[c] += 1;
                        if(!Settings.CountMaskedMatches)
                        {
                            break;
                        }
                    }

                    if (filterIndex != int.MaxValue && !exclude)
                    {
                        logItem.Foreground = filterFileItems[filterIndex].Foreground;
                        logItem.Background = filterFileItems[filterIndex].Background;
                        logItem.FontSize = Settings.FontSize;
                        filteredItems.Add(logItem);
                    }
                    else if (onlyHighlight)
                    {
                        logItem.Foreground = Settings.ForegroundColor;
                        logItem.Background = Settings.BackgroundColor;
                        logItem.FontSize = Settings.FontSize;
                        filteredItems.Add(logItem);
                    }
                }

                // write totals
                for (int i = 0; i < countTotals.Length; i++)
                {
                    filterFileItems[i].Count = countTotals[i];
                }
                SetStatus(string.Format("ApplyFilter:log file: {0} total time in seconds: {1}", logFile.Tag, DateTime.Now.Subtract(timer).TotalSeconds));
                Mouse.OverrideCursor = null;
                return filteredItems;
                
            }
            catch (Exception e)
            {
                SetStatus("ApplyFilter:exception" + e.ToString());
                Mouse.OverrideCursor = null;
                return filteredItems;
                
            }
        }

        public ObservableCollection<LogFileItem> ApplyFilterSlow(LogFile logFile, List<FilterFileItem> filterFileItems)
        {
            // not used but keeping for now. this one combines all filters into one search testing
            // shows this method slower than just searching for each filter separately
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
            if (!(filterPattern.Length == 0))
            {
                filterPattern.Remove(filterPattern.Length - 1, 1);
            }

            Regex regex = new Regex(filterPattern.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection mc;
            // int lowestIndex = int.MaxValue;

            try
            {
                foreach (LogFileItem logItem in logFile.ContentItems)
                // foreach (LogFileItem logItem in ReadFile(logFile.Tag))
                {
                    if (string.IsNullOrEmpty(logItem.Content))
                    {
                        continue;
                    }

                    int filterIndex = int.MaxValue; // -1;

                    // if(regex.IsMatch(logItem.Content))
                    if (((mc = regex.Matches(logItem.Content)).Count) > 0)
                    {
                        //foreach(Match m in mc)
                        //for (int c = 0; c < countTotals.Length & c < mc[0].Groups.Count; c++)
                        //for (int c = 0; c < mc[0].Groups.Count; c++)
                        for (int c = 0; c < mc.Count; c++)
                        {
                            // check for success and index all success gets counted match goes to
                            // lowest index exclusions mask any lower matches
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

        public override IFile<LogFileItem> NewFile(string LogName)
        {
            throw new NotImplementedException();
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
                int count = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    LogFileItem logFileItem = new LogFileItem();
                    //logFileItem.Content = line;
                    logFileItem.Content = line;
                    logFileItem.Background = Settings.BackgroundColor;
                    logFileItem.Foreground = Settings.ForegroundColor;
                    logFileItem.FontSize = Settings.FontSize;
                    logFileItem.FontFamily = new System.Windows.Media.FontFamily(Settings.FontName);
                    logFileItem.Index = count++;
                    logFileItems.Add(logFileItem);
                    // logFile.ContentItems.Add(logFileItem);
                }

                sr.Close();
            }

            return logFileItems;
        }

        public ObservableCollection<LogFileItem> ResetColors(ObservableCollection<LogFileItem> logFileItems)
        {
            foreach (LogFileItem item in logFileItems)
            {
                item.Background = Settings.BackgroundColor;
                item.Foreground = Settings.ForegroundColor;
            }

            return logFileItems;
        }
    }
}