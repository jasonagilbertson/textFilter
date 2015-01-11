using System;
using System.Windows.Media;

namespace RegexViewer
{
    public class FileItem : IFileItem
    {
        #region Public Properties

        public Brush Background { get; set; }

        public string Content { get; set; }

        public FontFamily FontFamily { get; set; }

        public int FontSize { get; set; }

        public Brush Foreground { get; set; }

        public int Index { get; set; }

        #endregion Public Properties

        #region Public Methods

        public IFileItem ShallowCopy()
        {
            return (LogFileItem)this.MemberwiseClone();
        }

        #endregion Public Methods
    }
}