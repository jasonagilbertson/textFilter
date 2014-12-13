using System.IO.MemoryMappedFiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media;

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
        List<FilterFileItem> _previousFilterFileItems = new List<FilterFileItem>();
        #region Public Methods

        public ObservableCollection<LogFileItem> ApplyFilter(ObservableCollection<LogFileItem> logFileItems, List<FilterFileItem> filterFileItems)
        {
            

            ObservableCollection<LogFileItem> filteredItems = new ObservableCollection<LogFileItem>();
            
            //SetStatus("ApplyFilter:" + filterFile.Tag);


            

            int[] countTotals = new int[filterFileItems.Count];
            
            try
            {
                foreach (LogFileItem logItem in logFileItems)
                {
                    if (string.IsNullOrEmpty(logItem.Text))
                    {
                        continue;
                    }
                        
                    //Debug.Print(logItem.Text);

                    for (int c = 0; c < filterFileItems.Count; c++)
                    {
                        FilterFileItem fileItem = filterFileItems[c];

                        if(!Regex.IsMatch(logItem.Text,fileItem.Filterpattern,RegexOptions.IgnoreCase))
                        {
                            continue;
                        }

                        if(fileItem.Exclude)
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

                if (!fileItem.Regex)
                {
                    fileItems[fileItems.Count - 1].Filterpattern = Regex.Escape(fileItem.Filterpattern);
                }
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
            foreach(FilterFileItem item in filterFileItems)
            {
                _previousFilterFileItems.Add((FilterFileItem)item.ShallowCopy());
            }
            
            Debug.Print("CompareFilterList:returning:" + retval.ToString());
            return retval;
        }
        public ObservableCollection<LogFileItem> ApplyFilterMappedFile(string logFile, FilterFile filterFile)
        {

            // http://blogs.msdn.com/b/salvapatuel/archive/2009/06/08/working-with-memory-mapped-files-in-net-4.aspx
            ObservableCollection<LogFileItem> filteredItems = new ObservableCollection<LogFileItem>();

            SetStatus("ApplyFilter:" + filterFile.Tag);
            List<FilterFileItem> fileItems = new List<FilterFileItem>();

            // clean up list
            foreach (FilterFileItem fileItem in filterFile.ContentItems.OrderBy(x => x.Index))
            {
                if (!fileItem.Enabled || string.IsNullOrEmpty(fileItem.Filterpattern))
                {
                    continue;
                }

                fileItems.Add(fileItem);

                if (!fileItem.Regex)
                {
                    fileItems[fileItems.Count - 1].Filterpattern = Regex.Escape(fileItem.Filterpattern);
                }
            }

            List<LogFileItem> logFileItems = new List<LogFileItem>();


            try
            {
                foreach (LogFileItem logItem in logFileItems)
                {
                    if (string.IsNullOrEmpty(logItem.Text))
                    {
                        continue;
                    }

                    //Debug.Print(logItem.Text);

                    for (int c = 0; c < fileItems.Count; c++)
                    {
                        FilterFileItem fileItem = fileItems[c];

                        if (!Regex.IsMatch(logItem.Text, fileItem.Filterpattern, RegexOptions.IgnoreCase))
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

                        filteredItems.Add(item);
                        break;
                    }
                }

                return filteredItems;

            }
            catch (Exception e)
            {
                SetStatus("ApplyFilter:exception" + e.ToString());
                return filteredItems;
            }
        }
        public override IFile<LogFileItem> OpenFile(string LogName)
        {
            IFile<LogFileItem> logFileItems = new LogFile();
            if (FileManager.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                
                SetStatus("file already open:" + LogName);
                return logFileItems;
            }

            if (File.Exists(LogName))
            {
                logFileItems.FileName = Path.GetFileName(LogName);
                logFileItems.Tag = LogName;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(LogName))
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
                        logFileItems.ContentItems.Add(logFileItem);
                    }
                }

                FileManager.Add(logFileItems);
                this.Settings.AddLogFile(LogName);
            }
            else
            {
                // ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                SetStatus("log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return logFileItems;
        }

        public override List<IFile<LogFileItem>> OpenFiles(string[] files)
        {
            List<IFile<LogFileItem>> textBlockItems = new List<IFile<LogFileItem>>();

            foreach (string file in files)
            {
                LogFile logProperties = new LogFile();
                if (String.IsNullOrEmpty((logProperties = (LogFile)OpenFile(file)).Tag))
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