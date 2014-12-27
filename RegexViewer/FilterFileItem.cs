using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace RegexViewer
{
    public class FilterFileItem : ListBoxItem, IFileItem, INotifyPropertyChanged
    {
        #region Public Properties

        public string ForegroundColor
        {
            get
            {
                return _foregroundColor;
            }

            set
            {
                if (_foregroundColor != value)
                {
                    _foregroundColor = value;
                    this.Foreground = ((SolidColorBrush)new BrushConverter().ConvertFromString(value));
                    OnPropertyChanged("ForegroundColor");
                }
            }
        }

        public Int64 Index
        {
            get
            {
                return _index;
            }

            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        public string Notes
        {
            get
            {
                return _notes;
            }

            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged("Notes");
                }
            }
        }

        public bool Regex
        {
            get
            {
                return _regex;
            }

            set
            {
                if (_regex != value)
                {
                    _regex = value;

                    OnPropertyChanged("Regex");
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        // public event PropertyChangedEventHandler PropertyChanged;
        public IFileItem ShallowCopy()
        {
            return (FilterFileItem)this.MemberwiseClone();
        }

        #endregion Public Methods

        #region Private Fields

        private string _backgroundColor = "White";

        private int _count = 0;

        private bool _enabled = false;

        private bool _exclude = false;

        private string _filterpattern = string.Empty;

        private string _foregroundColor = "Black";

        private Int64 _index = 0;

        private string _notes = string.Empty;

        private bool _regex = false;

        #endregion Private Fields

        #region Public Constructors

        public FilterFileItem()
        {
            this.BackgroundColor = RegexViewerSettings.Settings.BackgroundColor.ToString();
            this.ForegroundColor = RegexViewerSettings.Settings.ForegroundColor.ToString();
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        public new Brush Background
        {
            get
            {
                return base.Background;
            }
            set
            {
                base.Background = value;
            }
        }

        public string BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }

            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    this.Background = ((SolidColorBrush)new BrushConverter().ConvertFromString(value));
                    OnPropertyChanged("BackgroundColor");
                }
            }
        }

        public new string Content
        {
            get
            {
                return base.Content.ToString();
            }
            set
            {
                base.Content = value;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }

            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged("Count");
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        public bool Exclude
        {
            get
            {
                return _exclude;
            }

            set
            {
                if (_exclude != value)
                {
                    _exclude = value;
                    OnPropertyChanged("Exclude");
                }
            }
        }

        public string Filterpattern
        {
            get
            {
                return _filterpattern;
            }

            set
            {
                if (_filterpattern != value)
                {
                    _filterpattern = value;
                    OnPropertyChanged("Filterpattern");
                }
            }
        }

        public new Brush Foreground
        {
            get
            {
                return base.Foreground;
            }

            set
            {
                base.Foreground = value;
            }
        }

        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}
    }
}