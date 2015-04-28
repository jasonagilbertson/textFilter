using System.Windows;

namespace RegexViewer
{
    public class BindingProxy : Freezable
    {
        #region Public Fields

        // Using a DependencyProperty as the backing store for Data. This enables animation,
        // styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        #endregion Public Fields

        #region Public Properties

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        #endregion Public Properties

        #region Protected Methods

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion Protected Methods
    }
}