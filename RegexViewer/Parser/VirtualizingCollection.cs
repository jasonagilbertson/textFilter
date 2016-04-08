// ***********************************************************************
// Assembly         : RegexViewer
// Author           : jason
// Created          : 09-06-2015
//
// Last Modified By : jason
// Last Modified On : 10-31-2015
// ***********************************************************************
// <copyright file="Virtualizingcollection.cs" company="http://www.codeproject.com/Articles/34405/WPF-Data-Virtualization">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace RegexViewer
{
    public class VirtualizingCollection<T> : IList<T>, IList
    {
        #region Private Fields

        private readonly IItemsProvider<T> _itemsProvider;

        private readonly Dictionary<int, IList<T>> _pages = new Dictionary<int, IList<T>>();

        private readonly int _pageSize = 100;

        private readonly long _pageTimeout = 10000;

        private readonly Dictionary<int, DateTime> _pageTouchTimes = new Dictionary<int, DateTime>();

        private int _count = -1;

        #endregion Private Fields

        #region Public Constructors

        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
        {
            _itemsProvider = itemsProvider;
            _pageSize = pageSize;
            _pageTimeout = pageTimeout;
        }

        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
        {
            _itemsProvider = itemsProvider;
            _pageSize = pageSize;
        }

        public VirtualizingCollection(IItemsProvider<T> itemsProvider)
        {
            _itemsProvider = itemsProvider;
        }

        #endregion Public Constructors

        #region Public Properties

        public virtual int Count
        {
            get
            {
                if (_count == -1)
                {
                    LoadCount();
                }
                return _count;
            }
            protected set
            {
                _count = value;
            }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public IItemsProvider<T> ItemsProvider
        {
            get { return _itemsProvider; }
        }

        public int PageSize
        {
            get { return _pageSize; }
        }

        public long PageTimeout
        {
            get { return _pageTimeout; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        #endregion Public Properties

        #region Public Indexers

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        public T this[int index]
        {
            get
            {
                // determine which page and offset within page
                int pageIndex = index / PageSize;
                int pageOffset = index % PageSize;

                // request primary page
                RequestPage(pageIndex);

                // if accessing upper 50% then request next page
                if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
                    RequestPage(pageIndex + 1);

                // if accessing lower 50% then request prev page
                if (pageOffset < PageSize / 2 && pageIndex > 0)
                    RequestPage(pageIndex - 1);

                // remove stale pages
                CleanUpPages();

                // defensive check in case of async load
                if (_pages[pageIndex] == null)
                    return default(T);

                // return requested item
                return _pages[pageIndex][pageOffset];
            }
            set { throw new NotSupportedException(); }
        }

        #endregion Public Indexers

        #region Public Methods

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void CleanUpPages()
        {
            List<int> keys = new List<int>(_pageTouchTimes.Keys);
            foreach (int key in keys)
            {
                // page 0 is a special case, since WPF ItemsControl access the first item frequently
                if (key != 0 && (DateTime.Now - _pageTouchTimes[key]).TotalMilliseconds > PageTimeout)
                {
                    _pages.Remove(key);
                    _pageTouchTimes.Remove(key);
                    Trace.WriteLine("Removed Page: " + key);
                }
            }
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion Public Methods

        #region Protected Methods

        protected int FetchCount()
        {
            return ItemsProvider.FetchCount();
        }

        protected IList<T> FetchPage(int pageIndex)
        {
            return ItemsProvider.FetchRange(pageIndex * PageSize, PageSize);
        }

        protected virtual void LoadCount()
        {
            Count = FetchCount();
        }

        protected virtual void LoadPage(int pageIndex)
        {
            PopulatePage(pageIndex, FetchPage(pageIndex));
        }

        protected virtual void PopulatePage(int pageIndex, IList<T> page)
        {
            Trace.WriteLine("Page populated: " + pageIndex);
            if (_pages.ContainsKey(pageIndex))
                _pages[pageIndex] = page;
        }

        protected virtual void RequestPage(int pageIndex)
        {
            if (!_pages.ContainsKey(pageIndex))
            {
                _pages.Add(pageIndex, null);
                _pageTouchTimes.Add(pageIndex, DateTime.Now);
                Trace.WriteLine("Added page: " + pageIndex);
                LoadPage(pageIndex);
            }
            else
            {
                _pageTouchTimes[pageIndex] = DateTime.Now;
            }
        }

        #endregion Protected Methods
    }
}