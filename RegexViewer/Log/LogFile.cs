// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-09-2015
// ***********************************************************************
// <copyright file="LogFile.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.ObjectModel;

namespace RegexViewer
{
    public class LogFile : BaseFile<LogFileItem>
    {
        #region Public Classes

        public class ExportConfigurationInfo
        {
            #region Public Fields

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

            #endregion Public Fields
        }

        #endregion Public Classes

        #region Public Constructors

        public LogFile()
        {
            ContentItems = new ObservableCollection<LogFileItem>();
            ExportConfiguration = new ExportConfigurationInfo();
        }

        #endregion Public Constructors

        #region Public Properties

        public override ObservableCollection<LogFileItem> ContentItems { get; set; }

        public ExportConfigurationInfo ExportConfiguration { get; set; }

        #endregion Public Properties

        public bool HasBom { get; set; }

        public System.Text.Encoding Encoding { get; set; }
    }
}