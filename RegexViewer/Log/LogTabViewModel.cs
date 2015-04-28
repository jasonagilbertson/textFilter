using System.Collections.Generic;

namespace RegexViewer
{
    public class LogTabViewModel : BaseTabViewModel<LogFileItem>
    {
        #region Private Fields

        private bool _group1Visibility = false;

        private bool _group2Visibility = false;

        private bool _group3Visibility = false;

        private bool _group4Visibility = false;

        #endregion Private Fields

        #region Public Constructors

        public LogTabViewModel()
        {
            List<LogFileItem> ContentList = new List<LogFileItem>();
        }

        #endregion Public Constructors

        #region Public Properties

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

        #endregion Public Properties

        #region Public Methods

        public void SetGroupCount(int count)
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

        #endregion Public Methods

        #region Public Structs

        public struct LogTabViewModelEvents
        {
            #region Public Fields

            public static string Group1Visibility = "Group1Visibility";
            public static string Group2Visibility = "Group2Visibility";
            public static string Group3Visibility = "Group3Visibility";
            public static string Group4Visibility = "Group4Visibility";

            #endregion Public Fields
        }

        #endregion Public Structs
    }
}