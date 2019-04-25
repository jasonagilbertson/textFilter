// ************************************************************************************
// Assembly: TextFilter
// File: Base.cs
// Created: 3/19/2017
// Modified: 3/25/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TextFilter
{
    public class Base : INotifyPropertyChanged
    {
        public enum CurrentStatusSetting
        {
            enter_to_filter,
            filtered,
            quick_filtered,
            showing_all,
            filtering,
        }

        public string _tempTabNameFormat = "-new {0}-";
        public string _tempTabNameFormatPattern = @"\-new [0-9]{1,2}\-";
        public bool _transitioning;
        public int MaxGroupCount = 4;

        private Command _duplicateWindowCommand;
        private bool _filterIndexVisibility = false;//todo fix: TextFilterSettings.Settings.FilterIndexVisible;

        private bool _group1Visibility = false;

        private bool _group2Visibility = false;

        private bool _group3Visibility = false;

        private bool _group4Visibility = false;
        private Command _newWindowCommand;

        public static FilterViewModel _FilterViewModel { get; set; }

        public static LogViewModel _LogViewModel { get; set; }

        public static MainViewModel _MainViewModel { get; set; }

        public static Parser _Parser { get; set; }

        public Command DuplicateWindowCommand
        {
            get
            {
                if (_duplicateWindowCommand == null)
                {
                    _duplicateWindowCommand = new Command(DuplicateWindowExecuted);
                }
                _duplicateWindowCommand.CanExecute = true;

                return _duplicateWindowCommand;
            }
            set { _duplicateWindowCommand = value; }
        }

        public bool FilterIndexVisibility
        {
            get
            {
                return _filterIndexVisibility;
            }
            set
            {
                if (_filterIndexVisibility != value)
                {
                    _filterIndexVisibility = value;
                    OnPropertyChanged("FilterIndexVisibility");
                }
            }
        }

        public bool Group1Visibility
        {
            get
            {
                return _group1Visibility;
            }
            private set
            {
                if (_group1Visibility != value)
                {
                    _group1Visibility = value;
                    OnPropertyChanged(LogTabViewModelEvents.Group1Visibility);
                }
            }
        }

        public bool Group2Visibility
        {
            get
            {
                return _group2Visibility;
            }
            private set
            {
                if (_group2Visibility != value)
                {
                    _group2Visibility = value;
                    OnPropertyChanged(LogTabViewModelEvents.Group2Visibility);
                }
            }
        }

        public bool Group3Visibility
        {
            get
            {
                return _group3Visibility;
            }
            private set
            {
                if (_group3Visibility != value)
                {
                    _group3Visibility = value;
                    OnPropertyChanged(LogTabViewModelEvents.Group3Visibility);
                }
            }
        }

        public bool Group4Visibility
        {
            get
            {
                return _group4Visibility;
            }
            private set
            {
                if (_group4Visibility != value)
                {
                    _group4Visibility = value;
                    OnPropertyChanged(LogTabViewModelEvents.Group4Visibility);
                }
            }
        }

        public int GroupCount { get; private set; }

        public Command NewWindowCommand
        {
            get
            {
                if (_newWindowCommand == null)
                {
                    _newWindowCommand = new Command(NewWindowExecuted);
                }
                _duplicateWindowCommand.CanExecute = true;

                return _newWindowCommand;
            }
            set { _newWindowCommand = value; }
        }

        public void CreateProcess(string process, string arguments = null)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.UseShellExecute = true;
                processInfo.CreateNoWindow = false;
                if (!string.IsNullOrEmpty(arguments))
                {
                    processInfo.Arguments = arguments;
                }
                processInfo.FileName = process;
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                SetStatus("CreateProcess: exception" + e.ToString());
            }
        }

        public void DuplicateWindowExecuted(object sender)
        {
            NewWindow(true);
        }

        public void ExecuteAsAdmin(string fileName, string arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        public T FindVisualChild<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            if (parent == null)
            {
                SetStatus("findvisualchild: error: no parent");
                return default(T);
            }

            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    UIElement child = VisualTreeHelper.GetChild(parent, i) as UIElement;
                    if (child is T)
                    {
                        SetStatus("findvisualchild: found child");
                        return VisualTreeHelper.GetChild(parent, i) as T;
                    }
                    else
                    {
                        if (VisualTreeHelper.GetChildrenCount(child) > 0)
                        {
                            T rChild = FindVisualChild<T>(child);
                            if (rChild is T)
                            {
                                SetStatus("findvisualchild: found rchild");
                                return rChild as T;
                            }
                        }
                    }
                }
            }
            else
            {
                SetStatus("findvisualchild: no children");
            }

            SetStatus("findvisualchild: child of type not found");
            return null;
        }

        public T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        public StringBuilder FormatExportItem(bool fileItemEnabled, string separator, bool removeEmpty, string fileItemValue, StringBuilder sb)
        {
            if (fileItemEnabled)
            {
                if (removeEmpty && String.IsNullOrEmpty(fileItemValue))
                {
                    return sb;
                }

                sb.Append((sb.Length > 0 ? separator : "") + fileItemValue);
            }

            return sb;
        }

        public T GetFirstChildByType<T>(DependencyObject prop) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(prop); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild((prop), i) as DependencyObject;
                if (child == null)
                    continue;

                T castedProp = child as T;
                if (castedProp != null)
                    return castedProp;

                castedProp = GetFirstChildByType<T>(child);

                if (castedProp != null)
                    return castedProp;
            }
            return null;
        }

        public void NewWindow(bool withTabs = false, string file = "")
        {
            StringBuilder args = new StringBuilder();

            if (!string.IsNullOrEmpty(file))
            {
                if (this is FilterViewModel)
                {
                    args.Append(string.Format("/filter: \"{0}\"", file));
                }
                else
                {
                    args.Append(string.Format("/log: \"{0}\"", file));
                }
            }
            else if (withTabs)
            {
                if (TextFilterSettings.Settings.CurrentFilterFiles.Count > 0)
                {
                    args.Append(string.Format("/filter: \"{0}\"", string.Join("\";\"", TextFilterSettings.Settings.CurrentFilterFiles)));
                }

                if (TextFilterSettings.Settings.CurrentLogFiles.Count > 0)
                {
                    if (args.Length > 0)
                    {
                        args.Append(" ");
                    }

                    args.Append(string.Format("/log: \"{0}\"", string.Join("\";\"", TextFilterSettings.Settings.CurrentLogFiles)));
                }
            }

            TextFilterSettings.Settings.Save();
            CreateProcess(Process.GetCurrentProcess().MainModule.FileName, args.ToString());
            Debug.Print(args.ToString());
        }

        public void NewWindowExecuted(object sender)
        {
            NewWindow(false);
        }

        public void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_transitioning)
            {
                return;
            }

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void SetCurrentStatus(CurrentStatusSetting status)
        {
            EventHandler<string> newCurrentStatus = NewCurrentStatus;
            if (newCurrentStatus != null)
            {
                newCurrentStatus(this, Enum.GetName(typeof(CurrentStatusSetting), status).Replace("_", " ").ToUpper());
            }
        }

        public void SetGroupCount(int count)
        {
            GroupCount = count;

            if (count > 0)
            {
                Group1Visibility = true;
            }
            else
            {
                Group1Visibility = false;
            }

            if (count > 1)
            {
                Group2Visibility = true;
            }
            else
            {
                Group2Visibility = false;
            }

            if (count > 2)
            {
                Group3Visibility = true;
            }
            else
            {
                Group3Visibility = false;
            }

            if (count > 3)
            {
                Group4Visibility = true;
            }
            else
            {
                Group4Visibility = false;
            }

            if (count > MaxGroupCount)
            {
                SetStatus(string.Format("Warning: max group count is {0}. only {0} groups will be displayed. current group count: {1}", MaxGroupCount, count));
            }
        }

        public void SetStatus(string status)
        {
            if (status.ToLower().StartsWith("fatal:"))
            {
                MessageBox.Show(status, "Oh Snap! TextFilter exception", MessageBoxButton.OK);
            }

            try
            {
                if (!string.IsNullOrEmpty(TextFilterSettings.Settings.DebugFile))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(TextFilterSettings.Settings.DebugFile, true))
                    {
                        file.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("hh:mm:ss.fff"), status));
                    }
                }

                Debug.Print(status);
            }
            catch (Exception e)
            {
                Debug.Print(string.Format("SetStatus:exception: {0}: {1}", status, e));
            }
        }

        public static event EventHandler<string> NewCurrentStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        public struct LogTabViewModelEvents
        {
            public static string Group1Visibility = "Group1Visibility";

            public static string Group2Visibility = "Group2Visibility";

            public static string Group3Visibility = "Group3Visibility";

            public static string Group4Visibility = "Group4Visibility";
        }
    }
}