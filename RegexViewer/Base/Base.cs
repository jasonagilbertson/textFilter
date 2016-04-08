// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-25-2015
// ***********************************************************************
// <copyright file="Base.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace RegexViewer
{
    public class Base : INotifyPropertyChanged
    {
        #region Public Fields

        public bool _transitioning;

        public string _tempTabNameFormat = "-new {0}-";

        public string _tempTabNameFormatPattern = @"\-new [0-9]{1,2}\-";

        #endregion Public Fields

        #region Public Events

        public static event EventHandler<string> NewStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Methods

        public void CreateProcess(string process, string arguments = null)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.UseShellExecute = true;
                processInfo.CreateNoWindow = false;
                if (!string.IsNullOrEmpty(arguments))
                {
                    processInfo.Arguments = arguments;
                }
                processInfo.FileName = process;
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                SetStatus("CreateProcess: exception" + e.ToString());
            }
        }

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

        public StringBuilder FormatExportItem(bool fileItemEnabled, string separator, bool removeEmpty, string fileItemValue, StringBuilder sb)
        {
            if (fileItemEnabled)
            {
                if (removeEmpty && String.IsNullOrEmpty(fileItemValue))
                {
                    return sb;
                }

                sb.Append((sb.Length > 0 ? separator : "") + fileItemValue);
            }

            return sb;
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
            if(_transitioning)
            {
                return;
            }

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void SetStatus(string status)
        {
            if (status.ToLower().StartsWith("fatal:"))
            {
                MessageBox.Show(status, "Oh Snap! regexViewer exception", MessageBoxButton.OK);
            }

            OnNewStatus(status);
        }

        #endregion Public Methods
    }
}