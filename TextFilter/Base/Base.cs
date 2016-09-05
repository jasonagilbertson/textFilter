// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-25-2015 ***********************************************************************
// <copyright file="Base.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TextFilter
{
    public class Base : INotifyPropertyChanged
    {
        public enum CurrentStatusSetting
        {
            enter_to_filter,
            filtered,
            quick_filtered,
            showing_all,
            filtering,
        }

        #region Public Fields

        #region Fields

        public string _tempTabNameFormat = "-new {0}-";
        public string _tempTabNameFormatPattern = @"\-new [0-9]{1,2}\-";

        #endregion Fields

        public static FilterViewModel _FilterViewModel { get; set; }
        public static LogViewModel _LogViewModel { get; set; }
        public static Parser _Parser { get; set; }
        #region Events

        public static event EventHandler<string> NewCurrentStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Methods

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

        public void ExecuteAsAdmin(string fileName, string arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
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

        public void OnPropertyChanged(string name)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (_transitioning)
            //{
            //    return;
            //}

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public void SetCurrentStatus(CurrentStatusSetting status)
        {
            EventHandler<string> newCurrentStatus = NewCurrentStatus;
            if (newCurrentStatus != null)
            {
                newCurrentStatus(this, Enum.GetName(typeof(CurrentStatusSetting), status).Replace("_", " ").ToUpper());
            }
        }

        public void SetStatus(string status)
        {
            if (status.ToLower().StartsWith("fatal:"))
            {
                MessageBox.Show(status, "Oh Snap! TextFilter exception", MessageBoxButton.OK);
            }

            try
            {
                if (!string.IsNullOrEmpty(TextFilterSettings.Settings.DebugFile))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(TextFilterSettings.Settings.DebugFile, true))
                    {
                        file.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("hh:mm:ss.fff"), status));
                    }
                }

                Debug.Print(status);
            }
            catch (Exception e)
            {
                Debug.Print(string.Format("SetStatus:exception: {0}: {1}", status, e));
            }
        }

        #endregion Methods

    }
}