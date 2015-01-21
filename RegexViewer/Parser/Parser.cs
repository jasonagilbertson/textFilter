using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    class Parser: Base
    {
        private FilterViewModel _filterViewModel;
        private LogViewModel _logViewModel;
        List<IFile<FilterFileItem>> _filterFiles = new List<IFile<FilterFileItem>>();
        List<IFile<LogFileItem>> _logFiles = new List<IFile<LogFileItem>>();
        List<BackgroundParser> _parsers = new List<BackgroundParser>();


        //List<LogFile> _logFiles = new List<LogFile>();
        bool _filterMonitoringEnabled = false;
       // bool _logMonitoringEnabled = false;
        public Parser(FilterViewModel filterViewModel, LogViewModel logViewModel)
        {
            // TODO: Complete member initialization
            this._filterViewModel = filterViewModel;
            this._logViewModel = logViewModel;
            _filterFiles = (List<IFile<FilterFileItem>>)_filterViewModel.ViewManager.FileManager;
            //_filterViewModel.ViewManager.PropertyChanged += filterViewManager_PropertyChanged;
            _filterViewModel.PropertyChanged += filterViewManager_PropertyChanged;

            _logFiles = (List<IFile<LogFileItem>>)_logViewModel.ViewManager.FileManager;
            //_logViewModel.ViewManager.PropertyChanged += logViewManager_PropertyChanged;
            _logViewModel.PropertyChanged += logViewManager_PropertyChanged;

            this.EnableFilterFileMonitoring(true);
            //this.EnableLogFileMonitoring(true);
            
        }

        void filterViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:filterViewPropertyChanged");
        }

        void logViewManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetStatus("Parser:logViewPropertyChanged");
        }


        public void EnableFilterFileMonitoring(bool enable)
        {
            if (enable & !_filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in _filterFiles)
                {
                    item.ContentItems.CollectionChanged += filterItems_CollectionChanged;
                }
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }
            else if(!enable & _filterMonitoringEnabled)
            {
                foreach (IFile<FilterFileItem> item in _filterFiles)
                {
                    item.ContentItems.CollectionChanged -= filterItems_CollectionChanged;
                }
                _filterMonitoringEnabled = !_filterMonitoringEnabled;
            }

            
        }

        //public void EnableLogFileMonitoring(bool enable)
        //{
        //    if (enable & !_logMonitoringEnabled)
        //    {
        //        foreach (IFile<LogFileItem> item in _logFiles)
        //        {
        //            item.ContentItems.CollectionChanged += logItems_CollectionChanged;
        //        }
        //        _logMonitoringEnabled = !_logMonitoringEnabled;
        //    }
        //    else if (!enable & _logMonitoringEnabled)
        //    {
        //        foreach (IFile<LogFileItem> item in _logFiles)
        //        {
        //            item.ContentItems.CollectionChanged -= logItems_CollectionChanged;
        //        }
        //        _logMonitoringEnabled = !_logMonitoringEnabled;
        //    }

            
        //}

        void filterItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:filterItemsCollectionChanged");
        }

        void logItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetStatus("Parser:logItemsCollectionChanged");
        }
    }
}
