using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace RegexViewer
{
    
    public class FilterViewModel : BaseViewModel<DataRow>
    {
        #region Public Methods

        public FilterViewModel()
        {
            this.TabItems = new ObservableCollection<ITabViewModel>();
            this.FileManager = new FilterFileManager();
        }
        public override void AddTabItem(IFileProperties<DataRow> logProperties)
        {

            if (!this.TabItems.Any(x => String.Compare((string)x.Tag, logProperties.Tag, true) == 0))
            {
                FilterTabViewModel tabItem = new FilterTabViewModel();
                // tabItem.MouseRightButtonDown += tabItem_MouseRightButtonDown;
                tabItem.Name = this.TabItems.Count.ToString();
                tabItem.ContentList = ((FilterFileProperties)logProperties).ContentItems;
                tabItem.Tag = logProperties.Tag;
                tabItem.Header = logProperties.FileName;
                TabItems.Add(tabItem);
            }
        }

        public override void OpenFile(object sender)
        {
            string logName = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Xml Files (*.xml)|*.xml|All Files (*.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                logName = dlg.FileName;
            }

            FilterFileProperties logProperties = new FilterFileProperties();
            if (String.IsNullOrEmpty((logProperties = (FilterFileProperties)this.FileManager.OpenFile(logName)).Tag))
            {
                return;
            }

            // make new tab
            AddTabItem(logProperties);
        }

        #endregion Public Methods
    }
}