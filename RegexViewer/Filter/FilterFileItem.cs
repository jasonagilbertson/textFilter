using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace RegexViewer
{
    public struct FilterFileItemEvents
    {
        #region Public Fields

        public static string Background = "Background";
        public static string BackgroundColor = "BackgroundColor";

        public static string Count = "Count";
        public static string Enabled = "Enabled";
        public static string Exclue = "Exclude";
        public static string Filterpattern = "Filterpattern";

        public static string Foreground = "Foreground";
        public static string ForegroundColor = "ForegroundColor";

        public static string Index = "Index";
        public static string Notes = "Notes";
        public static string Regex = "Regex";

        #endregion Public Fields
    }

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
                    OnPropertyChanged(FilterFileItemEvents.ForegroundColor);
                }
            }
        }

        public int Index
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
                    OnPropertyChanged(FilterFileItemEvents.Index);
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
                    OnPropertyChanged(FilterFileItemEvents.Notes);
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

                    OnPropertyChanged(FilterFileItemEvents.Regex);
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

        private string _backgroundColor;

        private int _count = 0;

        // = "White";
        private bool _enabled = false;

        private bool _exclude = false;

        private string _filterpattern = string.Empty;

        private string _foregroundColor;

        private int _index = 0;

        // = "Black";
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
                if (base.Background != value)
                {
                    base.Background = value;
                    // OnPropertyChanged(FilterFileItemEvents.Background);
                }
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
                    OnPropertyChanged(FilterFileItemEvents.BackgroundColor);
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
                    OnPropertyChanged(FilterFileItemEvents.Count);
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

                    // clear counter
                    if (!_enabled)
                    {
                        this.Count = 0;
                    }

                    OnPropertyChanged(FilterFileItemEvents.Enabled);
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
                    OnPropertyChanged(FilterFileItemEvents.Exclue);
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
                    OnPropertyChanged(FilterFileItemEvents.Filterpattern);
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
                if (base.Foreground != value)
                {
                    base.Foreground = value;
                    // OnPropertyChanged(FilterFileItemEvents.Foreground);
                }
            }
        }
    }
}