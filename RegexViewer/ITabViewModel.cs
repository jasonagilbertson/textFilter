using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegexViewer
{
    public interface ITabViewModel<T>
    {
        #region Public Events

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        string Background { get; set; }

        ObservableCollection<T> ContentList { get; set; }

        Command CopyCommand { get; set; }
        Command PasteCommand { get; set; }

        string Header { get; set; }

        string Name { get; set; }

        string Tag { get; set; }

        #endregion Public Properties

        #region Public Methods
        void PasteText();
        
       // void CopyExecuted(object target);

        void OnPropertyChanged(string name);

        #endregion Public Methods
    }
}