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
            this.FileManager = new List<IFile<FilterFileItem>>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void NewFile()
        {
        }

        public override IFile<FilterFileItem> OpenFile(string LogName)
        {
            FilterFile filterFile = new FilterFile();
            
            try
            {
                if (FileManager.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
                {
                    
                    SetStatus("file already open:" + LogName);
                    return filterFile;
                }

                if (File.Exists(LogName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(LogName);

                    XmlNode root = doc.DocumentElement;

                    for (int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        FilterFileItem fileItem = new FilterFileItem();
                        fileItem.Count = 0;
                        fileItem.BackgroundColor = ReadStringNodeItem(root, "backgroundcolor", i);
                        fileItem.Enabled = ReadBoolNodeItem(root, "enabled", i);
                        fileItem.Exclude = ReadBoolNodeItem(root, "exclude", i);
                        fileItem.Regex = ReadBoolNodeItem(root, "regex", i);
                        fileItem.Filterpattern = ReadStringNodeItem(root, "filterpattern", i);
                        fileItem.ForegroundColor = ReadStringNodeItem(root, "foregroundcolor", i);
                        fileItem.Index = ReadIntNodeItem(root, "index", i);
                        fileItem.Notes = ReadStringNodeItem(root, "notes", i);

                        filterFile.ContentItems.Add(fileItem);
                    }

                    
                    filterFile.FileName = Path.GetFileName(LogName);
                    filterFile.Tag = LogName;
                    filterFile.EnablePatternNotifications(true);
                    filterFile.RebuildRegex();
                    filterFile.PropertyChanged += filterFile_PropertyChanged;
                    FileManager.Add(filterFile);
                    this.Settings.AddFilterFile(LogName);
                }
                else
                {
                    SetStatus("filter file does not exist:" + LogName);
                    this.Settings.RemoveLogFile(LogName);
                }

                return filterFile;
            }
            catch (Exception e)
            {
                SetStatus(string.Format("error opening filter file:{0}:{1}", LogName, e.ToString()));
                return filterFile;
            }
        }

        public override List<IFile<FilterFileItem>> OpenFiles(string[] files)
        {
            List<IFile<FilterFileItem>> filterFileItems = new List<IFile<FilterFileItem>>();

            foreach (string file in files)
            {
                FilterFile filterFile = new FilterFile();
                if (String.IsNullOrEmpty((filterFile = (FilterFile)OpenFile(file)).Tag))
                {
                    continue;
                }
                filterFile.PropertyChanged += filterFile_PropertyChanged;
                filterFileItems.Add(filterFile);
            }

            return filterFileItems;
        }

        void filterFile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("ContentList");
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