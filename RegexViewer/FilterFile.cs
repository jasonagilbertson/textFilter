using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexViewer
{
    public class FilterFile : BaseFile<FilterFileItem>
    {

        public static class TagPrefix
        {
            public static string disabledPattern = "d";
            public static string excludePattern = "e";
            public static string regexPattern = "r";
            public static string stringPattern = "s";

        }

        #region Public Constructors
        private string _regexPattern;
        private bool _patternNotifications;
        private ObservableCollection<FilterFileItem> _contentItems  = new ObservableCollection<FilterFileItem>();
        public FilterFile()
        {

            //this.ContentItems = new ObservableCollection<FilterFileItem>();

            
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

                  //  RebuildRegex();
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
          //  this.EnablePatternNotifications(false);

            RebuildRegex();
            OnPropertyChanged("ContentList");
          //  this.EnablePatternNotifications(true);
            
        }
        void _contentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            RebuildRegex();
            OnPropertyChanged("ContentList");
        }

        public void RebuildRegex()
        {
            // http://msdn.microsoft.com/en-us/library/bs2twtah(v=vs.110).aspx
            StringBuilder pattern = new StringBuilder();

            int disabledCount = 0;
            int excludeCount = 0;
            int regexCount = 0;
            int stringCount = 0;
            int tagCount = 0;
            Modified = true;
      //      ContentItems =  new ObservableCollection<FilterFileItem>(_contentItems.OrderBy(x => x.Index));
            foreach (FilterFileItem filterItem in _contentItems.OrderBy(x => x.Index))
        //    foreach (FilterFileItem filterItem in _contentItems)
            {
                if (string.IsNullOrEmpty(filterItem.Filterpattern))
                {
                    continue;
                }

                Regex regex;
                string filterPattern = filterItem.Filterpattern;
                string tagPrefix = string.Empty;

                if(!filterItem.Enabled)
                {
                    tagPrefix = TagPrefix.disabledPattern;
                    tagCount = disabledCount++;
                }
                else if (filterItem.Exclude)
                {
                    tagPrefix = TagPrefix.excludePattern;
                    tagCount = excludeCount++;
                }
                else if(filterItem.Regex)
                {
                    tagPrefix = TagPrefix.regexPattern;
                    tagCount = regexCount++;
                }
                else
                {
                    tagPrefix = TagPrefix.stringPattern;
                    filterPattern = Regex.Escape(filterPattern);
                    tagCount = stringCount++;
                }

                try
                {
                    regex = new Regex(filterItem.Filterpattern);
                    //if ((regex = new Regex(filterItem.Filterpattern)) != null)
                    //{
                        //if (!string.IsNullOrEmpty(pattern.ToString()))
                        //{
                        //    pattern.Append("|");
                        //}

                        //pattern.Append(string.Format("(<?i{0}{1}{2}>{3})", filterItem.Index, tagPrefix, tagCount, filterPattern));
                    //}
                }
                catch
                {
                    // throw new KeyNotFoundException();
                    // treat as string so escape it
                    SetStatus("invalid regex pattern in filter");
                    pattern.Append(string.Format("(?<i{0}{1}{2}>{3})", filterItem.Index, tagPrefix, tagCount, Regex.Escape(filterItem.Filterpattern)));
                }

                if (!string.IsNullOrEmpty(pattern.ToString()))
                {
                    pattern.Append("|");
                }

                pattern.Append(string.Format("(?<i{0}{1}{2}>{3})", filterItem.Index, tagPrefix, tagCount, filterPattern));

            }

            SetStatus(string.Format("RebuildRegex:{0}", pattern.ToString()));
            RegexPattern = pattern.ToString();

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

        public string RegexPattern 
        {
            get
            {
                return _regexPattern;
            }
            set
            {
                if(_regexPattern !=value)
                {
                    _regexPattern = value;
                    OnPropertyChanged("RegexPattern");
                }
            }
        }

        #endregion Public Properties
    }
}