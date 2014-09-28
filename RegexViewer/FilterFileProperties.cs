using System.Collections.Generic;
using System.Data;
namespace RegexViewer
{
    public class FilterFileProperties: BaseFileProperties<DataRow>
    {
          public FilterFileProperties()
        {
            this.Dirty = false;
            this.ContentItems = new List<DataRow>();
        }

        
        

        public override List<DataRow> ContentItems { get; set; }

    }
}