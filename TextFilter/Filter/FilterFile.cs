﻿// ************************************************************************************
// Assembly: TextFilter
// File: FilterFile.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.ObjectModel;

namespace TextFilter
{
    public class FilterFile : BaseFile<FilterFileItem>
    {
        private ObservableCollection<FilterFileItem> _contentItems = new ObservableCollection<FilterFileItem>();

        private bool _patternNotifications;

        public override ObservableCollection<FilterFileItem> ContentItems
        {
            get
            {
                return _contentItems;
            }

            set
            {
                _contentItems = value;
                OnPropertyChanged("ContentItems");
            }
        }

        public string FilterNotes { get; set; }

        public string FilterVersion { get; set; }

        public FilterFile()
        {
        }

        public void AddPatternNotification(FilterFileItem filterFileItem, bool enable)
        {
            if (enable)
            {
                filterFileItem.PropertyChanged += item_PropertyChanged;
            }
            else
            {
                filterFileItem.PropertyChanged -= item_PropertyChanged;
            }
        }

        public void EnablePatternNotifications(bool enable)
        {
            if (enable & !_patternNotifications)
            {
                _contentItems.CollectionChanged += _contentItems_CollectionChanged;

                foreach (FilterFileItem item in _contentItems)
                {
                    AddPatternNotification(item, enable);
                }
            }
            else if (!enable & _patternNotifications)
            {
                _contentItems.CollectionChanged -= _contentItems_CollectionChanged;
                foreach (FilterFileItem item in _contentItems)
                {
                    AddPatternNotification(item, enable);
                }
            }

            _patternNotifications = enable;
        }

        private void _contentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Modified = true;
            SetStatus("FilterFile:_contentItems_CollectionChanged");

            OnPropertyChanged("_contentItems_CollectionChanged");
        }

        private void item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((sender is FilterFileItem) && e.PropertyName != FilterFileItemEvents.Count && e.PropertyName != FilterFileItemEvents.MaskedCount)
            {
                Modified = true;
            }

            OnPropertyChanged(sender, e);
        }
    }
}