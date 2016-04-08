// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-25-2015
// ***********************************************************************
// <copyright file="FilterFile.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.ObjectModel;

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

        public string FilterNotes { get; set; }

        public string FilterVersion { get; set; }

        #endregion Public Properties

        #region Public Methods

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

        #endregion Public Methods

        #region Private Methods

        private void _contentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Modified = true;
            SetStatus("FilterFile:_contentItems_CollectionChanged");

            OnPropertyChanged("_contentItems_CollectionChanged");
        }

        private void item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((sender is FilterFileItem) && e.PropertyName != FilterFileItemEvents.Count)
            {
                Modified = true;
            }

            OnPropertyChanged(sender, e);
        }

        #endregion Private Methods
    }
}