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

        /// <summary>
        /// The cancel command.
        /// </summary>
        private Command cancelCommand;

        /// <summary>
        /// Flag indicated that cancellation has been requested.
        /// </summary>
        private bool isCancellationRequested;

        /// <summary>
        /// Flag indicating that the command is executing.
        /// </summary>
        private bool isExecuting = false;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousCommand"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="canExecute">if set to <c>true</c> the command can execute.</param>
        public AsynchronousCommand(Action action, bool canExecute = true)
            : base(action, canExecute)
        {
            // Initialise the command.
            Initialise();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsynchronousCommand"/> class.
        /// </summary>
        /// <param name="parameterizedAction">The parameterized action.</param>
        /// <param name="canExecute">if set to <c>true</c> [can execute].</param>
        public AsynchronousCommand(Action<object> parameterizedAction, bool canExecute = true)
            : base(parameterizedAction, canExecute)
        {
            // Initialise the command.
            Initialise();
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when the command is cancelled.
        /// </summary>
        public event CommandEventHandler Cancelled;

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        public Command CancelCommand
        {
            get { return cancelCommand; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is cancellation requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is cancellation requested; otherwise, <c>false</c> .
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is executing.
        /// </summary>
        /// <value><c>true</c> if this instance is executing; otherwise, <c>false</c> .</value>
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

        /// <summary>
        /// Cancels the command if requested.
        /// </summary>
        /// <returns>True if the command has been cancelled and we must return.</returns>
        public bool CancelIfRequested()
        {
            // If we haven't requested cancellation, there's nothing to do.
            if (IsCancellationRequested == false)
                return false;

            // We're done.
            return true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="param">The param.</param>
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

        /// <summary>
        /// Reports progress on the thread which invoked the command.
        /// </summary>
        /// <param name="action">The action.</param>
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

        /// <summary>
        /// Invokes the cancelled event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Apex.MVVM.CommandEventArgs"/> instance containing the event data.
        /// </param>
        protected void InvokeCancelled(CommandEventArgs args)
        {
            CommandEventHandler cancelled = Cancelled;

            // Call the cancelled event.
            if (cancelled != null)
                cancelled(this, args);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Initialises this instance.
        /// </summary>
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

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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