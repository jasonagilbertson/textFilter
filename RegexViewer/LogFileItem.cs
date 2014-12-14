using System.Windows.Controls;

namespace RegexViewer
{
    public class LogFileItem : TextBlock, IFileItem
    {
        #region Public Properties

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

        #endregion Public Properties

        #region Public Methods

        public IFileItem ShallowCopy()
        {
            return (LogFileItem)this.MemberwiseClone();
        }

        #endregion Public Methods
    }
}