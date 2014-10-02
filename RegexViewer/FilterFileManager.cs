using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace RegexViewer
{
    public class FilterFileManager : BaseFileManager<DataRow>
    {
        public FilterFileManager()
        {
           // SaveNewFilterTable();
            this.Files = new List<IFileProperties<DataRow>>();
        }

        private static void SaveNewFilterTable()
        {
            DataTable table = CreateNewFilterTable("XmlFilter");
            

            string fileName = "C:\\temp\\TestFilter.xml";
            table.WriteXml(fileName, XmlWriteMode.WriteSchema);

        
            
        }

        private static DataTable CreateNewFilterTable(string tableName)
        {
            // Create a test DataTable with two columns and a few rows.
            DataTable table = new DataTable(tableName);
            DataColumn column = new DataColumn("index", typeof(System.Int32));
            column.AutoIncrement = true;
            table.Columns.Add(column);

            column = new DataColumn("filterPattern", typeof(System.String));
            table.Columns.Add(column);
            column = new DataColumn("background", typeof(System.String));
            table.Columns.Add(column);
            column = new DataColumn("foreground", typeof(System.String));
            table.Columns.Add(column);
            column = new DataColumn("enabled", typeof(System.Boolean));
            table.Columns.Add(column);
            column = new DataColumn("exclude", typeof(System.Boolean));
            table.Columns.Add(column);


            // Add ten rows.
            DataRow row;
            for (int i = 0; i <= 9; i++)
            {
                row = table.NewRow();
                row["filterPattern"] = "pattern " + i;
                table.Rows.Add(row);
            }

            table.AcceptChanges();
            return table;
        }

        public override IFileProperties<DataRow> OpenFile(string LogName)
        {
            IFileProperties<DataRow> filterProperties = new FilterFileProperties();
            if (Files.Exists(x => String.Compare(x.Tag, LogName, true) == 0))
            {
                ts.TraceEvent(TraceEventType.Error, 1, "file already open:" + LogName);
                return filterProperties;
            }

            if (File.Exists(LogName))
            {
                DataTable dataTable = new DataTable();
                
                dataTable.ReadXml(LogName);
                filterProperties.ContentItems.AddRange(new List<DataRow>(dataTable.Select()));
                filterProperties.FileName = Path.GetFileName(LogName);
                filterProperties.Tag = LogName;

                //using (System.IO.StreamReader sr = new System.IO.StreamReader(LogName))
                //{
                //    string line;
                //    while ((line = sr.ReadLine()) != null)
                //    {
                //        // DataRow textBlock = new DataRow();
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
                ts.TraceEvent(TraceEventType.Error, 2, "log file does not exist:" + LogName);
                this.Settings.RemoveLogFile(LogName);
            }

            return filterProperties;
        }

        public override List<IFileProperties<DataRow>> OpenFiles(string[] files)
        {
            List<IFileProperties<DataRow>> textBlockItems = new List<IFileProperties<DataRow>>();

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


    }
}