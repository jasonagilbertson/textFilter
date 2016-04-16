// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 09-06-2015 ***********************************************************************
// <copyright file="FilterTabViewModel.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************

namespace TextFilter
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        #region Private Fields

        private bool _maskedVisibility = TextFilterSettings.Settings.CountMaskedMatches;

        #endregion Private Fields

        #region Public Constructors

        public FilterTabViewModel()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        public bool MaskedVisibility
        {
            get
            {
                return _maskedVisibility;
            }
            set
            {
                if (_maskedVisibility != value)
                {
                    _maskedVisibility = value;
                    OnPropertyChanged("MaskedVisibility");
                }
            }
        }

        #endregion Public Properties
    }
}