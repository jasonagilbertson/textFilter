using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegexViewer
{
    public class FilterFileItems : BaseFileItems<FilterFileItem>
    {
        #region Public Constructors

        public FilterFileItems()
        {

            this.ContentItems = new ObservableCollection<FilterFileItem>();
        }

        #endregion Public Constructors

        #region Public Properties

        public override ObservableCollection<FilterFileItem> ContentItems { get; set; }

        #endregion Public Properties
    }
}