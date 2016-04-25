// *********************************************************************** Assembly : textFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-31-2015 ***********************************************************************
// <copyright file="WorkerFunctions.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
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

namespace TextFilter
{
    public class WorkerFunctions : Base
    {

        #region Fields

        private static string _needsPatch = "textFilter*NeEdSpAtCh*";

        private TextFilterSettings Settings = TextFilterSettings.Settings;

        #endregion Fields

        #region Methods

        public WorkerItem MMFConcurrentFilter(WorkerItem workerItem)
        {

            try
            {

                workerItem.Status.AppendLine("MMFConcurrentFilter: enter");
                //List<FilterFileItem> filterItems = VerifyFilterPatterns(workerItem).VerifiedFilterItems;
                List<FilterFileItem> filterItems = workerItem.VerifiedFilterItems;

                if (workerItem.LogFile == null || workerItem.LogFile.ContentItems.Count == 0)
                {
                    workerItem.Status.AppendLine("MMFConcurrentFilter: logfile null. returning");
                    return workerItem;
                }
                if (workerItem.FilterFile == null || workerItem.FilterFile.ContentItems.Count == 0)
                {
                    workerItem.Status.AppendLine("MMFConcurrentFilter: filterfile null. returning");
                    return workerItem;
                }

                workerItem.Status.AppendLine(string.Format("ApplyFilter: filterItems.Count={0}:{1}", Thread.CurrentThread.ManagedThreadId, filterItems.Count));
                DateTime timer = DateTime.Now;
                Debug.Print(string.Format("ApplyFilter:start time: {0} log file: {1} ", timer.ToString("hh:mm:ss.fffffff"), workerItem.LogFile.Tag));
                LogFile logFile = workerItem.LogFile;

                // set regex cache size to number of filter items for better performance
                // https: //msdn.microsoft.com/en-us/library/gg578045(v=vs.110).aspx
                Regex.CacheSize = filterItems.Count;
                Nullable<long> lowest = new Nullable<long>();


                int inclusionFilterCount = filterItems.Count(x => x.Include == true);
                ParallelLoopResult loopResult = Parallel.ForEach(workerItem.LogFile.ContentItems, (logItem, state) =>
                {
                    if (state.ShouldExitCurrentIteration)
                    {
                        if (state.LowestBreakIteration.HasValue)
                        {
                            return;
                        }
                    }

                    if (workerItem.BackGroundWorker.CancellationPending)
                    {
                        workerItem.Status.AppendLine("MMFConcurrentFilter:cancelled");
                        state.Break();
                        if (state.LowestBreakIteration.HasValue)
                        {
                            if (lowest < state.LowestBreakIteration)
                            {
                                lowest = state.LowestBreakIteration;
                            }
                        }
                        else
                        {
                            lowest = state.LowestBreakIteration;
                        }

                    }

                    if (string.IsNullOrEmpty(logItem.Content))
                    {
                        workerItem.Status.AppendLine(string.Format("ApplyFilter: logItem.Content empty={0}:{1}", Thread.CurrentThread.ManagedThreadId, logItem.Content));
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
                        int filterItemIndex = filterItems[fItem].Index;
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

                // check for cancellation
                if(!loopResult.IsCompleted)
                {
                    Debug.Print("ApplyFilter: cancelled. returning");
                    return workerItem;
                }

                // write totals negative indexes arent displayed and are only used for counting
                int filterCount = 0;
                for (int i = 0; i < filterItems.Count; i++)
                {
                    int filterItemIndex = filterItems[i].Index;
                    filterItems[i].Count = logFile.ContentItems.Count(x => x.FilterIndex == filterItemIndex | x.FilterIndex == (i * -1) - 2);
                    workerItem.FilterFile.ContentItems.First(x => x.Index == filterItemIndex).Count = filterItems[i].Count;

                    if (Settings.CountMaskedMatches)
                    {
                        filterItems[i].MaskedCount = logFile.ContentItems.Count(x => (x.FilterIndex != int.MaxValue) & (x.FilterIndex != int.MinValue) && x.Masked[i, 0] == 1);
                        workerItem.FilterFile.ContentItems.First(x => x.Index == filterItemIndex).MaskedCount = filterItems[i].MaskedCount;

                        Debug.Print(string.Format("ApplyFilter:filterItem masked counttotal: {0}", filterItems[i].MaskedCount));
                    }

                    Debug.Print(string.Format("ApplyFilter:filterItem counttotal: {0}", filterItems[i].Count));

                    filterCount += filterItems[i].Count;
                }

                double totalSeconds = DateTime.Now.Subtract(timer).TotalSeconds;
                workerItem.Status.AppendLine(string.Format("ApplyFilter:total time in seconds: {0} lines per second: {1} "
                    + "logfile total count: {2} logfile filter count: {3} log file: {4}",
                    totalSeconds,
                    logFile.ContentItems.Count / totalSeconds,
                    logFile.ContentItems.Count,
                    filterCount,
                    logFile.Tag));

                workerItem.FilteredList = new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex > -2));
                return workerItem;
            }
            catch (Exception e)
            {
                //Debug.Print("ApplyFilter:exception" + e.ToString());
                workerItem.Status.AppendLine("ApplyFilter:exception" + e.ToString());

                return workerItem;
            }
        }
        public WorkerItem MMFConcurrentRead(WorkerItem workerItem)
        {
            workerItem.Status.AppendLine("MMFConcurrentRead: enter");
            LogFile logFile = workerItem.LogFile;
            GetEncoding(logFile);

            if (!File.Exists(logFile.Tag))
            {
                workerItem.Status.AppendLine("MMFConcurrentRead:error, file does not exist: " + logFile.Tag);
                workerItem.WorkerState = WorkerItem.State.Aborted;
                return workerItem;
            }

            byte[] bytes = new byte[new FileInfo(logFile.Tag).Length];
            // 4,8,12
            int threadCount = 1;
            if (bytes.Length > 1000000)
            {
                threadCount = 8;
            }

            int bposition = logFile.HasBom ? logFile.Encoding.GetPreamble().Length : 0;
            int blen = bytes.Length / threadCount;

            MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(logFile.Tag, FileMode.Open, "mmf", bytes.Length, MemoryMappedFileAccess.Read);

            ManualResetEvent[] completedEvents = new ManualResetEvent[threadCount];
            List<List<LogFileItem>> list = new List<List<LogFileItem>>();

            for (int i = 0; i < threadCount; i++)
            {
                list.Add(new List<LogFileItem>());
                completedEvents[i] = new ManualResetEvent(false);
            }

            for (int mmfCount = 0; mmfCount < threadCount; mmfCount++)
            {
                if (mmfCount != 0)
                {
                    bposition += blen;
                }

                if (mmfCount == threadCount - 1)
                {
                    blen = bytes.Length - bposition;
                }

                TaskMMFInfo taskInfo = new TaskMMFInfo()
                {
                    mmf = memoryMappedFile,
                    stringList = list[mmfCount],
                    logFile = logFile,
                    position = bposition,
                    length = blen,
                    completedEvent = completedEvents[mmfCount],
                    bgWorker = workerItem.BackGroundWorker
                };

                ThreadPool.QueueUserWorkItem(new WaitCallback(ParallelMMFRead), taskInfo);
            }

            workerItem.Status.AppendLine(string.Format("mmf thread length: {0}", blen));

            // watch for cancellation while waiting
            while (!workerItem.BackGroundWorker.CancellationPending)
            {
                if (WaitHandle.WaitAll(completedEvents, 10))
                {
                    break;
                }

            }

            if (memoryMappedFile != null)
            {
                memoryMappedFile.Dispose();
            }

            if (workerItem.BackGroundWorker.CancellationPending)
            {
                workerItem.Status.AppendLine("MMFConcurrentRead:cancelled");
                return workerItem;
            }

            // todo remove when switching to offset
            bool patch = false;

            List<LogFileItem> finalList = new List<LogFileItem>();
            for (int listCount = 0; listCount < threadCount; listCount++)
            {
                if (patch)
                {
                    // merge last line with first line in current list
                    finalList[finalList.Count - 1].Content = finalList[finalList.Count - 1].Content + list[listCount][0].Content;
                    list[listCount].RemoveAt(0);
                    patch = false;
                }

                finalList.AddRange(list[listCount]);
                list[listCount].Clear();

                // need to check for partial lines look in last index for needsPatch string
                if (finalList.Count > 0)
                {
                    if (finalList[finalList.Count - 1].Content == _needsPatch)
                    {
                        finalList.RemoveAt(finalList.Count - 1);
                        patch = true;
                    }
                    else
                    {
                        patch = false;
                    }
                }
            }

            // set index
            int counter = 1;
            foreach (LogFileItem item in finalList)
            {
                item.Index = counter++;
            }

            workerItem.Status.AppendLine("MMFConcurrentRead:exit");

            logFile.ContentItems = new ObservableCollection<LogFileItem>(finalList);

            return workerItem;
        }

        public void ParallelMMFRead(object taskMMFInfo)
        {
            try
            {
                TaskMMFInfo taskInfo = (TaskMMFInfo)taskMMFInfo;
                Debug.Print(string.Format("ParallelMMFRead:enter : position:{0} length:{1} total:{2}",
                    taskInfo.position, taskInfo.length, taskInfo.length + taskInfo.position));

                MemoryMappedViewStream viewStream = taskInfo.mmf.CreateViewStream(taskInfo.position, taskInfo.length, MemoryMappedFileAccess.Read);
                byte[] bytes = new byte[taskInfo.length];
                viewStream.Read(bytes, 0, taskInfo.length);

                byte[] newLine = (taskInfo.logFile.Encoding).GetBytes(Environment.NewLine);
                int beginningIndex = 0;
                int indexCount = 0;
                int fixUp = 0;
                int x = 0;
                int step = Math.Max(1, newLine.Length / 2);

                // check first two bytes to make sure not part of crlf
                if (bytes[0] == newLine[0] || bytes[0] == newLine[step])
                {
                    fixUp = step;

                    if (bytes[step] == newLine[step])
                    {
                        fixUp = step * 2;
                    }

                    taskInfo.stringList.Add(new LogFileItem());
                }

                for (x = fixUp; x < bytes.Length; x += step)
                {

                    if (bytes[x] == newLine[0])
                    {
                        if (taskInfo.bgWorker.CancellationPending)
                        {
                            taskInfo.completedEvent.Set();
                            Debug.Print("ParallelMMFRead:cancelled");
                            return;
                        }

                        LogFileItem logFileItem = new LogFileItem()
                        {
                            Content = (taskInfo.logFile.Encoding).GetString(bytes, beginningIndex, indexCount),
                            Background = Settings.BackgroundColor,
                            Foreground = Settings.ForegroundColor,
                        };

                        taskInfo.stringList.Add(logFileItem);

                        if (x + step <= bytes.Length && bytes[x + step] == newLine[step])
                        {
                            x += step;
                        }

                        beginningIndex = x + step;

                        indexCount = 0;
                    }
                    else
                    {
                        indexCount += step;
                    }
                }

                if (indexCount > 1)
                {
                    // partial string
                    LogFileItem logFileItem = new LogFileItem()
                    {
                        Content = (taskInfo.logFile.Encoding).GetString(bytes, beginningIndex, bytes.Length - beginningIndex),
                        Background = Settings.BackgroundColor,
                        Foreground = Settings.ForegroundColor,
                        // Index = x
                    };

                    taskInfo.stringList.Add(logFileItem);

                    logFileItem = new LogFileItem()
                    {
                        Content = _needsPatch,
                        Background = Settings.BackgroundColor,
                        Foreground = Settings.ForegroundColor,
                        // Index = x
                    };

                    taskInfo.stringList.Add(logFileItem);
                }

                taskInfo.completedEvent.Set();
                Debug.Print("ParallelMMFRead:exit");
            }
            catch (Exception e)
            {
                Debug.Print("ParallelMMFRead:exception" + e.ToString());
                return;
            }
        }

        public WorkerItem VerifyFilterPatterns(WorkerItem workerItem)
        {

            int groupCount = 0;
            List<string> groupNames = new List<string>();
            if (workerItem.FilterFile == null)
            {
                SetStatus("VerifyFilterPattern:FilterFile null. returning");
                return workerItem;
            }

            List<FilterFileItem> filterFileItems = workerItem.FilterFile.ContentItems.ToList();
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
                        // unnamed groups
                        newFilter.GroupCount = test.GetGroupNumbers().Length - 1;
                        groupCount = Math.Max(groupCount, newFilter.GroupCount);
                    }
                    catch
                    {
                        Debug.Print("not a regex:" + filterItem.Filterpattern);
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

            workerItem.FilterGroupCount = Math.Max(groupCount, groupNames.Count);
            //logTab.SetGroupCount(Math.Max(groupCount, groupNames.Count));
            workerItem.VerifiedFilterItems = filterItems;
            return workerItem;
        }

        private bool GetEncoding(LogFile logFile)
        {
            logFile.HasBom = false;
            logFile.Encoding = Encoding.GetEncoding("ISO-8859-1"); // extended ascii

            try
            {
                // ascii will be default as it never has bom if all fail utf-7 rarely used and does
                // not have preamble. wiki says 2b,2f,76,variable
                Debug.Print("GetEncoding:enter: " + logFile.Tag);

                Encoding[] encodings = new Encoding[] { Encoding.UTF32, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.Unicode };

                //logFile.FileName = Path.GetFileName(fileName);
                // find bom
                bool foundEncoding = false;

                using (System.IO.StreamReader sr = new System.IO.StreamReader(logFile.Tag, true))
                {
                    // encoding = sr.CurrentEncoding;

                    Debug.Print("current encoding:" + logFile.Encoding.EncodingName);
                    // biggest preamble is 4 bytes
                    if (sr.BaseStream.Length < 4)
                    {
                        return true;
                    }

                    int[] srBytes = new int[4];
                    for (int c = 0; c < srBytes.Length; c++)
                    {
                        srBytes[c] = sr.BaseStream.ReadByte();
                    }

                    sr.DiscardBufferedData();

                    foreach (Encoding enc in encodings)
                    {
                        byte[] bytes = enc.GetPreamble();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            if (bytes[i] != srBytes[i])
                            {
                                foundEncoding = false;
                                break;
                            }
                            else
                            {
                                foundEncoding = true;
                            }
                        }

                        if (foundEncoding)
                        {
                            logFile.HasBom = true;
                            logFile.Encoding = enc;
                            return true;
                        }
                    }

                    // if bom not supplied, try to determine utf-16 (unicode)

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        byte[] bytes = Encoding.UTF8.GetBytes(line);
                        string newLine = Encoding.UTF8.GetString(bytes).Replace("\0", "");
                        Debug.Print(string.Format("check encoding: bytes:{0} string: {1}", bytes.Length, newLine.Length));

                        if (bytes.Length > 0 && newLine.Length > 0
                            && ((bytes.Length - newLine.Length) * 2 - 1 == bytes.Length
                                | (bytes.Length - newLine.Length) * 2 == bytes.Length))
                        {
                            Debug.Print(string.Format("new encoding:Unicode bytes:{0} string: {1}", bytes.Length, newLine.Length));
                            logFile.Encoding = Encoding.Unicode;

                            Debug.Print("new encoding:" + logFile.Encoding.EncodingName);
                            break;
                        }
                        else if (bytes.Length > 0 && newLine.Length > 0)
                        {
                            break;
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Exception:GetEncoding:" + e.ToString());
                return false;
            }

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

            //public string fileName { get; set; }

            public DoWorkEventArgs doWorkEventArgs { get; set; }

            #endregion Properties

        }

        #endregion Classes

    }
}