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
    }
}