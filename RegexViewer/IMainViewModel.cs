using System;
namespace RegexViewer
{
    public interface IMainViewModel
    {
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void SetViewStatus(string statusData);
    }
}
