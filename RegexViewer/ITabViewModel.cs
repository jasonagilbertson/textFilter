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
        //public string ActiveTab { get; set; }
        ObservableCollection<T> ContentList { get; set; }
        //List<T> UnFilteredContentList { get; set; }
        Command CopyCommand { get; set; }
        // Command SelectionChanged { get; set; }
        Command PasteCommand { get; set; }

        string Header { get; set; }

        string Name { get; set; }
        bool Modified { get; set; }
        string Tag { get; set; }
     //   string ActiveTab { get; set; }

        #endregion Public Properties

        #region Public Methods
        void PasteText();
        
        void CopyExecuted(object target);
        void SelectionChangedExecuted(object target);

        void OnPropertyChanged(string name);

        #endregion Public Methods
    }
}