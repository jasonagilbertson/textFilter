// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 09-06-2015 ***********************************************************************
// <copyright file="ITabViewModel.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System.Collections.ObjectModel;

namespace TextFilter
{
    public interface ITabViewModel<T>
    {

        #region Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        string Background { get; set; }
        ObservableCollection<T> ContentList { get; set; }
        Command CopyCommand { get; set; }
        IFile<T> File { get; set; }
        string Header { get; set; }

        bool IsNew { get; set; }

        bool Modified { get; set; }

        string Name { get; set; }

        string Tag { get; set; }

        object Viewer { get; set; }

        #endregion Properties

        #region Methods
        
        void CopyExecuted(object target);

        void OnPropertyChanged(string name);

        void SelectionChangedExecuted(object target);

        #endregion Methods

    }
}