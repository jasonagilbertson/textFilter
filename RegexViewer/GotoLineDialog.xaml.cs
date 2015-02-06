using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace RegexViewer
{
    /// <summary>
    /// Interaction logic for GotoLineDialog.xaml
    /// </summary>
    public partial class GotoLineDialog : Window
    {
        #region Private Fields

        
        
        
        #endregion Private Fields

        #region Public Constructors

        public GotoLineDialog()
        {
            InitializeComponent();
            textBoxLineNumber.Focus();
            
        }

        #endregion Public Constructors

        #region Public Events

        
        #endregion Public Events

        #region Public Enums

        #endregion Public Enums

        #region Public Properties

       

        #endregion Public Properties

        #region Public Methods

        public void Disable()
        {
            this.Hide();
        }

              
        public int WaitForResult()
        {
            this.ShowDialog();
            int result = 0;
            if(Int32.TryParse(textBoxLineNumber.Text,out result))
            {
                return result;
            }
            return 0;
        }

        #endregion Public Methods

        #region Private Methods

         #endregion Private Methods

        private void buttonGotoLine_Click(object sender, RoutedEventArgs e)
        {
            Disable();
            this.Close();
        }

        private void textBoxLineNumber_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                buttonGotoLine_Click(null, null);
            }
        }
    }
}