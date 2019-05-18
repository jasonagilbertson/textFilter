// ************************************************************************************
// Assembly: TextFilter
// File: DisplayAllFile.xaml.cs
// Created: 2/12/2017
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

using System;

namespace TextFilter
{
    public partial class DisplayAllFile : Window, INotifyPropertyChanged
    {
        private ObservableCollection<string> _filterFileIndex = new ObservableCollection<string>();

        // { "0" };
        private string _initialMessage = string.Empty;

        private ObservableCollection<LogFileItem> _items = new ObservableCollection<LogFileItem>();
        private string _xmlMessage = string.Empty;

        public FilterFile FilterFile { get; set; }

        public ObservableCollection<string> FilterFileIndex
        {
            get
            {
                return _filterFileIndex;
            }
            set
            {
                //if (_filterFileIndex != value)
                //{
                _filterFileIndex = value;
                OnPropertyChanged("FilterFileIndex");
                //}
            }
        }

        public ObservableCollection<LogFileItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                //if (_items != value)
                //{
                _items = value;
                OnPropertyChanged("Items");
                //}
            }
        }

        public LogFile LogFile { get; set; }

        public DisplayAllFile()
        {
        }

        public DisplayAllFile(LogViewModel view)
        {
            Owner = Application.Current.MainWindow;
            DataContext = view.SelectedTab;
            InitializeComponent();
            _initialMessage = "";// file.FileName;
            Title = string.Format("{0} - {1}", _initialMessage, "");// file.Tag);
        }

        public DisplayAllFile(LogTabViewModel tab)
        {
            Owner = Application.Current.MainWindow;
            DataContext = tab;
            InitializeComponent();
            _initialMessage = "";// file.FileName;
            Title = string.Format("{0} - {1}", _initialMessage, "");// file.Tag);
        }

        public DisplayAllFile(LogFile logFile, FilterFile filterFile = null, string selectedIndex = "0")
        {
            LogFile = logFile;
            FilterFile = filterFile;

            Items = new ObservableCollection<LogFileItem>(logFile.ContentItems.Where(x => x.FilterIndex == Convert.ToInt32(selectedIndex)));
            //FilterFileIndex = new ObservableCollection<string>() { "0", "1" };
            FilterFileIndex.Add(selectedIndex);
            Owner = Application.Current.MainWindow;
            DataContext = this;
            InitializeComponent();

            _initialMessage = logFile.FileName;
            Title = string.Format("{0} - {1}", _initialMessage, logFile.Tag);

            UpdateLayout();
            //https://stackoverflow.com/questions/11420500/applying-datatemplate-to-a-grid
            //https://stackoverflow.com/questions/13246602/datatemplate-in-a-separate-resourcedictionary
            //https://wpftutorial.net/DataViews.html
            //http://mark-dot-net.blogspot.com/2008/12/list-filtering-in-wpf-with-m-v-vm.html
            //https://stackoverflow.com/questions/20888619/proper-way-to-use-collectionviewsource-in-viewmodel
        }

        public void Disable()
        {
            this.Hide();
            this.Close();
        }

        public void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        //    set
        //    {
        //        textBoxTraceMessage.IsEnabled = value;
        //        buttonSave.IsEnabled = value;
        //    }
        //}
        public string WaitForResult()
        {
            this.ShowDialog();
            //if (DialogCanSave)
            //{
            //    return textBoxTraceMessage.Text;
            //}
            //else
            //{
            return _initialMessage;
            //}
        }

        private string CheckXml(string message)
        {
            StringBuilder xmlMessage = new StringBuilder();

            try
            {
                // is there <> and is there same number? if so trim to outer edges
                int ltCount = Regex.Matches(message, "\\<").Count;
                int gtCount = Regex.Matches(message, "\\>").Count;

                if (ltCount > 1 & ltCount == gtCount)
                {
                    int ltStart = message.IndexOf("<");
                    int gtEnd = message.LastIndexOf(">");
                    // put header and footer back
                    xmlMessage.AppendLine(message.Substring(0, ltStart));
                    string tempXml = message.Substring(ltStart, gtEnd - ltStart + 1);
                    xmlMessage.AppendLine(XElement.Parse(tempXml, LoadOptions.PreserveWhitespace).ToString());
                    xmlMessage.AppendLine(message.Substring(gtEnd + 1, message.Length - gtEnd - 1));
                }

                return xmlMessage.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        //public bool DialogCanSave
        //{
        //    get
        //    {
        //        return textBoxTraceMessage.IsEnabled;
        //    }
        //private void buttonCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    textBoxTraceMessage.Text = _initialMessage;
        //    Disable();
        //}

        //private void buttonSave_Click(object sender, RoutedEventArgs e)
        //{
        //    Disable();
        //}
        public event PropertyChangedEventHandler PropertyChanged;
    }
}