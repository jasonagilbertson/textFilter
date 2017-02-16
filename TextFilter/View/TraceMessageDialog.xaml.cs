﻿// ************************************************************************************
// Assembly: TextFilter
// File: tracemessagedialog.xaml.cs
// Created: 2/12/2017
// Modified: 2/12/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace TextFilter
{
    public partial class TraceMessageDialog : Window
    {
        private string _initialMessage = string.Empty;
        private string _xmlMessage = string.Empty;

        public TraceMessageDialog(string message, int id, string file)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            _initialMessage = message;
            Title = string.Format("{0} - {1}", id, file);
            textBoxTraceMessage.Text = _initialMessage.Replace(",", ",\r\n").Replace(";", ";\r\n").Replace(". ", ". \r\n").Replace("\t", "\r\n");
            _xmlMessage = CheckXml(_initialMessage);

            if (!string.IsNullOrEmpty(_xmlMessage))
            {
                textBoxTraceMessage.Text = _xmlMessage;
            }

            textBoxTraceMessage.Focus();
            textBoxTraceMessage.FontFamily = new FontFamily(TextFilterSettings.Settings.FontName);
            textBoxTraceMessage.FontSize = TextFilterSettings.Settings.FontSize;
            textBoxTraceMessage.Foreground = TextFilterSettings.Settings.ForegroundColor;
            textBoxTraceMessage.Background = TextFilterSettings.Settings.BackgroundColor;
        }

        public void Disable()
        {
            this.Hide();
            this.Close();
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
    }
}