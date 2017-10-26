// ************************************************************************************
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
using System.Runtime.Serialization.Json;
using System.IO;
using System;
using System.Web.Script.Serialization;

namespace TextFilter
{
    public partial class TraceMessageDialog : Window
    {
        private string _initialMessage = string.Empty;
        private string _xmlMessage = string.Empty;
        private string _jsonMessage = string.Empty;

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
            else if (Regex.IsMatch(_initialMessage, "{.+}"))
            {
                // see if it is json
                Match jsonMessage = Regex.Match(_initialMessage, "({.+})");
                string formattedJson = JsonFormat(jsonMessage.ToString());
                if (formattedJson != string.Empty)
                {
                    textBoxTraceMessage.Text = string.Format("{0}\r\n{1}", _initialMessage.Replace(jsonMessage.ToString(), ""), formattedJson);
                }
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
        public string WaitForResult()
        {
            this.ShowDialog();
            return _initialMessage;
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

        private string JsonFormat(string text)
        {
            try
            {
                // originally from https://dl.dropboxusercontent.com/u/52219470/Source/JSonPresentationFormatter.cs
                // http://chrisghardwick.blogspot.com/2012/01/c-json-pretty-print-simple-c-json.html
                if (string.IsNullOrEmpty(text)) return string.Empty;
                text = text.Replace(System.Environment.NewLine, string.Empty).Replace("\t", string.Empty);

                var offset = 0;
                var output = new StringBuilder();
                Action<StringBuilder, int> tabs = (sb, pos) => { for (var i = 0; i < pos; i++) { sb.Append("\t"); } };
                Func<string, int, Nullable<Char>> previousNotEmpty = (s, i) =>
                {
                    if (string.IsNullOrEmpty(s) || i <= 0) return null;

                    Nullable<Char> prev = null;

                    while (i > 0 && prev == null)
                    {
                        prev = s[i - 1];
                        if (prev.ToString() == " ") prev = null;
                        i--;
                    }

                    return prev;
                };
                Func<string, int, Nullable<Char>> nextNotEmpty = (s, i) =>
                {
                    if (string.IsNullOrEmpty(s) || i >= (s.Length - 1)) return null;

                    Nullable<Char> next = null;
                    i++;

                    while (i < (s.Length - 1) && next == null)
                    {
                        next = s[i++];
                        if (next.ToString() == " ") next = null;
                    }

                    return next;
                };

                var inQuote = false;
                var ignoreQuote = false;

                for (var i = 0; i < text.Length; i++)
                {
                    var chr = text[i];

                    if (chr == '"' && !ignoreQuote) inQuote = !inQuote;
                    if (chr == '\'' && inQuote) ignoreQuote = !ignoreQuote;
                    if (inQuote)
                    {
                        output.Append(chr);
                    }
                    else if (chr.ToString() == "{")
                    {
                        offset++;
                        output.Append(chr);
                        output.Append(System.Environment.NewLine);
                        tabs(output, offset);
                    }
                    else if (chr.ToString() == "}")
                    {
                        offset--;
                        output.Append(System.Environment.NewLine);
                        tabs(output, offset);
                        output.Append(chr);

                    }
                    else if (chr.ToString() == ",")
                    {
                        output.Append(chr);
                        output.Append(System.Environment.NewLine);
                        tabs(output, offset);
                    }
                    else if (chr.ToString() == "[")
                    {
                        output.Append(chr);

                        var next = nextNotEmpty(text, i);

                        if (next != null && next.ToString() != "]")
                        {
                            offset++;
                            output.Append(System.Environment.NewLine);
                            tabs(output, offset);
                        }
                    }
                    else if (chr.ToString() == "]")
                    {
                        var prev = previousNotEmpty(text, i);

                        if (prev != null && prev.ToString() != "[")
                        {
                            offset--;
                            output.Append(System.Environment.NewLine);
                            tabs(output, offset);
                        }

                        output.Append(chr);
                    }
                    else
                        output.Append(chr);
                }

                return output.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}