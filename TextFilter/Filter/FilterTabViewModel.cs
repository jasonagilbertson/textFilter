// ************************************************************************************
// Assembly: TextFilter
// File: FilterTabViewModel.cs
// Created: 3/19/2017
// Modified: 3/25/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

namespace TextFilter
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        private bool _maskedVisibility = TextFilterSettings.Settings.CountMaskedMatches;

        public FilterTabViewModel()
        {
            TextFilterSettings.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        ~FilterTabViewModel()
        {
            TextFilterSettings.Settings.PropertyChanged -= Settings_PropertyChanged;
        }

        public bool MaskedVisibility
        {
            get
            {
                return _maskedVisibility;
            }
            set
            {
                if (_maskedVisibility != value)
                {
                    _maskedVisibility = value;
                    OnPropertyChanged("MaskedVisibility");
                }
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == (TextFilterSettings.AppSettingNames.CountMaskedMatches).ToString())
            {
                MaskedVisibility = TextFilterSettings.Settings.CountMaskedMatches;
            }
        }
    }
}