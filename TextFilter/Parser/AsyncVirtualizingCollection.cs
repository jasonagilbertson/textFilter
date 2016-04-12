// ***********************************************************************
// Assembly         : TextFilter
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-31-2015
// ***********************************************************************
// <copyright file="AsyncVirtualizingCollection.cs" company="http://www.codeproject.com/Articles/34405/WPF-Data-Virtualization">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************


using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace TextFilter
{
    public class AsyncVirtualizingCollection<T> : VirtualizingCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Private Fields

        private readonly SynchronizationContext _synchronizationContext;

        private bool _isLoading;

        #endregion Private Fields

        #region Public Constructors

        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider)
            : base(itemsProvider)
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
            : base(itemsProvider, pageSize)
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
            : base(itemsProvider, pageSize, pageTimeout)
        {
            _synchronizationContext = SynchronizationContext.Current;
        }

        #endregion Public Constructors

        #region Public Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (value != _isLoading)
                {
                    _isLoading = value;
                }
                FirePropertyChanged("IsLoading");
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
        }

        #endregion Protected Properties

        #region Protected Methods

        protected override void LoadCount()
        {
            Count = 0;
            IsLoading = true;
            ThreadPool.QueueUserWorkItem(LoadCountWork);
        }

        protected override void LoadPage(int index)
        {
            IsLoading = true;
            ThreadPool.QueueUserWorkItem(LoadPageWork, index);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
                h(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        #endregion Protected Methods

        #region Private Methods

        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        private void FirePropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
        }

        private void LoadCountCompleted(object args)
        {
            Count = (int)args;
            IsLoading = false;
            FireCollectionReset();
        }

        private void LoadCountWork(object args)
        {
            int count = FetchCount();
            SynchronizationContext.Send(LoadCountCompleted, count);
        }

        private void LoadPageCompleted(object args)
        {
            int pageIndex = (int)((object[])args)[0];
            IList<T> page = (IList<T>)((object[])args)[1];

            PopulatePage(pageIndex, page);
            IsLoading = false;
            FireCollectionReset();
        }

        private void LoadPageWork(object args)
        {
            int pageIndex = (int)args;
            IList<T> page = FetchPage(pageIndex);
            SynchronizationContext.Send(LoadPageCompleted, new object[] { pageIndex, page });
        }

        #endregion Private Methods
    }
}