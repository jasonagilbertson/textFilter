using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace RegexViewer
{
    public class LogFileItem : TextBlock, IFileItem
    {

        public IFileItem ShallowCopy()
        {
            return (LogFileItem)this.MemberwiseClone();
        }
        public string Content
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }
    }
}
