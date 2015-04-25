using System.Collections.Generic;
using System.Windows;

namespace RegexViewer
{
    public class LogTabViewModel : BaseTabViewModel<LogFileItem>
    {
        #region Public Constructors
        
        private bool _group1Visibility = false;
        private bool _group2Visibility = false;
        private bool _group3Visibility = false;
        private bool _group4Visibility = false;

        public int GroupCount { get; private set; }
        public void SetGroupCount (int count)
        {
            this.GroupCount = count;

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
        }
        public LogTabViewModel()
        {
            List<LogFileItem> ContentList = new List<LogFileItem>();
        }

        public bool Group1Visibility
        {
            get
            {
                return _group1Visibility;
            }
            set
            {
                if (_group1Visibility != value)
                {
                    _group1Visibility = value;
                    OnPropertyChanged("Group1Visibility");
                }
            }
        }

        
        public bool Group2Visibility
        {
            get
            {
                return _group2Visibility;
            }
            set
            {
                if (_group2Visibility != value)
                {
                    _group2Visibility = value;
                    OnPropertyChanged("Group2Visibility");
                }
            }
        }

        public bool Group3Visibility
        {
            get
            {
                return _group3Visibility;
            }
            set
            {
                if (_group3Visibility != value)
                {
                    _group3Visibility = value;
                    OnPropertyChanged("Group3Visibility");
                }
            }
        }
        public bool Group4Visibility
        {
            get
            {
                return _group4Visibility;
            }
            set
            {
                if (_group4Visibility != value)
                {
                    _group4Visibility = value;
                    OnPropertyChanged("Group4Visibility");
                }
            }
        }
        #endregion Public Constructors
    }
}