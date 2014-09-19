using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    class FilterViewModel : INotifyPropertyChanged, RegexViewer.IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
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

        void IViewModel.OpenFile(object sender)
        {
            throw new NotImplementedException();
        }

        event PropertyChangedEventHandler IViewModel.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
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
    }
}
