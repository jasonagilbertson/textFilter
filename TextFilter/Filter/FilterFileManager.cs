// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-13-2015 ***********************************************************************
// <copyright file="FilterFileManager.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

//using System.Windows.Media;
using System.Xml;

namespace TextFilter
{
    public class FilterFileManager : BaseFileManager<FilterFileItem>
    {
        public FilterFileManager()
        {
            FileManager = new List<IFile<FilterFileItem>>();
        }

        public enum FilterFileVersionResult
        {
            Version1,

            Version2,

            NotAFilterFile
        }

        public FilterFileVersionResult FilterFileVersion(string fileName)
        {
            try
            {
                FilterFileVersionResult result = FilterFileVersionResult.NotAFilterFile;
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                XmlNode root = doc.DocumentElement;

                // for v2 documentelement is filterInfo
                if (root.Name.ToLower() == "filterinfo")
                {
                    result = FilterFileVersionResult.Version2;
                }
                else if (root.Name.ToLower() == "filters")
                {
                    result = FilterFileVersionResult.Version1;
                }

                if (root.Name.ToLower() != "filters")
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.Name.ToLower() == "filters")
                        {
                            root = node;
                            break;
                        }
                    }
                }

                if (root.Name.ToLower() != "filters")
                {
                    // not TextFilter filter
                    result = FilterFileVersionResult.NotAFilterFile;
                }

                SetStatus(string.Format("FilterFileVersion: filter: {0} version: {1}", fileName, result.ToString()));
                return result;
            }
            catch (Exception e)
            {
                SetStatus(string.Format("Exception: FilterFileVersion: filter: {0}, {1}", fileName, e));
                return FilterFileVersionResult.NotAFilterFile;
            }
        }

        public override IFile<FilterFileItem> ManageFileProperties(string LogName, IFile<FilterFileItem> filterFile)
        {
            filterFile.FileName = Path.GetFileName(LogName);
            filterFile.Tag = LogName;

            // todo rework this:
            ((FilterFile)filterFile).EnablePatternNotifications(false);
            ((FilterFile)filterFile).EnablePatternNotifications(true);
            ((FilterFile)filterFile).PropertyChanged += filterFile_PropertyChanged;
            return filterFile;
        }

        public void ManageFilterFileItem(FilterFile filterFile, int filterIndex = -1, bool remove = false)
        {
            // add blank new item so defaults / modifications can be set some type of bug
            IEnumerable<FilterFileItem> results = null;
            int indexMax = -1;

            SetStatus("ManageNewFilterFileItem:" + filterFile.FileName);

            results = filterFile.ContentItems.Where(x => x.Enabled == false
                    && x.Exclude == false
                    && x.Regex == false
                    && string.IsNullOrEmpty(x.Filterpattern)
                    && string.IsNullOrEmpty(x.Notes));

            if (filterFile.ContentItems.Count > 0)
            {
                // valid enabled filter count
                indexMax = filterFile.ContentItems.Max(x => x.Index);
            }

            if (results == null | results != null && results.Count() == 0 | filterIndex >= 0)
            {
                // no empty / new filter item or index ge 0 (insert or remove)
                FilterFileItem fileItem = new FilterFileItem();

                filterFile.EnablePatternNotifications(false);
                fileItem.Index = indexMax + 1;

                SetStatus("ManageNewFilterFileItem:adding new line");

                if (filterIndex >= 0 && !remove)
                {
                    // insert in new enabled filter item at specified index
                    fileItem.Enabled = true;
                    fileItem.Index = filterIndex;
                    filterFile.AddPatternNotification(fileItem, true);
                    filterFile.ContentItems.Insert(filterIndex, fileItem);
                }
                else if (filterIndex >= 0 && remove)
                {
                    // remove old filter item at specified index
                    filterFile.AddPatternNotification(fileItem, false);
                    filterFile.ContentItems.RemoveAt(filterIndex);
                }
                else
                {
                    filterFile.AddPatternNotification(fileItem, true);
                    filterFile.ContentItems.Add(fileItem);
                }

                filterFile.EnablePatternNotifications(true);
            }
            else if (results.Count() == 1)
            {
                if (results.ToList()[0].Index != indexMax)
                {
                    filterFile.EnablePatternNotifications(false);
                    results.ToList()[0].Index = indexMax + 1;
                    filterFile.EnablePatternNotifications(true);
                }

                return;
            }
            else
            {
                for (int i = 0; i < results.Count() - 1; i++)
                {
                    filterFile.ContentItems.Remove(results.ToList()[i]);
                }
            }
        }

        public override IFile<FilterFileItem> NewFile(string LogName, ObservableCollection<FilterFileItem> fileItems = null)
        {
            FilterFile filterFile = new FilterFile();
            ManageFilterFileItem(filterFile);

            FileManager.Add(ManageFileProperties(LogName, filterFile));

            Settings.AddFilterFile(LogName);
            OnPropertyChanged("FilterFileManager");
            return filterFile;
        }

        public override IFile<FilterFileItem> OpenFile(string fileName)
        {
            FilterFile filterFile = new FilterFile();
            SetStatus("OpenFile:enter: " + fileName);
            try
            {
                if (FileManager.Exists(x => String.Compare(x.Tag, fileName, true) == 0))
                {
                    SetStatus("file already open:" + fileName);
                    return filterFile;
                }

                filterFile = (FilterFile)ReadFile(fileName);
                ManageFilterFileItem(filterFile);

                ManageFileProperties(fileName, filterFile);
                FileManager.Add(filterFile);
                Settings.AddFilterFile(fileName);
                OnPropertyChanged("FilterFileManager");
                SetStatus("OpenFile:exit: " + fileName);
                return filterFile;
            }
            catch (Exception e)
            {
                SetStatus(string.Format("error opening filter file:{0}:{1}", fileName, e.ToString()));
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

                filterFileItems.Add(filterFile);
            }

            return filterFileItems;
        }

        public override IFile<FilterFileItem> ReadFile(string fileName)
        {
            FilterFile filterFile = new FilterFile();

            try
            {
                SetStatus("ReadFile:enter: " + fileName);

                filterFile.FileName = Path.GetFileName(fileName);
                if (Path.GetExtension(fileName).ToLower().Contains("tat"))
                {
                    filterFile.ContentItems = new ObservableCollection<FilterFileItem>(ReadTatFile(fileName));
                    filterFile.IsNew = false;
                    return filterFile;
                }

                List<FilterFileItem> filterFileItems = new List<FilterFileItem>();
                FilterFileVersionResult filterFileVersion = FilterFileVersion(fileName);

                if (filterFileVersion == FilterFileVersionResult.NotAFilterFile)
                {
                    return filterFile;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                // see if file is readonly
                try
                {
                    doc.Save(fileName);
                }
                catch
                {
                    filterFile.IsReadOnly = true;
                }

                XmlNode root = doc.DocumentElement;

                // for v2 documentelement is filterInfo
                if (filterFileVersion != FilterFileVersionResult.Version1)
                {
                    filterFile.FilterVersion = ReadStringNodeItem(root, "filterversion");
                    filterFile.FilterNotes = ReadStringNodeItem(root, "filternotes");
                }

                if (root.Name.ToLower() != "filters")
                {
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.Name.ToLower() == "filters")
                        {
                            root = node;
                            break;
                        }
                    }
                }

                filterFile.IsNew = false;

                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    FilterFileItem fileItem = new FilterFileItem();
                    fileItem.Count = 0;
                    fileItem.BackgroundColor = ReadStringNodeChildItem(root, "backgroundcolor", i);
                    fileItem.CaseSensitive = ReadBoolNodeChildItem(root, "casesensitive", i);
                    fileItem.Enabled = ReadBoolNodeChildItem(root, "enabled", i);
                    fileItem.Exclude = ReadBoolNodeChildItem(root, "exclude", i);
                    fileItem.Regex = ReadBoolNodeChildItem(root, "regex", i);
                    fileItem.Filterpattern = ReadStringNodeChildItem(root, "filterpattern", i);
                    fileItem.ForegroundColor = ReadStringNodeChildItem(root, "foregroundcolor", i);
                    fileItem.Index = ReadIntNodeChildItem(root, "index", i);
                    fileItem.Notes = ReadStringNodeChildItem(root, "notes", i);

                    filterFileItems.Add(fileItem);
                }

                filterFile.ContentItems = new ObservableCollection<FilterFileItem>(filterFileItems);
                SetStatus("ReadFile:exit: " + fileName);
                return filterFile;
            }
            catch (Exception e)
            {
                SetStatus("Fatal:Readfile:exception" + e.ToString());
                return filterFile;
            }
        }

        public override bool SaveFile(string fileName, IFile<FilterFileItem> file)
        {
            FilterFile filterFile = (FilterFile)file;
            filterFile.IsNew = false;
            SetStatus("SaveFile:enter: " + fileName);

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = filterFile.FileName;
            }

            try
            {
                SetStatus("saving file:" + fileName);

                if (Path.GetExtension(fileName).ToLower().Contains("tat"))
                {
                    if (SaveTatFile(fileName, filterFile.ContentItems))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                XmlTextWriter xmlw = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
                xmlw.Formatting = Formatting.Indented;
                xmlw.WriteStartDocument();
                xmlw.WriteStartElement("filterinfo");

                xmlw.WriteStartElement("filterversion");
                xmlw.WriteString(DateTime.Now.ToString("yyMMddss"));
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("filternotes");
                xmlw.WriteString(filterFile.FilterNotes);
                xmlw.WriteEndElement();

                xmlw.WriteStartElement("filters");

                foreach (FilterFileItem item in filterFile.ContentItems)
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

                    xmlw.WriteStartElement("casesensitive");
                    xmlw.WriteString(item.CaseSensitive.ToString());
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
                xmlw.WriteEndElement();
                xmlw.WriteEndDocument();

                xmlw.Close();
                SetStatus("SaveFile:exit: " + fileName);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                filterFile.IsReadOnly = true;
                SetStatus("Fatal:SaveFile:exception" + ex.ToString());
                return false;
            }
            catch (Exception e)
            {
                SetStatus("Fatal:SaveFile:exception: " + e.ToString());
                return false;
            }
        }

        private void filterFile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == FilterFileItemEvents.Count |
                e.PropertyName == FilterFileItemEvents.Notes |
                e.PropertyName == FilterFileItemEvents.MaskedCount)
            {
                // dont forward count updates
                return;
            }
            SetStatus("FilterFileManager:filterFile_PropertyChanged: " + e.PropertyName);

            OnPropertyChanged(sender, e);
        }

        private string FindColorName(string rgbColor)
        {
            SetStatus("FindColorName:" + rgbColor);

            if (!Regex.IsMatch(rgbColor, "[0-9A-Fa-f]{6}"))
            {
                SetStatus("FindColorName: invalid. returning black");
                return "black";
            }

            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            KnownColor[] allColors = new KnownColor[colorsArray.Length];
            Array.Copy(colorsArray, allColors, colorsArray.Length);

            Color newColor = System.Drawing.Color.FromArgb(0xff,
                                Convert.ToByte(rgbColor.Substring(0, 2), 16),
                                Convert.ToByte(rgbColor.Substring(2, 2), 16),
                                Convert.ToByte(rgbColor.Substring(4, 2), 16));

            foreach (KnownColor color in allColors)
            {
                Color tempColor = Color.FromKnownColor(color);
                if (!tempColor.IsSystemColor
                    && tempColor.R == newColor.R
                    && tempColor.G == newColor.G
                    && tempColor.B == newColor.B)
                {
                    SetStatus("FindColorName return:" + color.ToString());
                    return color.ToString();
                }
            }

            return "white";
        }

        private bool ReadAttributeBool(XmlNode node, string attName, int item)
        {
            try
            {
                // for tat files
                string val = (node.ChildNodes.Item(item).Attributes[attName].Value.ToString());
                if (val.ToLower() == "y" | val.ToLower() == "true")
                {
                    return true;
                }
                else if (val.ToLower() == "n" | val.ToLower() == "false")
                {
                    return false;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string ReadAttributeString(XmlNode node, string attName, int item)
        {
            try
            {
                return (node.ChildNodes.Item(item).Attributes[attName].Value.ToString());
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool ReadBoolNodeChildItem(XmlNode node, string nodeName, int item)
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

        private int ReadIntNodeChildItem(XmlNode node, string nodeName, int item)
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

        private string ReadStringNodeChildItem(XmlNode node, string nodeName, int item)
        {
            try
            {
                return (node.ChildNodes.Item(item).SelectSingleNode(nodeName)).InnerText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ReadStringNodeItem(XmlNode node, string nodeName)
        {
            try
            {
                return (node.SelectSingleNode(nodeName)).InnerText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private List<FilterFileItem> ReadTatFile(string fileName)
        {
            List<FilterFileItem> filterFileItems = new List<FilterFileItem>();

            try
            {
                SetStatus("ReadTatFile:enter: " + fileName);

                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                XmlNode root = doc.DocumentElement.ChildNodes[0];

                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    FilterFileItem fileItem = new FilterFileItem();
                    fileItem.Count = 0;

                    // for backward compatibility
                    if (!string.IsNullOrEmpty(ReadAttributeString(root, "color", i)))
                    {
                        fileItem.ForegroundColor = FindColorName(ReadAttributeString(root, "color", i));
                        fileItem.BackgroundColor = "White";
                    }
                    else
                    {
                        fileItem.BackgroundColor = FindColorName(ReadAttributeString(root, "backColor", i));
                        fileItem.ForegroundColor = FindColorName(ReadAttributeString(root, "foreColor", i));
                    }

                    fileItem.CaseSensitive = ReadAttributeBool(root, "case_sensitive", i);
                    fileItem.Enabled = ReadAttributeBool(root, "enabled", i);
                    fileItem.Exclude = ReadAttributeBool(root, "excluding", i);
                    fileItem.Regex = ReadAttributeBool(root, "regex", i);
                    fileItem.Filterpattern = ReadAttributeString(root, "text", i);
                    fileItem.TatType = ReadAttributeString(root, "type", i);

                    fileItem.Index = i; // ReadIntNodeItem(root, "index", i);
                    //fileItem.Notes = ReadStringNodeItem(root, "notes", i);

                    filterFileItems.Add(fileItem);
                }
                return filterFileItems;
            }
            catch (Exception e)
            {
                SetStatus("ReadTatFile:exception: " + e.ToString());
                return filterFileItems;
            }
        }

        private bool SaveTatFile(string fileName, ObservableCollection<FilterFileItem> fileItems)
        {
            try
            {
                // check version of tat file for color differences
                SetStatus("SaveTatFile:enter: " + fileName);

                // first version supporting foreColor and backColor
                string currentVersion = string.Empty;
                bool legacyVersion = false;

                // read file and try to get version
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    currentVersion = doc.DocumentElement.GetAttribute("version");
                }
                catch { }

                if (string.IsNullOrEmpty(currentVersion))
                {
                    MessageBoxResult mbResult = MessageBox.Show("Do you want to save Tat filter as new version that supports background color?\nTextAnalysisTool.NET builds before 2015 do not support background color.", "TatVersion", MessageBoxButton.YesNo);
                    if (mbResult != MessageBoxResult.Yes)
                    {
                        legacyVersion = true;
                        currentVersion = "2014-04-22";
                    }
                    else
                    {
                        legacyVersion = false;
                        currentVersion = "2015-01-28";
                    }
                }
                else if (currentVersion.Contains("2015"))
                {
                    legacyVersion = false;
                    currentVersion = "2015-01-28";
                }
                else
                {
                    legacyVersion = true;
                    currentVersion = "2014-04-22";
                }

                XmlTextWriter xmlw = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
                xmlw.Formatting = Formatting.Indented;
                xmlw.WriteStartDocument();

                xmlw.WriteStartElement("TextAnalysisTool.NET");
                xmlw.WriteAttributeString("version", currentVersion);
                xmlw.WriteAttributeString("showOnlyFilteredLines", "False");

                xmlw.WriteStartElement("filters");

                foreach (FilterFileItem item in fileItems.OrderBy(x => x.Index))
                {
                    xmlw.WriteStartElement("filter");

                    xmlw.WriteAttributeString("enabled", item.Enabled ? "y" : "n");
                    xmlw.WriteAttributeString("excluding", item.Exclude ? "y" : "n");

                    string fColor = item.Foreground.ToString();
                    fColor = fColor.Substring(fColor.Length - 6);

                    string bColor = item.Background.ToString();
                    bColor = bColor.Substring(bColor.Length - 6);

                    if (legacyVersion)
                    {
                        // for legacy
                        xmlw.WriteAttributeString("color", fColor);
                    }
                    else
                    {
                        // for newest version
                        xmlw.WriteAttributeString("foreColor", fColor);
                        xmlw.WriteAttributeString("backColor", bColor);
                    }

                    // tat does not support this setting xmlw.WriteAttributeString("notes", item.Notes.ToString());

                    // dont currently support marker and is not saved in TextFilter filter file
                    xmlw.WriteAttributeString("type", item.TatType ?? "matches_text");

                    // dont currently support 'y' in gui
                    xmlw.WriteAttributeString("case_sensitive", item.CaseSensitive ? "y" : "n");
                    xmlw.WriteAttributeString("regex", item.Regex ? "y" : "n");
                    xmlw.WriteAttributeString("text", item.Filterpattern);

                    xmlw.WriteEndElement();
                }

                xmlw.WriteEndElement();
                xmlw.WriteEndDocument();

                xmlw.Close();
                SetStatus("SaveTatFile:exit: " + fileName);
                return true;
            }
            catch (Exception e)
            {
                SetStatus("SaveAsTat exception:" + e.ToString());
                return false;
            }
        }
    }
}