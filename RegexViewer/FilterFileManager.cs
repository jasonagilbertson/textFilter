using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Xml;

namespace RegexViewer
{
    public class FilterFileManager : BaseFileManager<FilterFileItem>
    {
        #region Public Constructors

        public FilterFileManager()
        {
            this.ListFileItems = new List<IFileItems<FilterFileItem>>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void NewFile()
        {
        }

        public override IFileItems<FilterFileItem> OpenFile(string LogName)
        {
            IFileItems<FilterFileItem> filterProperties = new FilterFileItems();

            try
            {
                if (ListFileItems.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
                {
                    
                    SetStatus("file already open:" + LogName);
                    return filterProperties;
                }

                if (File.Exists(LogName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(LogName);

                    XmlNode root = doc.DocumentElement;

                    for (int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        FilterFileItem filterFileItems = new FilterFileItem();
                        filterFileItems.Count = 0;
                        filterFileItems.BackgroundColor = ReadStringNodeItem(root, "backgroundcolor", i);
                        filterFileItems.Enabled = ReadBoolNodeItem(root, "enabled", i);
                        filterFileItems.Exclude = ReadBoolNodeItem(root, "exclude", i);
                        filterFileItems.Regex = ReadBoolNodeItem(root, "regex", i);
                        filterFileItems.Filterpattern = ReadStringNodeItem(root, "filterpattern", i);
                        filterFileItems.ForegroundColor = ReadStringNodeItem(root, "foregroundcolor", i);
                        filterFileItems.Index = ReadIntNodeItem(root, "index", i);
                        filterFileItems.Notes = ReadStringNodeItem(root, "notes", i);

                        filterProperties.ContentItems.Add(filterFileItems);
                    }

                    filterProperties.FileName = Path.GetFileName(LogName);
                    filterProperties.Tag = LogName;

                    ListFileItems.Add(filterProperties);
                    this.Settings.AddFilterFile(LogName);
                }
                else
                {
                    SetStatus("filter file does not exist:" + LogName);
                    this.Settings.RemoveLogFile(LogName);
                }

                return filterProperties;
            }
            catch (Exception e)
            {
                SetStatus(string.Format("error opening filter file:{0}:{1}", LogName, e.ToString()));
                return filterProperties;
            }
        }

        public override List<IFileItems<FilterFileItem>> OpenFiles(string[] files)
        {
            List<IFileItems<FilterFileItem>> textBlockItems = new List<IFileItems<FilterFileItem>>();

            foreach (string file in files)
            {
                FilterFileItems logProperties = new FilterFileItems();
                if (String.IsNullOrEmpty((logProperties = (FilterFileItems)OpenFile(file)).Tag))
                {
                    continue;
                }

                textBlockItems.Add(logProperties);
            }

            return textBlockItems;
        }

        public override bool SaveFile(string FileName, ObservableCollection<FilterFileItem> fileItems)
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            SetStatus("saving file:" + FileName);

            XmlTextWriter xmlw = new XmlTextWriter(FileName, System.Text.Encoding.UTF8);
            xmlw.Formatting = Formatting.Indented;
            xmlw.WriteStartDocument();
            xmlw.WriteStartElement("filters");

            foreach (FilterFileItem item in fileItems)
            {
                xmlw.WriteStartElement("filter");

                xmlw.WriteStartElement("filterpattern");
                xmlw.WriteString(item.Filterpattern);
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("backgroundcolor");
                xmlw.WriteString(item.BackgroundColor);
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("foregroundcolor");
                xmlw.WriteString(item.ForegroundColor);
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

                xmlw.WriteStartElement("regex");
                xmlw.WriteString(item.Regex.ToString());
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("notes");
                xmlw.WriteString(item.Notes.ToString());
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

        private bool ReadBoolNodeItem(XmlNode node, string nodeName, int item)
        {
            try
            {
                return Convert.ToBoolean((node.ChildNodes.Item(item).SelectSingleNode(nodeName)).InnerXml);
            }
            catch
            {
                return false;
            }
        }

        private int ReadIntNodeItem(XmlNode node, string nodeName, int item)
        {
            try
            {
                return Convert.ToInt32((node.ChildNodes.Item(item).SelectSingleNode(nodeName)).InnerXml);
            }
            catch
            {
                return 0;
            }
        }

        private string ReadStringNodeItem(XmlNode node, string nodeName, int item)
        {
            try
            {
                return (node.ChildNodes.Item(item).SelectSingleNode(nodeName)).InnerXml;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion Private Methods
    }
}