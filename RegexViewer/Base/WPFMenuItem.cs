using System;

namespace RegexViewer
{
    public class WPFMenuItem
    {
        #region Public Constructors

        public WPFMenuItem()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        public Command Command { get; set; }

        public String IconUrl { get; set; }

        public String Text { get; set; }

        #endregion Public Properties
    }
}