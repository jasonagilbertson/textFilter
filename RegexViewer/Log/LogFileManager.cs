using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


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

        
        public ObservableCollection<LogFileItem> ApplyFilter(LogFile logFile, List<FilterFileItem> filterFileItems, FilterCommand filterCommand)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            DateTime timer = DateTime.Now;
            SetStatus(string.Format("ApplyFilter:start time: {0} log file: {1} ", timer.ToString("hh:mm:ss.fffffff"), logFile.Tag));

            int[] countTotals = new int[filterFileItems.Count];

            DateTime lastUpdate = DateTime.Now;

            List<FilterFileItem> filterItems = VerifyFilterPatterns(filterFileItems);
            
            
            try
            {
                Parallel.ForEach(logFile.ContentItems, logItem =>
                //foreach (LogFileItem logItem in logFile.ContentItems)
                {
                    if (string.IsNullOrEmpty(logItem.Content))
                    {
                        // used for goto line as it needs all line items
                        //continue;
                        logItem.FilterIndex = -1;
                        return;
                    }

                    int filterIndex = int.MaxValue;
                    bool exclude = false;

                    for (int c = 0; c < filterItems.Count; c++)
                    {
                        FilterFileItem filterItem = filterItems[c];

                        if (!filterItem.Regex && !logItem.Content.ToLower().Contains(filterItem.Filterpattern.ToLower()))
                        {
                            logItem.FilterIndex = -1;
                            continue;
                        }
                        else if (!Regex.IsMatch(logItem.Content, filterItem.Filterpattern, RegexOptions.IgnoreCase))
                        {
                            logItem.FilterIndex = -1;
                            continue;
                        }

                        if (filterIndex > c)
                        {
                            if (filterItem.Exclude)
                            {
                                exclude = true;
                                logItem.FilterIndex = -1;
                            }

                            filterIndex = c;
                        }

                        countTotals[c] += 1;
                        if (!Settings.CountMaskedMatches)
                        {
                            break;
                        }
                    }

                    
                    if (filterIndex > -1 && filterIndex != int.MaxValue && !exclude)
                    {
                            logItem.FilterIndex = filterIndex;
                    }
                }
                
                );

                // write totals
                for (int i = 0; i < countTotals.Length; i++)
                {
                    filterFileItems[i].Count = countTotals[i];
                    SetStatus(string.Format("ApplyFilter:counttotals: {0}", countTotals[i]));
                }

                SetStatus(string.Format("ApplyFilter:total time in seconds: {0} logfile line count: {1} log file: {2}", DateTime.Now.Subtract(timer).TotalSeconds, logFile.ContentItems.Count, logFile.Tag));
                Mouse.OverrideCursor = null;
                
                return new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex != -1));

            }
            catch (Exception e)
            {
                SetStatus("ApplyFilter:exception" + e.ToString());
                Mouse.OverrideCursor = null;
                return new ObservableCollection<LogFileItem>();
            }
        }
        private List<FilterFileItem> VerifyFilterPatterns(List<FilterFileItem> filterFileItems)
        {
            List<FilterFileItem> filterItems = new List<FilterFileItem>();
            foreach (FilterFileItem filterItem in filterFileItems)
            {
                if (string.IsNullOrEmpty(filterItem.Filterpattern))
                {
                    continue;
                }
                FilterFileItem newFilter = new FilterFileItem()
                {
                    Background = filterItem.Background,
                    Enabled = filterItem.Enabled,
                    Exclude = filterItem.Exclude,
                    Filterpattern = filterItem.Filterpattern,
                    Foreground = filterItem.Foreground,
                    Regex = filterItem.Regex

                };

                if (newFilter.Regex)
                {

                    try
                    {
                        Regex test = new Regex(filterItem.Filterpattern);

                    }
                    catch
                    {
                        SetStatus("quick find not a regex:" + filterItem.Filterpattern);
                        newFilter.Regex = false;
                        newFilter.Filterpattern = Regex.Escape(filterItem.Filterpattern);
                    }
                }

                filterItems.Add(newFilter);
            }
            return filterItems;
        }



        public ObservableCollection<LogFileItem> ApplyColor(ObservableCollection<LogFileItem> logFileItems, List<FilterFileItem> filterFileItems)//, bool showAll = false)
        {
            DateTime timer = DateTime.Now;
            SetStatus(string.Format("ApplyColor:start time: {0}", timer.ToString("hh:mm:ss.fffffff")));

            List<FilterFileItem> filterItems = VerifyFilterPatterns(filterFileItems);

            try
            {
                foreach (LogFileItem item in logFileItems)
                {

                    if (item.FilterIndex == -1 | item.FilterIndex >= filterItems.Count)
                    {
                        item.Background = Settings.BackgroundColor;
                        item.Foreground = Settings.ForegroundColor;
                    }
                    else
                    {
                        item.Foreground = filterItems[item.FilterIndex].Foreground;
                        item.Background = filterItems[item.FilterIndex].Background;
                    }

                    item.FontSize = Settings.FontSize;
                    
                }
                
                    
                SetStatus(string.Format("ApplyColor:total time in seconds: {0}", DateTime.Now.Subtract(timer).TotalSeconds));

                //if (showAll)
                //{
                //    return logFileItems;
                //}
                //else
                //{
                    return new ObservableCollection<LogFileItem>(logFileItems.Where(x => x.FilterIndex != -1));
                //}
            }
            catch (Exception e)
            {
                SetStatus("ApplyColor:exception" + e.ToString());

                return new ObservableCollection<LogFileItem>();
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
                //argItem.Value.ArgReplacementString = Encoding.ASCII.GetString(tmfTraceMsg.EventStringBytes)
                //            .Substring(_startIndex, _nextIndex)
                //            .Replace("\0", "");
                // http://cc.davelozinski.com/c-sharp/the-fastest-way-to-read-and-process-text-files
                while ((line = sr.ReadLine()) != null)
                //while ((line = sr.ReadLine().Replace("\0","")) != null)
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

        
    }
}