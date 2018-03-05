// ************************************************************************************
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
        public LogTabViewModel()
        {
            // List<LogFileItem> ContentList = new List<LogFileItem>();
            TextFilterSettings.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        ~LogTabViewModel()
        {
            TextFilterSettings.Settings.PropertyChanged -= Settings_PropertyChanged;
        }
        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == (TextFilterSettings.AppSettingNames.FilterIndexVisible).ToString())
            {
                FilterIndexVisibility = TextFilterSettings.Settings.FilterIndexVisible;
            }
        }
      
    }
}