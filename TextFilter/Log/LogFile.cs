// ************************************************************************************
// Assembly: TextFilter
// File: LogFile.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.ObjectModel;

namespace TextFilter
{
    public class LogFile : BaseFile<LogFileItem>
    {
        public override ObservableCollection<LogFileItem> ContentItems { get; set; }

        public System.Text.Encoding Encoding { get; set; }

        public ExportConfigurationInfo ExportConfiguration { get; set; }

        public bool HasBom { get; set; }

        public LogFile()
        {
            ContentItems = new ObservableCollection<LogFileItem>();
            ExportConfiguration = new ExportConfigurationInfo();
        }

        public class ExportConfigurationInfo
        {
            public bool Cancel;

            public bool Content = true;

            public bool Copy;

            public bool Group1;

            public bool Group2;

            public bool Group3;

            public bool Group4;

            public bool Index;

            public bool RemoveEmpty = true;

            public string Separator = ",";
        }
    }
}