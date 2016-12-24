// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-09-2015 ***********************************************************************
// <copyright file="HtmlFragment.cs" company="http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

namespace TextFilter
{
    // Sample class for Copying and Pasting HTML fragments to and from the clipboard.
    //
    // Mike Stall. http://blogs.msdn.com/jmstall
    using System;
    using System.Text;

    //using System.Windows.Forms;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Windows;
    using System.Windows.Media;

    internal class HtmlFragment
    {
        #region Fields

        private StringBuilder htmlClipBuilder;

        private string m_fragment;

        private string m_fullText;

        private System.Uri m_source;

        // Data. See properties for descriptions.

        private string m_version;

        private StringBuilder textClipBuilder;

        #endregion Fields

        #region Constructors

        public HtmlFragment()
        {
            htmlClipBuilder = new StringBuilder();
            textClipBuilder = new StringBuilder();
            Clipboard.Clear();
        }

        public HtmlFragment(string rawClipboardText)
        {
            ProcessFragment(rawClipboardText);
        }

        #endregion Constructors

        #region Properties

        public string Context
        {
            get { return m_fullText; }
        }

        public string Fragment
        {
            get { return m_fragment; }
        }

        public System.Uri SourceUrl
        {
            get { return m_source; }
        }

        public string Version
        {
            get { return m_version; }
        }

        #endregion Properties

        #region Methods

        public static void CopyToClipboard(string htmlFragment, string textFragment)
        {
            CopyToClipboard(htmlFragment, textFragment, null, null);
        }

        public static void CopyToClipboard(string htmlFragment, string textFragment, string title, Uri sourceUrl)
        {
            if (title == null) title = "From Clipboard";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Builds the CF_HTML header. See format specification here:
            // http: //msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // The string contains index references to other spots in the string, so we need
            // placeholders so we can compute the offsets. The <<<<<<<_ strings are just
            // placeholders. We'll backpatch them actual values afterwards. The string layout (<<<)
            // also ensures that it can't appear in the body of the html because the < character must
            // be escaped.
            string header = @"Format:HTML Format
                                Version:1.0
                                StartHTML:<<<<<<<1
                                EndHTML:<<<<<<<2
                                StartFragment:<<<<<<<3
                                EndFragment:<<<<<<<4
                                StartSelection:<<<<<<<3
                                EndSelection:<<<<<<<3
                                ";

            string pre = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
                            <HTML><HEAD><TITLE>" + title + @"</TITLE></HEAD><BODY><!--StartFragment-->";

            string post = @"<!--EndFragment--></BODY></HTML>";

            sb.Append(header);
            if (sourceUrl != null)
            {
                sb.AppendFormat("SourceURL:{0}", sourceUrl);
            }
            int startHTML = sb.Length;

            sb.Append(pre);
            int fragmentStart = sb.Length;

            sb.Append(htmlFragment);
            int fragmentEnd = sb.Length;

            sb.Append(post);
            int endHTML = sb.Length;

            // Backpatch offsets
            sb.Replace("<<<<<<<1", To8DigitString(startHTML));
            sb.Replace("<<<<<<<2", To8DigitString(endHTML));
            sb.Replace("<<<<<<<3", To8DigitString(fragmentStart));
            sb.Replace("<<<<<<<4", To8DigitString(fragmentEnd));

            // Finally copy to clipboard.
            string data = sb.ToString();
            Clipboard.Clear();

            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, data);
            dataObject.SetData(DataFormats.Text, textFragment);
            dataObject.SetData(DataFormats.UnicodeText, textFragment);
            Clipboard.SetDataObject(dataObject);
            //Clipboard.SetText(data, TextDataFormat.Html);
        }

        static public HtmlFragment FromClipboard()
        {
            string rawClipboardText = Clipboard.GetText(TextDataFormat.Html);
            HtmlFragment h = new HtmlFragment(rawClipboardText);
            return h;
        }

        public void AddClipToList(string fragment, Brush backgroundColor, Brush foregroundColor)
        {
            // convert 8 digit hex a,r,g,b to 6 digit hex r,g,b
            string bColor = backgroundColor.ToString();
            string fColor = foregroundColor.ToString();
            fColor = string.Format("#{0}", fColor.Substring(fColor.Length - 6));
            bColor = string.Format("#{0}", bColor.Substring(bColor.Length - 6));

            string colorText = HttpUtility.HtmlEncode(fragment);
            colorText = string.Format("<p><span style=\"background-color:{0};color:{1}\">{2}</span></p>", bColor, fColor, colorText);

            htmlClipBuilder.AppendLine(colorText);
            textClipBuilder.AppendLine(fragment);
        }

        public void CopyListToClipboard()
        {
            // ProcessFragment(htmlClipBuilder.ToString());
            CopyToClipboard(htmlClipBuilder.ToString(), textClipBuilder.ToString());
            htmlClipBuilder.Clear();
            textClipBuilder.Clear();
        }

        // Helper to convert an integer into an 8 digit string. String must be 8 characters, because
        // it will be used to replace an 8 character string within a larger string.

        private static string To8DigitString(int x)
        {
            return String.Format("{0,8}", x);
        }

        private void ProcessFragment(string rawClipboardText)
        {
            rawClipboardText = HttpUtility.HtmlEncode(rawClipboardText);
            // This decodes CF_HTML, which is an entirely text format using UTF-8. Format of this
            // header is described at:
            // http: //msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // Note the counters are byte counts in the original string, which may be Ansi. So byte
            // counts may be the same as character counts (since sizeof(char) == 1). But
            // System.String is unicode, and so byte couns are no longer the same as character
            // counts, (since sizeof(wchar) == 2).
            int startHMTL = 0;
            int endHTML = 0;

            int startFragment = 0;
            int endFragment = 0;

            Regex r;
            Match m;

            r = new Regex("([a-zA-Z]+):(.+?)[\r\n]",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            for (m = r.Match(rawClipboardText); m.Success; m = m.NextMatch())
            {
                string key = m.Groups[1].Value.ToLower();
                string val = m.Groups[2].Value;

                switch (key)
                {
                    // Version number of the clipboard. Starting version is 0.9.
                    case "version":
                        m_version = val;
                        break;

                    // Byte count from the beginning of the clipboard to the start of the context, or
                    // -1 if no context
                    case "starthtml":
                        if (startHMTL != 0) throw new FormatException("StartHtml is already declared");
                        startHMTL = int.Parse(val);
                        break;

                    // Byte count from the beginning of the clipboard to the end of the context, or
                    // - 1 if no context.
                    case "endhtml":
                        if (startHMTL == 0) throw new FormatException("StartHTML must be declared before endHTML");
                        endHTML = int.Parse(val);

                        m_fullText = rawClipboardText.Substring(startHMTL, endHTML - startHMTL);
                        break;

                    // Byte count from the beginning of the clipboard to the start of the fragment.
                    case "startfragment":
                        if (startFragment != 0) throw new FormatException("StartFragment is already declared");
                        startFragment = int.Parse(val);
                        break;

                    // Byte count from the beginning of the clipboard to the end of the fragment.
                    case "endfragment":
                        if (startFragment == 0) throw new FormatException("StartFragment must be declared before EndFragment");
                        endFragment = int.Parse(val);
                        m_fragment = rawClipboardText.Substring(startFragment, endFragment - startFragment);
                        break;

                    // Optional Source URL, used for resolving relative links.
                    case "sourceurl":
                        m_source = new System.Uri(val);
                        break;
                }
            } // end for

            if (m_fullText == null && m_fragment == null)
            {
                throw new FormatException("No data specified");
            }
        }

        #endregion Methods
    } // end of class
}