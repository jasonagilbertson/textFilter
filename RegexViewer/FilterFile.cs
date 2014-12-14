﻿using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RegexViewer
{
    public class FilterFile : BaseFile<FilterFileItem>
    {
        #region Private Fields

        private ObservableCollection<FilterFileItem> _contentItems = new ObservableCollection<FilterFileItem>();
        private bool _patternNotifications;

        #endregion Private Fields

        #region Public Constructors

        public FilterFile()
        {
        }

        #endregion Public Constructors

        #region Public Properties

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

        #endregion Public Properties

        #region Public Methods

        public void EnablePatternNotifications(bool enable)
        {
            if (enable & !_patternNotifications)
            {
                _contentItems.CollectionChanged += _contentItems_CollectionChanged;
                foreach (FilterFileItem item in _contentItems)
                {
                    item.PropertyChanged += item_PropertyChanged;
                }

                item_PropertyChanged(this, new PropertyChangedEventArgs("enable"));
            }
            else if (!enable & _patternNotifications)
            {
                _contentItems.CollectionChanged -= _contentItems_CollectionChanged;
                foreach (FilterFileItem item in _contentItems)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                }
                Modified = false;
            }

            _patternNotifications = enable;
        }

        #endregion Public Methods

        #region Private Methods

        private void _contentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Modified = true;
            OnPropertyChanged("ContentList");
        }

        private void item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Modified = true;
            OnPropertyChanged(e.PropertyName);
        }

        #endregion Private Methods
    }
}