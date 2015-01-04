using System;
using System.ComponentModel;

namespace RegexViewer
{
    public class Base : INotifyPropertyChanged
    {
        #region Public Events

        public static event EventHandler<string> NewStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Methods

        //public static IMainViewModel MainModel;
        public void OnNewStatus(string status)
        {
            EventHandler<string> newStatus = NewStatus;
            if (newStatus != null)
            {
                newStatus(this, status);
            }
        }

        public void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void SetStatus(string status)
        {
            //MainModel.SetViewStatus(status);
            OnNewStatus(status);
        }

        #endregion Public Methods
    }
}