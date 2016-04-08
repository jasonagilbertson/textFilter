// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 09-06-2015
// ***********************************************************************
// <copyright file="AsynchronousCommand.cs" company="http://www.codeproject.com/Articles/274982/Commands-in-MVVM">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace RegexViewer
{
    public class AsynchronousCommand : Command, INotifyPropertyChanged
    {
        #region Protected Fields

        protected Dispatcher callingDispatcher;

        #endregion Protected Fields

        #region Private Fields

        private Command cancelCommand;

        private bool isCancellationRequested;

        private bool isExecuting = false;

        #endregion Private Fields

        #region Public Constructors

        public AsynchronousCommand(Action action, bool canExecute = true)
            : base(action, canExecute)
        {
            // Initialise the command.
            Initialise();
        }

        public AsynchronousCommand(Action<object> parameterizedAction, bool canExecute = true)
            : base(parameterizedAction, canExecute)
        {
            // Initialise the command.
            Initialise();
        }

        #endregion Public Constructors

        #region Public Events

        public event CommandEventHandler Cancelled;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public Command CancelCommand
        {
            get { return cancelCommand; }
        }

        public bool IsCancellationRequested
        {
            get
            {
                return isCancellationRequested;
            }
            set
            {
                if (isCancellationRequested != value)
                {
                    isCancellationRequested = value;
                    NotifyPropertyChanged("IsCancellationRequested");
                }
            }
        }

        public bool IsExecuting
        {
            get
            {
                return isExecuting;
            }
            set
            {
                if (isExecuting != value)
                {
                    isExecuting = value;
                    NotifyPropertyChanged("IsExecuting");
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public bool CancelIfRequested()
        {
            // If we haven't requested cancellation, there's nothing to do.
            if (IsCancellationRequested == false)
                return false;

            // We're done.
            return true;
        }

        public override void DoExecute(object param)
        {
            // If we are already executing, do not continue.
            if (IsExecuting)
                return;

            // Invoke the executing command, allowing the command to be cancelled.
            CancelCommandEventArgs args = new CancelCommandEventArgs() { Parameter = param, Cancel = false };
            InvokeExecuting(args);

            // If the event has been cancelled, bail now.
            if (args.Cancel)
                return;

            // We are executing.
            IsExecuting = true;

            // Store the calling dispatcher.
#if !SILVERLIGHT
            callingDispatcher = Dispatcher.CurrentDispatcher;
#else
      callingDispatcher = System.Windows.Application.Current.RootVisual.Dispatcher;
#endif

            // Run the action on a new thread from the thread pool (this will therefore work in SL
            // and WP7 as well).
            ThreadPool.QueueUserWorkItem(
              (state) =>
              {
                  // Invoke the action.
                  InvokeAction(param);

                  // Fire the executed event and set the executing state.
                  ReportProgress(
                    () =>
                    {
                        // We are no longer executing.
                        IsExecuting = false;

                        // If we were cancelled, invoke the cancelled event - otherwise invoke executed.
                        if (IsCancellationRequested)
                            InvokeCancelled(new CommandEventArgs() { Parameter = param });
                        else
                            InvokeExecuted(new CommandEventArgs() { Parameter = param });

                        // We are no longer requesting cancellation.
                        IsCancellationRequested = false;
                    }
                  );
              }
            );
        }

        public void ReportProgress(Action action)
        {
            if (IsExecuting)
            {
                if (callingDispatcher.CheckAccess())
                    action();
                else
                    callingDispatcher.BeginInvoke(((Action)(() => { action(); })));
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected void InvokeCancelled(CommandEventArgs args)
        {
            CommandEventHandler cancelled = Cancelled;

            // Call the cancelled event.
            if (cancelled != null)
                cancelled(this, args);
        }

        #endregion Protected Methods

        #region Private Methods

        private void Initialise()
        {
            // Construct the cancel command.
            cancelCommand = new Command(
              () =>
              {
                  // Set the Is Cancellation Requested flag.
                  IsCancellationRequested = true;
              }, true);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            // Store the event handler - in case it changes between the line to check it and the
            // line to fire it.
            PropertyChangedEventHandler propertyChanged = PropertyChanged;

            // If the event has been subscribed to, fire it.
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private Methods
    }
}