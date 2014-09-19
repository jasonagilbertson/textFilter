using System;
namespace RegexViewer
{
    interface IViewModel
    {
        Command CloseCommand { get; set; }
        void CloseFile(object sender);
        void OnPropertyChanged(string name);
        Command OpenCommand { get; set; }
        void OpenFile(object sender);
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        System.Collections.ObjectModel.ObservableCollection<ItemViewModel> TabItems { get; set; }
    }
}
