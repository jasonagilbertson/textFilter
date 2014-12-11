using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    public class Base: INotifyPropertyChanged
    {
     //public static IMainViewModel MainModel;
        #region Public Events

        public static event EventHandler<string> NewStatus;
        #endregion Public Events
        public void OnNewStatus(string status)
        {
            EventHandler<string> newStatus = NewStatus;
            if(newStatus != null)
            {
                newStatus(this, status);
            }
        }
        public void SetStatus(string status)
        {
            //MainModel.SetViewStatus(status);
            OnNewStatus(status);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


    }
}
