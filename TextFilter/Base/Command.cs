// *********************************************************************** Assembly : TextFilter
// Author : jason Created : 09-06-2015
//
// Last Modified By : jason Last Modified On : 10-13-2015 ***********************************************************************
// <copyright file="Command.cs" company="">
//     Copyright © 2015
// </copyright>
// <summary>
// </summary>
// ***********************************************************************
using System;
using System.Windows.Input;

namespace TextFilter
{
    public delegate void CancelCommandEventHandler(object sender, CancelCommandEventArgs args);

    public delegate void CommandEventHandler(object sender, CommandEventArgs args);

    public class CancelCommandEventArgs : CommandEventArgs
    {

        #region Properties

        public bool Cancel { get; set; }

        #endregion Properties

    }

    public class Command : ICommand
    {

        #region Fields

        protected Action _action = null;

        protected Action<object> _parameterizedAction = null;

        private bool _canExecute = false;

        #endregion Fields

        #region Constructors

        public Command(Action action, bool canExecute = true)
        {
            // Set the action.
            _action = action;
            _canExecute = canExecute;
        }

        public Command(Action<object> parameterizedAction, bool canExecute = true)
        {
            // Set the action.
            _parameterizedAction = parameterizedAction;
            _canExecute = canExecute;
        }

        #endregion Constructors

        #region Events

        public event EventHandler CanExecuteChanged;

        public event CommandEventHandler Executed;

        public event CancelCommandEventHandler Executing;

        #endregion Events

        #region Properties

        public bool CanExecute
        {
            get { return _canExecute; }
            set
            {
                if (_canExecute != value)
                {
                    _canExecute = value;
                    EventHandler canExecuteChanged = CanExecuteChanged;
                    if (canExecuteChanged != null)
                        canExecuteChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion Properties

        #region Methods

        public virtual void DoExecute(object param)
        {
            // Invoke the executing command, allowing the command to be cancelled.
            CancelCommandEventArgs args = new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            InvokeExecuting(args);

            // If the event has been cancelled, bail now.
            if (args.Cancel)
                return;

            // Call the action or the parameterized action, whichever has been set.
            InvokeAction(param);

            // Call the executed function.
            InvokeExecuted(new CommandEventArgs() { Parameter = param });
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _canExecute;
        }

        void ICommand.Execute(object parameter)
        {
            DoExecute(parameter);
        }

        protected void InvokeAction(object param)
        {
            Action theAction = _action;
            Action<object> theParameterizedAction = _parameterizedAction;
            if (theAction != null)
                theAction();
            else if (theParameterizedAction != null)
                theParameterizedAction(param);
        }

        protected void InvokeExecuted(CommandEventArgs args)
        {
            CommandEventHandler executed = Executed;

            // Call the executed event.
            if (executed != null)
                executed(this, args);
        }

        protected void InvokeExecuting(CancelCommandEventArgs args)
        {
            CancelCommandEventHandler executing = Executing;

            // Call the executed event.
            if (executing != null)
                executing(this, args);
        }

        #endregion Methods

    }

    public class CommandEventArgs : EventArgs
    {

        #region Properties

        public object Parameter { get; set; }

        #endregion Properties

    }
}