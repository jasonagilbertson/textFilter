﻿// ************************************************************************************
// Assembly: TextFilter
// File: LogTabViewModel.cs
// Created: 3/19/2017
// Modified: 3/25/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

namespace TextFilter
{
    public class LogTabViewModel : BaseTabViewModel<LogFileItem>
    {
        public int MaxGroupCount = 4;

        private bool _filterIndexVisibility = TextFilterSettings.Settings.FilterIndexVisible;

        private bool _group1Visibility = false;

        private bool _group2Visibility = false;

        private bool _group3Visibility = false;

        private bool _group4Visibility = false;
        public LogTabViewModel()
        {
            // List<LogFileItem> ContentList = new List<LogFileItem>();
            TextFilterSettings.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        ~LogTabViewModel()
        {
            TextFilterSettings.Settings.PropertyChanged -= Settings_PropertyChanged;
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

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == (TextFilterSettings.AppSettingNames.FilterIndexVisible).ToString())
            {
                FilterIndexVisibility = TextFilterSettings.Settings.FilterIndexVisible;
            }
        }
        public struct LogTabViewModelEvents
        {
            public static string Group1Visibility = "Group1Visibility";

            public static string Group2Visibility = "Group2Visibility";

            public static string Group3Visibility = "Group3Visibility";

            public static string Group4Visibility = "Group4Visibility";
        }
    }
}