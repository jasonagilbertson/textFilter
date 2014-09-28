using System;
using System.Data;
using System.Windows.Controls;

namespace RegexViewer
{
    //public class FilterViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class FilterViewModel : BaseViewModel<DataRow>
    {
        #region Public Methods

        public override void AddTabItem(IFileProperties<DataRow> fileProperties)
        {
            throw new NotImplementedException();
        }

        public override void OpenFile(object sender)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}