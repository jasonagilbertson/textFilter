using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexViewer
{
    public class FilterFile : BaseFile<FilterFileItem>
    {

      
        #region Public Constructors
        private bool _patternNotifications;
        private ObservableCollection<FilterFileItem> _contentItems  = new ObservableCollection<FilterFileItem>();
        public FilterFile()
        {

        }

        public void EnablePatternNotifications(bool enable)
        {
                if(enable & !_patternNotifications)
                {
                    _contentItems.CollectionChanged += _contentItems_CollectionChanged;
                    foreach(FilterFileItem item in _contentItems)
                    {
                        item.PropertyChanged += item_PropertyChanged;
                    }

                    item_PropertyChanged(this, new PropertyChangedEventArgs("enable"));
                }
                else if(!enable & _patternNotifications)
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

        void item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Modified = true;
            OnPropertyChanged(e.PropertyName);
            
        }
        void _contentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Modified = true;
            OnPropertyChanged("ContentList");
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
    }
}