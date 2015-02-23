using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace RegexViewer
{
    public class Base : INotifyPropertyChanged
    {
        #region Public Events

        public static event EventHandler<string> NewStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Methods

        public T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        public T GetFirstChildByType<T>(DependencyObject prop) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(prop); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild((prop), i) as DependencyObject;
                if (child == null)
                    continue;

                T castedProp = child as T;
                if (castedProp != null)
                    return castedProp;

                castedProp = GetFirstChildByType<T>(child);

                if (castedProp != null)
                    return castedProp;
            }
            return null;
        }

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
            OnNewStatus(status);
        }

        #endregion Public Methods
    }
}