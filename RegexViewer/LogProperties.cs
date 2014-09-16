using System.Collections.Generic;
using System.Windows.Controls;

namespace RegexViewer
{
    public class LogProperties
    {
        public LogProperties()
        {
            this.Dirty = false;
        }

        public string Name { get; set; }

        public string FileName { get; set; }

        public List<TextBlock> TextBlocks { get; set; }

        public bool Dirty { get; set; }
    }
}