using System;
namespace RegexViewer
{
    interface IFileItem
    {
        System.Windows.Media.Brush Background { get; set; }
        string Content { get; set; }
        System.Windows.Media.Brush Foreground { get; set; }
    }
}
