using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace RegexViewer
{
    public class FilterFileManager : BaseFileManager<FilterFileItems>
    {
        #region Public Constructors

        public FilterFileManager()
        {
            // CreateNewFilterTable("testFilter.xml");
            this.Files = new List<IFileProperties<FilterFileItems>>();
        }

        #endregion Public Constructors

        #region Public Methods

        public override IFileProperties<FilterFileItems> OpenFile(string LogName)
        {
            IFileProperties<FilterFileItems> filterProperties = new FilterFileProperties();

            try
            {
                if (Files.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
                {
                    ts.TraceEvent(TraceEventType.Error, 1, "file already open:" + LogName);
                    return filterProperties;
                }

                if (File.Exists(LogName))
                {
                    XmlDocument doc = new XmlDocument();

                    doc.Load(LogName);
                    XmlNode root = doc.DocumentElement;
                    for (int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        FilterFileItems filterFileItems = new FilterFileItems();
                        filterFileItems.Index = Convert.ToInt32((root.ChildNodes.Item(i).SelectSingleNode("index")).InnerXml);
                        filterFileItems.Background = (root.ChildNodes.Item(i).SelectSingleNode("background")).InnerXml;
                        filterFileItems.Foreground = (root.ChildNodes.Item(i).SelectSingleNode("foreground")).InnerXml;
                        filterFileItems.Enabled = Convert.ToBoolean((root.ChildNodes.Item(i).SelectSingleNode("enabled")).InnerXml);
                        filterFileItems.Exclude = Convert.ToBoolean((root.ChildNodes.Item(i).SelectSingleNode("exclude")).InnerXml);
                        filterFileItems.FilterPattern = (root.ChildNodes.Item(i).SelectSingleNode("filterPattern")).InnerXml;
                        filterProperties.ContentItems.Add(filterFileItems);
                    }

                    //myNode.Value = "blabla";
                    //doc.Save("D:\\build.xml");
                    //XmlNode root = doc.DocumentElement["Filters"];
                    //root.FirstChild.InnerText = "Filter";
                    //XmlNode root1 = doc.DocumentElement["index"];
                    //root1.FirstChild.InnerText = "Second";
                    //doc.Save(@"C:\WINDOWS\Temp\exm.xml");
                    //filterProperties.ContentItems.AddRange(new List<FilterFileItems>(dataTable.Select()));
                    //  filterProperties.ContentItems.AddRange(dataTable.AsEnumerable());

                    filterProperties.FileName = Path.GetFileName(LogName);
                    filterProperties.Tag = LogName;

                    //using (System.IO.StreamReader sr = new System.IO.StreamReader(LogName))
                    //{
                    //    string line;
                    //    while ((line = sr.ReadLine()) != null)
                    //    {
                    //        // FilterFileItems textBlock = new FilterFileItems();
                    //        //textBlock.Content = line;
                    //        //textBlock.Background = Settings.BackgroundColor;
                    //        //textBlock.Foreground = Settings.FontColor;
                    //        //textBlock.FontSize = Settings.FontSize;
                    //        //textBlock.FontFamily = new System.Windows.Media.FontFamily("Courier");
                    //        //  filterProperties.ContentItems.Add(textBlock);
                    //    }
                    //}

                    Files.Add(filterProperties);
                    this.Settings.AddFilterFile(LogName);
                }
                else
                {
                    ts.TraceEvent(TraceEventType.Error, 2, "filter file does not exist:" + LogName);
                    this.Settings.RemoveLogFile(LogName);
                }

                return filterProperties;
            }
            catch (Exception e)
            {
                ts.TraceEvent(TraceEventType.Error, 2, string.Format("error opening filter file:{0}:{1}", LogName, e.ToString()));
                return filterProperties;
            }
        }

        public override List<IFileProperties<FilterFileItems>> OpenFiles(string[] files)
        {
            List<IFileProperties<FilterFileItems>> textBlockItems = new List<IFileProperties<FilterFileItems>>();

            foreach (string file in files)
            {
                FilterFileProperties logProperties = new FilterFileProperties();
                if (String.IsNullOrEmpty((logProperties = (FilterFileProperties)OpenFile(file)).Tag))
                {
                    continue;
                }

                textBlockItems.Add(logProperties);
            }

            return textBlockItems;
        }

        public override bool SaveFile(string FileName, List<FilterFileItems> fileItems)
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            XmlTextWriter xmlw = new XmlTextWriter(FileName, System.Text.Encoding.UTF8);
            xmlw.Formatting = Formatting.Indented;
            xmlw.WriteStartDocument();
            xmlw.WriteStartElement("filters");

            foreach (FilterFileItems item in fileItems)
            {
                xmlw.WriteStartElement("filter");

                xmlw.WriteStartElement("filterPattern");
                xmlw.WriteString(item.FilterPattern);
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("background");
                xmlw.WriteString(item.Background);
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("foreground");
                xmlw.WriteString(item.Foreground);
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("index");
                xmlw.WriteString(item.Index.ToString());
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("enabled");
                xmlw.WriteString(item.Enabled.ToString());
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("exclude");
                xmlw.WriteString(item.Exclude.ToString());
                xmlw.WriteEndElement();

                xmlw.WriteEndElement();
            }

            xmlw.WriteEndElement();
            xmlw.WriteEndDocument();

            xmlw.Close();

            return true;
        }

        #endregion Public Methods

        #region Private Methods

        private static void CreateNewFilterTable(string name)
        {
            XmlTextWriter xmlw = new XmlTextWriter(name, System.Text.Encoding.UTF8);
            xmlw.WriteStartDocument();
            xmlw.WriteStartElement("filters");
            xmlw.WriteStartElement("filter");
            xmlw.WriteStartElement("filterPattern");
            xmlw.WriteString("test pattern");
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("background");
            xmlw.WriteString("black");
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("foreground");
            xmlw.WriteString("cyan");
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("index");
            xmlw.WriteString("0");
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("enabled");
            xmlw.WriteString("true");
            xmlw.WriteEndElement();
            xmlw.WriteStartElement("exclude");
            xmlw.WriteString("false");
            xmlw.WriteEndElement();
            xmlw.WriteEndElement();
            xmlw.WriteEndElement();
            xmlw.WriteEndDocument();
            xmlw.Formatting = Formatting.Indented;
            xmlw.Close();
        }

        #endregion Private Methods
    }
}