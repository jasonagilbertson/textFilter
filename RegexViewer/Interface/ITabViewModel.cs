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
        //bool Group1Visibility { get; set; }
        //bool Group2Visibility { get; set; }
        Command CopyCommand { get; set; }

        string Header { get; set; }

        bool Modified { get; set; }

        string Name { get; set; }

        Command PasteCommand { get; set; }

        string Tag { get; set; }

        object Viewer { get; set; }

        #endregion Public Properties

        #region Public Methods

        void CopyExecuted(object target);

        void OnPropertyChanged(string name);

        void PasteText();

        void SelectionChangedExecuted(object target);

        #endregion Public Methods
    }
}