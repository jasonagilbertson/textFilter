using System;
using System.ComponentModel;

namespace RegexViewer
{
    //public class FilterViewModel : MainViewModel, INotifyPropertyChanged, RegexViewer.IViewModel
    public class FilterViewModel : INotifyPropertyChanged, RegexViewer.IViewModel
    {
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler IViewModel.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion Public Events

        #region Public Properties

        Command IViewModel.CloseCommand
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Command IViewModel.OpenCommand
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int IViewModel.SelectedIndex
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        System.Collections.ObjectModel.ObservableCollection<ItemViewModel> IViewModel.TabItems
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion Public Properties

        #region Public Methods

        void IViewModel.CloseFile(object sender)
        {
            throw new NotImplementedException();
        }

        void IViewModel.OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        void IViewModel.OpenFile(object sender)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}