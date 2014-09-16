using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RegexViewer
{
    
        public class Command : ICommand
        {
            //http://stackoverflow.com/questions/12422945/how-to-bind-wpf-button-to-a-command-in-viewmodelbase
            private readonly Action executeAction;

            private bool enabled;
            public bool Enabled
            {
                get
                {
                    return enabled;
                }
                set
                {
                    if (enabled != value)
                    {
                        enabled = value;

                        if (CanExecuteChanged != null)
                            CanExecuteChanged(this, new EventArgs());
                    }
                }
            }

            public Command(Action executeAction)
            {
                this.executeAction = executeAction;
            }

            public bool CanExecute(object parameter)
            {
                return enabled;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                executeAction();
            }
        }
    }
