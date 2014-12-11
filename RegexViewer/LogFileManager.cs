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

        #region Public Methods

        public ObservableCollection<LogFileItem> ApplyFilter(ObservableCollection<LogFileItem> logFileItems, FilterFile filterFile)
        {

            ObservableCollection<LogFileItem> filteredItems = new ObservableCollection<LogFileItem>();


            if(string.IsNullOrEmpty(filterFile.RegexPattern))
            {
                return logFileItems;
            }
            
            SetStatus("ApplyFilter:" + filterFile.RegexPattern);
            Regex regex = new Regex(filterFile.RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline| RegexOptions.Compiled);

            //object[,] filterObject = new object[filterFile.ContentItems.Count,4];
            //int i = 0;
            List<FilterFileItem> fileItems = filterFile.ContentItems.OrderBy(x => x.Index).ToList();
            //foreach(FilterFileItem filterItem in filterFile.ContentItems.OrderBy(x => x.Index))
            //{
            //    filterObject[i, 0] = filterItem.Foreground;
            //    filterObject[i, 1] = filterItem.Background;
            //    filterObject[i, 2] = filterItem.Enabled;
            //    filterObject[i++, 3] = filterItem.Exclude;
            
            //}

                try
                {
                    foreach (LogFileItem logItem in logFileItems)
                    {
                        if (string.IsNullOrEmpty(logItem.Text))
                        {
                            continue;
                        }

                        if (regex.IsMatch(logItem.Text))
                        {
                            // loop though matches starting at index 0
                            Debug.Print(logItem.Text);

                            MatchCollection matches = regex.Matches(logItem.Text);
                            for (int c = 0; c < filterFile.ContentItems.Count; c++)
                            {
                                // todo: make sure this doesnt affect user pattern grouping which currently it does.
                                // maybe check group name???
                                if (matches[0].Groups[c + 1].Success) // (bool)filterObject[c,2] && !(bool)filterObject[c,3])
                                {
                                    
                                    if (fileItems[c].Enabled && !fileItems[c].Exclude)
                                    {
                                        Debug.Print("matches count:" + matches.Count);
                                        //matches[0].Groups[5].Success
                                        LogFileItem item = new LogFileItem()
                                        {
                                            Text = logItem.Text,
                                            Foreground = fileItems[c].Foreground, //(Brush)filterObject[c,0],
                                            Background = fileItems[c].Background, //(Brush)filterObject[c,1]
                                            FontSize = RegexViewerSettings.Settings.FontSize //fileItems[c].FontSize
                                        };

                                        // todo : fix recursive propertychanged
                                        //fileItems[c].Count++;
                                        filteredItems.Add(item);
                                        break;
                                    }
                                    else if (fileItems[c].Enabled && fileItems[c].Exclude)
                                    {
                                        break;
                                    }

                                }
                            }
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