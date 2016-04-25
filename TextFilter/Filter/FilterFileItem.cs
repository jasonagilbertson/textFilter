// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-25-2015 ***********************************************************************
// <copyright file="FilterFileItem.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextFilter
{
    public struct FilterFileItemEvents
    {

        #region Fields

        public static string Background = "Background";

        public static string BackgroundColor = "BackgroundColor";

        public static string CaseSensitive = "CaseSensitive";

        public static string Count = "Count";

        public static string Enabled = "Enabled";

        public static string Exclude = "Exclude";

        public static string Filterpattern = "Filterpattern";

        public static string Foreground = "Foreground";

        public static string ForegroundColor = "ForegroundColor";

        public static string Index = "Index";

        public static string MaskedCount = "MaskedCount";

        public static string Notes = "Notes";

        public static string Regex = "Regex";

        #endregion Fields

    }

    public class FilterFileItem : ListBoxItem, IFileItem, INotifyPropertyChanged
    {

        #region Fields

        private string _backgroundColor;

        private bool _caseSensitive;

        private int _count = 0;

        private bool _enabled = false;

        private bool _exclude = false;

        private string _filterpattern = string.Empty;

        private string _foregroundColor;

        private bool _include = false;

        private int _index = 0;

        private int _maskedCount = 0;

        private string _notes = string.Empty;

        private bool _regex = false;
        private bool _stringOperators = false;

        #endregion Fields

        #region Constructors

        public FilterFileItem()
        {
            BackgroundColor = TextFilterSettings.Settings.BackgroundColor.ToString();
            ForegroundColor = TextFilterSettings.Settings.ForegroundColor.ToString();
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

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
                    Background = ((SolidColorBrush)new BrushConverter().ConvertFromString(value));
                    OnPropertyChanged(FilterFileItemEvents.BackgroundColor);
                }
            }
        }

        public bool CaseSensitive
        {
            get
            {
                return _caseSensitive;
            }
            set
            {
                if (_caseSensitive != value)
                {
                    _caseSensitive = value;
                    OnPropertyChanged("CaseSensitive");
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
                        Count = 0;
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
                    OnPropertyChanged(FilterFileItemEvents.Exclude);
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
                }
            }
        }

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
                    Foreground = ((SolidColorBrush)new BrushConverter().ConvertFromString(value));
                    OnPropertyChanged(FilterFileItemEvents.ForegroundColor);
                }
            }
        }

        public int GroupCount { get; set; }

        public bool Include
        {
            get
            {
                return _include;
            }
            set
            {
                if (_include != value)
                {
                    _include = value;
                    OnPropertyChanged("Include");
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

        public int MaskedCount
        {
            get
            {
                return _maskedCount;
            }

            set
            {
                if (_maskedCount != value)
                {
                    _maskedCount = value;
                    OnPropertyChanged(FilterFileItemEvents.MaskedCount);
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

        public bool StringOperators
        {
            get
            {
                return _stringOperators;
            }

            set
            {
                if (_stringOperators != value)
                {
                    _stringOperators = value;
                }
            }
        }

        // for tat 'type' text or marker

        public string TatType { get; set; }

        #endregion Properties

        #region Methods

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
            return (FilterFileItem)MemberwiseClone();
        }

        #endregion Methods

    }
}