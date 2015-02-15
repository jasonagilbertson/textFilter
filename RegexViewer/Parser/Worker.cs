using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexViewer
{
    public class Worker 
    {
        FilterFile _filterFile;
        LogFile _logFile;
        private void Work(FilterFile filterFile, LogFile logFile)
        {
            _filterFile = filterFile;
            _logFile = logFile;
        }
    }
 
}
