using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RegexViewer
{
    public class FileTypeAssociation : Base
    {
        #region Private Fields

        private static FileTypeAssociation _fileTypeAssociation;
        private string _extension;

        private string _extensionBackup;

        private string _fileDescription;

        private string _hkcuKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\";

        private string _hkcuKeyExt;

        private string _keyName = Process.GetCurrentProcess().ProcessName;

        private string _openWith = Process.GetCurrentProcess().MainModule.FileName;

        #endregion Private Fields

        #region Public Constructors

        static FileTypeAssociation()
        {
            if (_fileTypeAssociation == null)
            {
                _fileTypeAssociation = new FileTypeAssociation();
            }
        }

        public FileTypeAssociation()
        {
            _extension = ".csv";
            _extensionBackup = _extension + "_back";
            _fileDescription = "RegexViewer";

            _hkcuKeyExt = _hkcuKey + _extension;
        }

        #endregion Public Constructors

        #region Public Properties

        public static FileTypeAssociation Instance
        {
            get { return _fileTypeAssociation; }
        }

        #endregion Public Properties

        #region Public Methods

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public bool ConfigureFTA(bool set)
        {
            // set path to programfiles
            string ftaFolder = Directory.GetCurrentDirectory();

            if (set)
            {
                // add
                SetAssociation(string.Format("{0}\\{1}", ftaFolder, System.AppDomain.CurrentDomain.FriendlyName));
                SetStatus("registered using current configuration: " + ftaFolder);
                return true;
            }
            else
            {
                // remove

                UnSetAssociation();
                return true;
            }
        }

        public bool CopyKey(RegistryKey parentKey,
            string keyNameToCopy, string newKeyName)
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

        public void SetAssociation(string file = null)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RenameSubKey(Registry.ClassesRoot, _extension, _extensionBackup);
            SetStatus("SetAssociation:enter");
            BaseKey = Registry.ClassesRoot.CreateSubKey(_extension);
            BaseKey.SetValue("", _keyName);

            OpenMethod = Registry.ClassesRoot.CreateSubKey(_keyName);
            OpenMethod.SetValue("", _fileDescription);

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

        public void UnSetAssociation()
        {
            DeleteKey(Registry.ClassesRoot, _keyName);
            DeleteKey(Registry.ClassesRoot, _extension);
            RenameSubKey(Registry.ClassesRoot, _extensionBackup, _extension);

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        #endregion Public Methods

        #region Private Methods

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

        #endregion Private Methods
    }
}