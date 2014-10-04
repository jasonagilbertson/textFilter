using System.Collections.Generic;

namespace RegexViewer
{
    public class FilterFileProperties : BaseFileProperties<FilterFileItems>
    {
        #region Public Constructors

        public FilterFileProperties()
        {
            this.Dirty = false;
            this.ContentItems = new List<FilterFileItems>();
        }

        #endregion Public Constructors

        #region Public Properties

        public override List<FilterFileItems> ContentItems { get; set; }

        #endregion Public Properties
    }
}