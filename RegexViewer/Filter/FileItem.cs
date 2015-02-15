using System.Windows.Media;

namespace RegexViewer
{
    public class FileItem : IFileItem
    {
        //private System.Windows.Visibility _visibility;
        #region Public Properties

        public Brush Background { get; set; }

        public string Content { get; set; }

        public FontFamily FontFamily { get; set; }

        public int FontSize { get; set; }

        public Brush Foreground { get; set; }
        //public System.Windows.Visibility Visibility 
        //{ 
        //    get
        //    {
        //        return _visibility;
        //    }
        //     set
        //    {
        //         if(_visibility != value)
        //         {
        //             _visibility = value;
        //           //  OnPropertyChanged("Visibility");

        //         }
        //    }
             
        //     }
        public int Index { get; set; }
        

        #endregion Public Properties

        #region Public Methods

        public IFileItem ShallowCopy()
        {
            return (IFileItem)this.MemberwiseClone();
        }

        #endregion Public Methods
    }
}