// ************************************************************************************
// Assembly: TextFilter
// File: ITabViewModel.cs
// Created: 12/24/2016
// Modified: 2/11/2017
// Copyright (c) 2017 jason gilbertson
//
// ************************************************************************************

using System.Collections.ObjectModel;

namespace TextFilter
{
    public interface ITabViewModel<T>
    {
        string Background { get; set; }

        ObservableCollection<T> ContentList { get; set; }

        Command CopyCommand { get; set; }

        //bool Group1Visibility { get; set; }
        //bool Group2Visibility { get; set; }
        string Header { get; set; }

        bool IsNew { get; set; }

        bool Modified { get; set; }

        string Name { get; set; }

        string Tag { get; set; }

        object Viewer { get; set; }

        void CopyExecuted(object target);

        void OnPropertyChanged(string name);

        void SelectionChangedExecuted(object target);

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}