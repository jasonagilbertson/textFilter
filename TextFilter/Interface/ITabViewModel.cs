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
        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        string Background { get; set; }
        IFile<T> File { get; set; }
        ObservableCollection<T> ContentList { get; set; }
        
        Command CopyCommand { get; set; }

        string Header { get; set; }

        bool IsNew { get; set; }

        bool Modified { get; set; }

        string Name { get; set; }

        string Tag { get; set; }

        object Viewer { get; set; }

        #endregion Public Properties

        #region Public Methods

        void CopyExecuted(object target);

        void OnPropertyChanged(string name);

        void SelectionChangedExecuted(object target);

        #endregion Public Methods
    }
}