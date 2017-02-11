// ************************************************************************************
// Assembly: TextFilter
// File: FileTypeAssociation.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace TextFilter
{
    public class FileTypeAssociation : Base
    {
        private static FileTypeAssociation _fileTypeAssociation;

        private string[] _extensions = new string[4] { ".csv", ".log", ".rvf", ".rvconfig" };

        private string _keyName = Process.GetCurrentProcess().ProcessName;

        private string _openWith = Process.GetCurrentProcess().MainModule.FileName;

        static FileTypeAssociation()
        {
            if (_fileTypeAssociation == null)
            {
                _fileTypeAssociation = new FileTypeAssociation();
            }
        }

        public FileTypeAssociation()
        {
        }

        public static FileTypeAssociation Instance
        {
            get { return _fileTypeAssociation; }
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public bool ConfigureFTA(bool set)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("have to run elevated to set file type associations. exiting");
                //SetStatus("have to run elevated to set file type associations. exiting");
                return false;
            }
            // set path to programfiles
            string ftaFolder = Directory.GetCurrentDirectory();

            if (set)
            {
                // add
                SetContextMenu();
                foreach (string extension in _extensions)
                {
                    SetAssociation(extension, string.Format("{0}\\{1}", ftaFolder, System.AppDomain.CurrentDomain.FriendlyName));
                }
                SetStatus("registered using current configuration: " + ftaFolder);
                return true;
            }
            else
            {
                // remove
                UnSetContextMenu();
                foreach (string extension in _extensions)
                {
                    UnSetAssociation(extension);
                }
                return true;
            }
        }

        public bool CopyKey(RegistryKey parentKey, string keyNameToCopy, string newKeyName)
        {
            //Create new key
            RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName);

            //Open the sourceKey we are copying from
            RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy);

            RecurseCopyKey(sourceKey, destinationKey);

            return true;
        }

        public bool RenameSubKey(RegistryKey parentKey, string subKeyName, string newSubKeyName)
        {
            SetStatus(string.Format("RenameSubKey: enter:{0} to {1}", subKeyName, newSubKeyName));
            DeleteKey(parentKey, subKeyName);
            return true;
        }

        public void SetAssociation(string extension, string file = null)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RenameSubKey(Registry.ClassesRoot, extension, extension + "_back");
            SetStatus("SetAssociation:enter");
            BaseKey = Registry.ClassesRoot.CreateSubKey(extension);
            BaseKey.SetValue("", _keyName);

            OpenMethod = Registry.ClassesRoot.CreateSubKey(_keyName);
            OpenMethod.SetValue("", _keyName);//_fileDescription);

            if (!string.IsNullOrEmpty(file))
            {
                _openWith = file;
            }

            OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + _openWith + "\",0");
            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + _openWith + "\"" + " \"%1\"");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + _openWith + "\"" + " \"%1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        public void UnSetAssociation(string extension)
        {
            DeleteKey(Registry.ClassesRoot, _keyName);
            DeleteKey(Registry.ClassesRoot, extension);
            RenameSubKey(Registry.ClassesRoot, extension + "_back", extension);

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        private static void DeleteKey(RegistryKey key, string keyName)
        {
            if (key.OpenSubKey(keyName) != null)
            {
                key.Close();
                key.DeleteSubKeyTree(keyName);
            }
        }

        private void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, objValue, valKind);
            }

            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                RegistryKey sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName);
                RegistryKey destSubKey = destinationKey.CreateSubKey(sourceSubKeyName);
                RecurseCopyKey(sourceSubKey, destSubKey);
            }
        }

        private void SetContextMenu()
        {
            RegistryKey baseKey;

            SetStatus("SeContextMenu:enter");
            baseKey = Registry.ClassesRoot.CreateSubKey("*\\shell");
            if (baseKey.OpenSubKey(_keyName) != null)
            {
                UnSetContextMenu();
            }

            baseKey.CreateSubKey(_keyName).CreateSubKey("command").SetValue("", "\"" + _openWith + "\"" + " \"%1\"");

            baseKey.Close();
        }

        private void UnSetContextMenu()
        {
            RegistryKey baseKey;

            SetStatus("SeContextMenu:enter");
            baseKey = Registry.ClassesRoot.CreateSubKey("*\\shell");
            if (baseKey.OpenSubKey(_keyName) != null)
            {
                baseKey.DeleteSubKeyTree(_keyName);
            }

            baseKey.Close();
        }
    }
}