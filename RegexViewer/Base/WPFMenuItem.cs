using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    public class WPFMenuItem
    {
        public String Text { get; set; }
        public String IconUrl { get; set; }
        //public List<WPFMenuItem> Children { get; private set; }
        public Command Command { get; set; }
        public WPFMenuItem()
        {
            //Text = item;
            //Children = new List<WPFMenuItem>();
        }
    }
}
