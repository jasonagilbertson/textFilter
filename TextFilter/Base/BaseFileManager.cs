// ************************************************************************************
// Assembly: TextFilter
// File: BaseFileManager.cs
// Created: 9/6/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TextFilter
{
    public abstract class BaseFileManager<T> : Base, IFileManager<T>
    {
        public TextFilterSettings Settings = TextFilterSettings.Settings;

        public List<IFile<T>> FileManager { get; set; }

        public BaseFileManager()
        {
        }

        public bool CloseFile(string FileName)
        {
            try
            {
                if (FileManager.Exists(x => String.Compare(x.Tag, FileName, true) == 0))
                {
                    SetStatus("file open. removing:" + FileName);
                    FileManager.Remove(FileManager.Find(x => String.Compare(x.Tag, FileName, true) == 0));
                    if (typeof(T) == typeof(FilterFileItem))
                    {
                        Settings.RemoveFilterFile(FileName);
                    }
                    if (typeof(T) == typeof(LogFileItem))
                    {
                        Settings.RemoveLogFile(FileName);
                    }
                    return true;
                }
                else
                {
                    SetStatus("file not open:" + FileName);

                    return false;
                }
            }
            catch (Exception e)
            {
                SetStatus("CloseFile exception:" + e.ToString());
                return false;
            }
        }

        public abstract IFile<T> ManageFileProperties(string LogName, IFile<T> items = null);

        public abstract IFile<T> NewFile(string LogName, ObservableCollection<T> items);

        public abstract IFile<T> OpenFile(string LogName);

        public abstract List<IFile<T>> OpenFiles(string[] files);

        public abstract IFile<T> ReadFile(string LogName);

        public abstract bool SaveFile(string FileName, IFile<T> file);
    }
}