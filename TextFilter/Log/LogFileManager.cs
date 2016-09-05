// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="LogFileManager.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TextFilter
{
    public class LogFileManager : BaseFileManager<LogFileItem>
    {

        #region Constructors

        public LogFileManager()
        {
            FileManager = new List<IFile<LogFileItem>>();
        }

        #endregion Constructors

        #region Methods

        public ObservableCollection<LogFileItem> ApplyColor(ObservableCollection<LogFileItem> logFileItems, List<FilterFileItem> filterFileItems, bool showAll = false)
        {
            DateTime timer = DateTime.Now;
            SetStatus(string.Format("ApplyColor:start time: {0}", timer.ToString("hh:mm:ss.fffffff")));

            List<FilterFileItem> filterItems = VerifyFilterPatterns(filterFileItems);

            try
            {
                foreach (LogFileItem item in logFileItems)
                {
                    if (item.FilterIndex < 0)
                    {
                        item.Background = Settings.BackgroundColor;
                        item.Foreground = Settings.ForegroundColor;
                    }
                    else
                    {
                        FilterFileItem filterItem = filterItems.FirstOrDefault(x => x.Index == item.FilterIndex);
                        item.Foreground = filterItem.Foreground;
                        item.Background = filterItem.Background;
                    }
                }

                SetStatus(string.Format("ApplyColor:total time in seconds: {0}", DateTime.Now.Subtract(timer).TotalSeconds));

                if (showAll)
                {
                    return logFileItems;
                }
                else
                {
                    return new ObservableCollection<LogFileItem>(logFileItems.Where(x => x.FilterIndex > -2));
                }
            }
            catch (Exception e)
            {
                SetStatus("ApplyColor:exception" + e.ToString());

                return new ObservableCollection<LogFileItem>();
            }
        }

        public ObservableCollection<LogFileItem> ApplyFilter(LogTabViewModel logTab, LogFile logFile, List<FilterFileItem> filterFileItems, FilterCommand filterCommand)
        {
            //moved to parser
            throw new NotImplementedException();
            return new ObservableCollection<LogFileItem>();

            Mouse.OverrideCursor = Cursors.Wait;
            DateTime timer = DateTime.Now;
            SetStatus(string.Format("ApplyFilter:start time: {0} log file: {1} ", timer.ToString("hh:mm:ss.fffffff"), logFile.Tag));

            List<FilterFileItem> filterItems = VerifyFilterPatterns(filterFileItems, logTab);
            Debug.Print(string.Format("ApplyFilter: filterItems.Count={0}:{1}", Thread.CurrentThread.ManagedThreadId, filterItems.Count));

            // set regex cache size to number of filter items for better performance
            // https: //msdn.microsoft.com/en-us/library/gg578045(v=vs.110).aspx
            Regex.CacheSize = filterItems.Count;

            int inclusionFilterCount = filterItems.Count(x => x.Include == true);

            ParallelOptions po = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            try
            {
                Parallel.ForEach(logFile.ContentItems, po, logItem =>
                {
                    if (string.IsNullOrEmpty(logItem.Content))
                    {
                        Debug.Print(string.Format("ApplyFilter: logItem.Content empty={0}:{1}", Thread.CurrentThread.ManagedThreadId, logItem.Content));
                        // used for goto line as it needs all line items
                        logItem.FilterIndex = int.MinValue;
                        return;
                    }

                    int filterIndex = int.MaxValue; // int.MinValue;
                    int includeFilters = inclusionFilterCount;

                    if (Settings.CountMaskedMatches)
                    {
                        logItem.Masked = new int[filterItems.Count, 1];
                    }

                    // clear out groups
                    logItem.Group1 = string.Empty;
                    logItem.Group2 = string.Empty;
                    logItem.Group3 = string.Empty;
                    logItem.Group4 = string.Empty;

                    bool matchSet = false;

                    for (int fItem = 0; fItem < filterItems.Count; fItem++)
                    {
                        int filterItemIndex = filterFileItems[fItem].Index;
                        bool match = false;
                        FilterFileItem filterItem = filterItems[fItem];
                        Debug.Print(string.Format("ApplyFilter: loop:{0} filterItem.Pattern={1}:{2} logItem.Content:{3}", filterItemIndex,
                            Thread.CurrentThread.ManagedThreadId, filterItem.Filterpattern, logItem.Content));

                        // unnamed and named groups
                        //if (logTab.GroupCount > 0 && filterItem.Regex)
                        if (filterItem.GroupCount > 0 && filterItem.Regex)
                        {
                            MatchCollection mc = Regex.Matches(logItem.Content, filterItem.Filterpattern, RegexOptions.Singleline | (filterItem.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
                            if (mc.Count > 0)
                            {
                                match = true;

                                foreach (Match m in mc)
                                {
                                    if (!string.IsNullOrEmpty(m.Groups[1].Value.ToString()))
                                    {
                                        logItem.Group1 += (string.IsNullOrEmpty(logItem.Group1) ? "" : ";\n") + m.Groups[1].Value.ToString();
                                    }

                                    if (!string.IsNullOrEmpty(m.Groups[2].Value.ToString()))
                                    {
                                        logItem.Group2 += (string.IsNullOrEmpty(logItem.Group2) ? "" : ";\n") + m.Groups[2].Value.ToString();
                                    }

                                    if (!string.IsNullOrEmpty(m.Groups[3].Value.ToString()))
                                    {
                                        logItem.Group3 += (string.IsNullOrEmpty(logItem.Group3) ? "" : ";\n") + m.Groups[3].Value.ToString();
                                    }

                                    if (!string.IsNullOrEmpty(m.Groups[4].Value.ToString()))
                                    {
                                        logItem.Group4 += (string.IsNullOrEmpty(logItem.Group4) ? "" : ";\n") + m.Groups[4].Value.ToString();
                                    }
                                }
                            }
                        }
                        else if (filterItem.Regex && Regex.IsMatch(logItem.Content, filterItem.Filterpattern, RegexOptions.Singleline | (filterItem.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)))
                        {
                            match = true;
                        }
                        else if (!filterItem.Regex)
                        {
                            bool andMatch = true;
                            if (filterItem.StringOperators)
                            {
                                Debug.Print(string.Format("ApplyFilter: loop:{0} string with operators "
                                   + "thread id:{1} filter index:{2} filter string: {3}",
                                   filterItemIndex,
                                   Thread.CurrentThread.ManagedThreadId,
                                   filterIndex,
                                   filterItem.Filterpattern));
                                // check for ' AND ' and ' OR ' operators
                                foreach (string andPattern in Regex.Split(filterItem.Filterpattern, " AND "))
                                {
                                    Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern "
                                        + "thread id:{1} filter index:{2} filter string: {3}",
                                        filterItemIndex,
                                        Thread.CurrentThread.ManagedThreadId,
                                        filterIndex,
                                        andPattern));

                                    match = false;
                                    string[] ors = Regex.Split(andPattern, " OR ");
                                    if (ors.Length > 1)
                                    {
                                        foreach (string orPattern in ors)
                                        {
                                            Debug.Print(string.Format("ApplyFilter: loop:{0} string orPattern "
                                                + "thread id:{1} filter index:{2} filter string: {3}",
                                                filterItemIndex,
                                                Thread.CurrentThread.ManagedThreadId,
                                                filterIndex,
                                                orPattern));
                                            // only match one
                                            if (filterItem.CaseSensitive && logItem.Content.Contains(orPattern))
                                            {
                                                match = true;
                                                Debug.Print(string.Format("ApplyFilter: loop:{0} string orPattern match "
                                                    + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                                    filterItemIndex,
                                                    Thread.CurrentThread.ManagedThreadId,
                                                    filterIndex,
                                                    orPattern,
                                                    logItem.Content));
                                                break;
                                            }
                                            else if (logItem.Content.ToLower().Contains(orPattern.ToLower()))
                                            {
                                                match = true;
                                                Debug.Print(string.Format("ApplyFilter: loop:{0} string orPattern match "
                                                    + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                                    filterItemIndex,
                                                    Thread.CurrentThread.ManagedThreadId,
                                                    filterIndex,
                                                    orPattern,
                                                    logItem.Content));
                                                break;
                                            }
                                            else
                                            {
                                                Debug.Print(string.Format("ApplyFilter: loop:{0} string orPattern NO match "
                                                    + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                                    filterItemIndex,
                                                    Thread.CurrentThread.ManagedThreadId,
                                                    filterIndex,
                                                    orPattern,
                                                    logItem.Content));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // match all
                                        if (filterItem.CaseSensitive && logItem.Content.Contains(andPattern))
                                        {
                                            match = true;
                                            Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern all match "
                                                + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                                filterItemIndex,
                                                Thread.CurrentThread.ManagedThreadId,
                                                filterIndex,
                                                andPattern,
                                                logItem.Content));
                                        }
                                        else if (logItem.Content.ToLower().Contains(andPattern.ToLower()))
                                        {
                                            match = true;
                                            Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern all match "
                                                + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                                filterItemIndex,
                                                Thread.CurrentThread.ManagedThreadId,
                                                filterIndex,
                                                andPattern,
                                                logItem.Content));
                                        }
                                        else
                                        {
                                            Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern all NO match "
                                                + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                                filterItemIndex,
                                                Thread.CurrentThread.ManagedThreadId,
                                                filterIndex,
                                                andPattern,
                                                logItem.Content));
                                        }
                                    }

                                    andMatch &= match;
                                }

                                match = andMatch;
                            }
                            else
                            {
                                // normal string match
                                if (filterItem.CaseSensitive && logItem.Content.Contains(filterItem.Filterpattern))
                                {
                                    match = true;
                                    Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern one match "
                                        + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                        filterItemIndex,
                                        Thread.CurrentThread.ManagedThreadId,
                                        filterIndex,
                                        filterItem.Filterpattern,
                                        logItem.Content));
                                }
                                else if (logItem.Content.ToLower().Contains(filterItem.Filterpattern.ToLower()))
                                {
                                    match = true;
                                    Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern one match "
                                        + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                        filterItemIndex,
                                        Thread.CurrentThread.ManagedThreadId,
                                        filterIndex,
                                        filterItem.Filterpattern,
                                        logItem.Content));
                                }
                                else
                                {
                                    Debug.Print(string.Format("ApplyFilter: loop:{0} string andPattern one NO match "
                                        + "thread id:{1} filter index:{2} filter string: {3} logItem content: {4}",
                                        filterItemIndex,
                                        Thread.CurrentThread.ManagedThreadId,
                                        filterIndex,
                                        filterItem.Filterpattern,
                                        logItem.Content));
                                }
                            }
                        }

                        Debug.Print(string.Format("ApplyFilter:** loop:{0} filterItem Match={1}:{2} **", filterItemIndex, Thread.CurrentThread.ManagedThreadId, match));

                        if (!matchSet)
                        {
                            if (match && filterItem.Include && !filterItem.Exclude)
                            {
                                filterIndex = filterItemIndex;
                                Debug.Print(string.Format("ApplyFilter: loop:{0} filterItem.Include not exclude setting filterIndex={1}:{2}", filterItemIndex,
                                    Thread.CurrentThread.ManagedThreadId, filterIndex));
                                matchSet = true;
                                includeFilters--;
                                // break;
                            }
                            else if (!match && filterItem.Include && filterItem.Exclude)
                            {
                                // dynamic filter with and quickfindand but no match so exit
                                filterIndex = int.MinValue;
                                Debug.Print(string.Format("ApplyFilter: loop:{0} no match filterItem.Include and exclude setting filterIndex={1}:{2}", filterItemIndex,
                                    Thread.CurrentThread.ManagedThreadId, filterIndex));
                                matchSet = true;
                                includeFilters = 0;
                                // break;
                            }
                            else if (match && filterItem.Include && filterItem.Exclude)
                            {
                                // dynamic filter with and quickfindand but with match
                                filterIndex = int.MinValue;
                                Debug.Print(string.Format("ApplyFilter: loop:{0} match filterItem.Include and exlude setting filterIndex={1}:{2}", filterItemIndex,
                                    Thread.CurrentThread.ManagedThreadId, filterIndex));
                                matchSet = false;
                                includeFilters = 0;
                                // break;
                            }
                            else if (match && filterItem.Exclude)
                            {
                                filterIndex = (fItem * -1) - 2;
                                Debug.Print(string.Format("ApplyFilter: loop:{0} filterItem.Exclude and match filterIndex={1}:{2}", filterItemIndex,
                                    Thread.CurrentThread.ManagedThreadId, filterIndex));

                                matchSet = true;
                                // break;
                            }
                            else if (!match && !filterItem.Exclude)
                            {
                                filterIndex = int.MinValue;
                                Debug.Print(string.Format("ApplyFilter: loop:{0} not filterItem.Exclude and not match filterIndex={1}:{2}",
                                    filterItemIndex, Thread.CurrentThread.ManagedThreadId, filterIndex));
                            }
                            else if (match)
                            {
                                filterIndex = filterItemIndex;
                                Debug.Print(string.Format("ApplyFilter: loop:{0} setting filterIndex={1}:{2}", filterItemIndex,
                                    Thread.CurrentThread.ManagedThreadId, filterIndex));
                                matchSet = true;
                                // break;
                            }
                            else if (filterItem.Include)
                            {
                                includeFilters--;
                            }
                        }
                        else if (matchSet && match && Settings.CountMaskedMatches)
                        {
                            logItem.Masked[fItem, 0] = 1;
                            Debug.Print(string.Format("ApplyFilter: loop:{0} masked match filterIndex={1}:{2}", filterItemIndex,
                                 Thread.CurrentThread.ManagedThreadId, filterItemIndex));
                        }

                        if (matchSet && !Settings.CountMaskedMatches)
                        {
                            Debug.Print(string.Format("ApplyFilter: loop:{0} not filterItem.Exclude CountMaskedMatches={1}:{2}", filterItemIndex,
                                Thread.CurrentThread.ManagedThreadId, Settings.CountMaskedMatches));

                            if (includeFilters == 0)
                            {
                                break;
                            }
                        }
                    }

                    Debug.Print(string.Format("ApplyFilter: loop finished set filterIndex={0}:{1}", Thread.CurrentThread.ManagedThreadId, filterIndex));
                    logItem.FilterIndex = filterIndex;
                });

                // write totals negative indexes arent displayed and are only used for counting
                int filterCount = 0;
                for (int i = 0; i < filterFileItems.Count; i++)
                {
                    int filterItemIndex = filterFileItems[i].Index;
                    filterFileItems[i].Count = logFile.ContentItems.Count(x => x.FilterIndex == filterItemIndex | x.FilterIndex == (i * -1) - 2);

                    if (Settings.CountMaskedMatches)
                    {
                        filterFileItems[i].MaskedCount = logFile.ContentItems.Count(x => (x.FilterIndex != int.MaxValue) & (x.FilterIndex != int.MinValue) && x.Masked[i, 0] == 1);
                        SetStatus(string.Format("ApplyFilter:filterItem masked counttotal: {0}", filterFileItems[i].MaskedCount));
                    }

                    SetStatus(string.Format("ApplyFilter:filterItem counttotal: {0}", filterFileItems[i].Count));

                    filterCount += filterFileItems[i].Count;
                }

                double totalSeconds = DateTime.Now.Subtract(timer).TotalSeconds;
                SetStatus(string.Format("ApplyFilter:total time in seconds: {0}\n\tlines per second: {1} "
                    + "\n\tlogfile total lines count: {2}\n\tlogfile filter lines count: {3}\n\tlog file: {4}"
                    + "\n\tnumber of filters: {5}\n\tcalculated lines per second for single query: {6}",
                    totalSeconds,
                    logFile.ContentItems.Count / totalSeconds,
                    logFile.ContentItems.Count,
                    filterCount,
                    logFile.Tag,
                    filterItems.Count,
                    logFile.ContentItems.Count / totalSeconds * filterItems.Count));

                Mouse.OverrideCursor = null;
                return new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex > -2));
            }
            catch (Exception e)
            {
                SetStatus("ApplyFilter:exception" + e.ToString());
                Mouse.OverrideCursor = null;
                return new ObservableCollection<LogFileItem>();
            }
        }

        public override IFile<LogFileItem> ManageFileProperties(string LogName, IFile<LogFileItem> logFile)
        {
            // filename is used to name tab
            logFile.FileName = Path.GetFileName(LogName);

            // tag is used to keep complete file name and path
            logFile.Tag = LogName;

            return logFile;
        }

        public override IFile<LogFileItem> NewFile(string fileName, ObservableCollection<LogFileItem> logFileItems = null)
        {
            SetStatus("NewFile:enter: " + fileName);
            LogFile logFile = new LogFile();
            if (logFileItems != null)
            {
                logFile.ContentItems = logFileItems;
            }

            FileManager.Add(ManageFileProperties(fileName, logFile));
            logFile.Modified = true;
            Settings.AddLogFile(fileName);
            OnPropertyChanged("LogFileManager");
            SetStatus("NewFile:exit: " + fileName);
            return logFile;
        }

        public override IFile<LogFileItem> OpenFile(string fileName)
        {
            IFile<LogFileItem> logFile = new LogFile();

            try
            {
                SetStatus("OpenFile:enter: " + fileName);

                if (FileManager.Exists(x => String.Compare(x.Tag, fileName, true) == 0))
                {
                    SetStatus("file already open:" + fileName);
                    return logFile;
                }

                if (File.Exists(fileName))
                {
                    logFile.FileName = Path.GetFileName(fileName);
                    logFile.Tag = fileName;
                    // moved to parser
                    //logFile.ContentItems = ((LogFile)ReadFile(fileName)).ContentItems;
                    FileManager.Add(logFile);
                    Settings.AddLogFile(fileName);
                }
                else
                {
                    SetStatus("log file does not exist:" + fileName);
                    Settings.RemoveLogFile(fileName);
                }

                SetStatus("OpenFile:exit: " + fileName);
                return logFile;
            }
            catch (Exception e)
            {
                SetStatus("OpenFile:exception: " + e.ToString());
                return logFile;
            }
        }

        public override List<IFile<LogFileItem>> OpenFiles(string[] files)
        {
            List<IFile<LogFileItem>> logFileItems = new List<IFile<LogFileItem>>();

            foreach (string file in files)
            {
                LogFile logFile = new LogFile();
                if (String.IsNullOrEmpty((logFile = (LogFile)OpenFile(file)).Tag))
                {
                    continue;
                }

                logFileItems.Add(logFile);
            }

            return logFileItems;
        }

        public override IFile<LogFileItem> ReadFile(string fileName)
        {
            // BOM UTF - 8 0xEF,0xBB,0xBF BOM UTF - 16 FE FF NO BOM assume ansi but utf-8 doesnt have
            // to have one either

            LogFile logFile = new LogFile();

            try
            {
                SetStatus("ReadFile:enter: " + fileName);
                Encoding encoding = Encoding.Default;

                logFile.FileName = Path.GetFileName(fileName);
                // find bom
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName, true))
                {
                    encoding = sr.CurrentEncoding;

                    SetStatus("current encoding:" + encoding.EncodingName);

                    while (!sr.EndOfStream)
                    {
                        // if bom not supplied, try to determine utf-16 (unicode)
                        string line = sr.ReadLine();
                        byte[] bytes = Encoding.UTF8.GetBytes(line);
                        string newLine = Encoding.UTF8.GetString(bytes).Replace("\0", "");
                        SetStatus(string.Format("check encoding: bytes:{0} string: {1}", bytes.Length, newLine.Length));

                        if (bytes.Length > 0 && newLine.Length > 0
                            && ((bytes.Length - newLine.Length) * 2 - 1 == bytes.Length
                                | (bytes.Length - newLine.Length) * 2 == bytes.Length))
                        {
                            SetStatus(string.Format("new encoding:Unicode bytes:{0} string: {1}", bytes.Length, newLine.Length));

                            encoding = Encoding.Unicode;
                            break;
                        }
                        else if (bytes.Length > 0 && newLine.Length > 0)
                        {
                            break;
                        }
                    }
                }

                // todo: use mapped file only for large files?

                string[] lines = File.ReadAllLines(fileName, encoding);
                ConcurrentBag<LogFileItem> cLogFileItems = new ConcurrentBag<LogFileItem>();

                Parallel.For(0, lines.Length, x =>
                {
                    LogFileItem logFileItem = new LogFileItem()
                    {
                        Content = lines[x],
                        Background = Settings.BackgroundColor,
                        Foreground = Settings.ForegroundColor,
                        Index = x + 1
                    };

                    cLogFileItems.Add(logFileItem);
                });

                logFile.ContentItems = new ObservableCollection<LogFileItem>(cLogFileItems.OrderBy(x => x.Index));
                SetStatus("ReadFile:exit: " + fileName);
                logFile.IsNew = false;
                return logFile;
            }
            catch (Exception e)
            {
                SetStatus("Fatal:ReadFile:exception: " + e.ToString());
                return logFile;
            }
        }

        public override bool SaveFile(string fileName, IFile<LogFileItem> file)
        {
            try
            {
                SetStatus("SaveFile:enter: " + fileName);
                file.IsNew = false;
                LogFile logFile = (LogFile)file;

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (StreamWriter writer = File.CreateText(fileName))
                {
                    LogFile.ExportConfigurationInfo config = logFile.ExportConfiguration;

                    foreach (LogFileItem item in logFile.ContentItems)
                    {
                        StringBuilder sb = new StringBuilder();

                        sb = FormatExportItem(config.Index, config.Separator, config.RemoveEmpty, item.Index.ToString(), sb);
                        sb = FormatExportItem(config.Content, config.Separator, config.RemoveEmpty, item.Content, sb);
                        sb = FormatExportItem(config.Group1, config.Separator, config.RemoveEmpty, item.Group1, sb);
                        sb = FormatExportItem(config.Group2, config.Separator, config.RemoveEmpty, item.Group2, sb);
                        sb = FormatExportItem(config.Group3, config.Separator, config.RemoveEmpty, item.Group3, sb);
                        sb = FormatExportItem(config.Group4, config.Separator, config.RemoveEmpty, item.Group4, sb);

                        writer.WriteLine(sb.ToString());
                    }

                    writer.Close();
                }

                SetStatus("SaveFile:exit: " + fileName);
                return true;
            }
            catch (Exception e)
            {
                SetStatus("Fatal:SaveFile:exception: " + e.ToString());
                return false;
            }
        }
        private List<FilterFileItem> VerifyFilterPatterns(List<FilterFileItem> filterFileItems, LogTabViewModel logTab = null)
        {
            bool getGroups = false;
            int groupCount = 0;
            List<string> groupNames = new List<string>();

            if (logTab != null)
            {
                getGroups = true;
            }

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
                    Include = filterItem.Include,
                    Regex = filterItem.Regex,
                    Index = filterItem.Index
                };

                if (newFilter.Regex)
                {
                    try
                    {
                        Regex test = new Regex(filterItem.Filterpattern);
                        if (getGroups)
                        {
                            // unnamed groups
                            newFilter.GroupCount = test.GetGroupNumbers().Length - 1;
                            groupCount = Math.Max(groupCount, newFilter.GroupCount);
                        }
                    }
                    catch
                    {
                        SetStatus("not a regex:" + filterItem.Filterpattern);
                        newFilter.Regex = false;
                        newFilter.Filterpattern = Regex.Escape(filterItem.Filterpattern);
                    }
                }
                else
                {
                    // check for string operators and flag
                    if (newFilter.Filterpattern.Contains(" AND ") | newFilter.Filterpattern.Contains(" OR "))
                    {
                        newFilter.StringOperators = true;
                    }
                }

                filterItems.Add(newFilter);
            }

            if (getGroups)
            {
                logTab.SetGroupCount(Math.Max(groupCount, groupNames.Count));
            }

            return filterItems;
        }

        #endregion Methods

        #region Classes

        public class TaskMMFInfo
        {

            #region Fields

            public BackgroundWorker bgWorker;

            public Int32 length;

            public LogFile logFile;

            public MemoryMappedFile mmf;

            public Int32 position;

            public List<LogFileItem> stringList;

            #endregion Fields

            #region Properties

            public ManualResetEvent completedEvent { get; set; }

            #endregion Properties

            //public string fileName { get; set; }
        }

        #endregion Classes

    }
}