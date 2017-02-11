// ************************************************************************************
// Assembly: TextFilter
// File: WorkerFunctions.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;

namespace TextFilter
{
    public class WorkerFunctions : Base
    {
        private static string _needsPatch = "textFilter*NeEdSpAtCh*";

        private TextFilterSettings Settings = TextFilterSettings.Settings;

        public LogFile MMFConcurrentRead(LogFile logFile, BackgroundWorker backgroundWorker)
        {
            Debug.Print("MMFConcurrentRead: enter");
            GetEncoding(logFile);
            // not sure why this here. messing up temp file
            //logFile.IsNew = false;
            //logFile.Modified = false;

            if (!File.Exists(logFile.Tag))
            {
                Debug.Print("MMFConcurrentRead:error, file does not exist: " + logFile.Tag);
                return logFile;
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
                    bgWorker = backgroundWorker
                };

                ThreadPool.QueueUserWorkItem(new WaitCallback(ParallelMMFRead), taskInfo);
            }

            Debug.Print(string.Format("mmf thread length: {0}", blen));

            WaitHandle.WaitAll(completedEvents);

            if (memoryMappedFile != null)
            {
                memoryMappedFile.Dispose();
            }

            if (backgroundWorker.CancellationPending)
            {
                Debug.Print("MMFConcurrentRead:cancelled");
                return logFile;
            }

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

            Debug.Print("MMFConcurrentRead:exit");

            logFile.ContentItems = new ObservableCollection<LogFileItem>(finalList);

            return logFile;
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
                    if (taskInfo.bgWorker.CancellationPending)
                    {
                        taskInfo.completedEvent.Set();
                        Debug.Print("ParallelMMFRead:cancelled");
                        return;
                    }

                    if (bytes[x] == newLine[0])
                    {
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

        private bool GetEncoding(LogFile logFile)
        {
            logFile.HasBom = false;
            logFile.Encoding = Encoding.GetEncoding("ISO-8859-1"); // extended ascii

            try
            {
                // ascii will be default as it never has bom if all fail utf-7 rarely used and does
                // not have preamble. wiki says 2b,2f,76,variable
                SetStatus("GetEncoding:enter: " + logFile.Tag);

                Encoding[] encodings = new Encoding[] { Encoding.UTF32, Encoding.UTF8, Encoding.BigEndianUnicode, Encoding.Unicode };

                //logFile.FileName = Path.GetFileName(fileName);
                // find bom
                bool foundEncoding = false;

                using (System.IO.StreamReader sr = new System.IO.StreamReader(logFile.Tag, true))
                {
                    // encoding = sr.CurrentEncoding;

                    SetStatus("current encoding:" + logFile.Encoding.EncodingName);
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
                        SetStatus(string.Format("check encoding: bytes:{0} string: {1}", bytes.Length, newLine.Length));

                        if (bytes.Length > 0 && newLine.Length > 0
                            && ((bytes.Length - newLine.Length) * 2 - 1 == bytes.Length
                                | (bytes.Length - newLine.Length) * 2 == bytes.Length))
                        {
                            SetStatus(string.Format("new encoding:Unicode bytes:{0} string: {1}", bytes.Length, newLine.Length));
                            logFile.Encoding = Encoding.Unicode;

                            SetStatus("new encoding:" + logFile.Encoding.EncodingName);
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
                SetStatus("Exception:GetEncoding:" + e.ToString());
                return false;
            }
        }

        public class TaskMMFInfo
        {
            public BackgroundWorker bgWorker;

            public Int32 length;

            public LogFile logFile;

            public MemoryMappedFile mmf;

            public Int32 position;

            public List<LogFileItem> stringList;

            public ManualResetEvent completedEvent { get; set; }

            //public string fileName { get; set; }

            public DoWorkEventArgs doWorkEventArgs { get; set; }
        }
    }
}