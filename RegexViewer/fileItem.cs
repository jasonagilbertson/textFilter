using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace RegexViewer
{
    public class FileItem : IFileItem
    {
        public string Content { get; set; }
        public Brush Background { get; set; }
        public Int64 Index { get; set; }
        public Brush Foreground { get; set; }
        public int FontSize { get; set; }
        public IFileItem ShallowCopy()
        {
            return (LogFileItem)this.MemberwiseClone();
        }
        public FontFamily FontFamily { get; set; }

    }
}
