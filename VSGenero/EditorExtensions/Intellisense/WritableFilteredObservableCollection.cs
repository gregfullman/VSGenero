using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class WritableFilteredObservableCollection<T> : IList, ICollection, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged
    {
        private delegate void AddRangeCallback(IList<T> items);
        private List<T> _filteredList;
        private Predicate<T> _filterPredicate;
        private bool _isFiltering;
        private IList<T> _underlyingList;
        private object _filterLock = new object();
        private Dispatcher _dispatcher;
        private int _rangeOperationCount;
        private bool _collectionChangedDuringRangeOperation;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public WritableFilteredObservableCollection(IList<T> underlyingList)
        {
            this._filteredList = new List<T>();
            if (underlyingList == null)
            {
                throw new ArgumentNullException("underlyingList");
            }
            if (!(underlyingList is INotifyCollectionChanged))
            {
                throw new ArgumentException("Underlying collection must implement INotifyCollectionChanged", "underlyingList");
            }
            if (!(underlyingList is IList))
            {
                throw new ArgumentException("Underlying collection must implement IList", "underlyingList");
            }
            this._underlyingList = underlyingList;
            ((INotifyCollectionChanged)this._underlyingList).CollectionChanged += OnUnderlyingList_CollectionChanged;
        }

        private void BeginBulkOperation()
        {
            this._rangeOperationCount++;
        }

        private void EndBulkOperation()
        {
            if (((this._rangeOperationCount > 0) && (--this._rangeOperationCount == 0)) && this._collectionChangedDuringRangeOperation)
            {
                //this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                DoUpdate();
                this._collectionChangedDuringRangeOperation = false;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                //if (this._dispatcher.CheckAccess())
                //{
                try
                {
                    this.BeginBulkOperation();
                    foreach (T local in items)
                    {
                        Add(local);
                    }
                }
                finally
                {
                    this.EndBulkOperation();
                }
                //this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                //}
                //else
                //{
                //    this._dispatcher.BeginInvoke(DispatcherPriority.Send, new AddRangeCallback(this.AddRange), items);
                //}
            }
        }

        public int Add(object value)
        {
            if (!(value is T))
            {
                throw new ArgumentException("Incorrect object type", "value");
            }
            T item = (T)value;
            lock (_filterLock)
            {
                _underlyingList.Add(item);
            }
            return _underlyingList.Count - 1;
        }

        public void Add(T item)
        {
            lock (_filterLock)
            {
                _underlyingList.Add(item);
            }
        }

        public void Clear()
        {
            lock (_filterLock)
            {
                _underlyingList.Clear();
            }
        }

        public bool Contains(object value)
        {
            return this.Contains((T)value);
        }

        public bool Contains(T item)
        {
            if (this._isFiltering)
            {
                return this._filteredList.Contains(item);
            }
            return this._underlyingList.Contains(item);
        }

        public void CopyTo(Array array, int index)
        {
            if (this._isFiltering)
            {
                if ((array.Length - index) < this.Count)
                {
                    throw new ArgumentException("Array not big enough", "array");
                }
                int num = index;
                foreach (T local in this._filteredList)
                {
                    array.SetValue(local, num);
                    num++;
                }
            }
            else
            {
                ((IList)this._underlyingList).CopyTo(array, index);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (this._isFiltering)
            {
                this._filteredList.CopyTo(array, arrayIndex);
            }
            else
            {
                this._underlyingList.CopyTo(array, arrayIndex);
            }
        }

        public void Filter(Predicate<T> filterPredicate)
        {
            if (filterPredicate == null)
            {
                throw new ArgumentNullException("filterPredicate");
            }
            lock (_filterLock)
            {
                this._filterPredicate = filterPredicate;
                this._isFiltering = true;
                this.UpdateFilteredItems();
                this.RaiseCollectionChanged();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this._isFiltering)
            {
                return this._filteredList.GetEnumerator();
            }
            return this._underlyingList.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        public int IndexOf(T item)
        {
            if (this._isFiltering)
            {
                return this._filteredList.IndexOf(item);
            }
            return this._underlyingList.IndexOf(item);
        }

        public void Insert(int index, object value)
        {
            if (!(value is T))
            {
                throw new ArgumentException("Incorrect object type", "value");
            }
            T item = (T)value;
            // TODO: inserting into a specific index isn't really going to work...need to figure out how this would be used.
            lock (_filterLock)
            {
                _underlyingList.Insert(index, item);
            }
        }

        public void Insert(int index, T item)
        {
            // TODO: inserting into a specific index isn't really going to work...need to figure out how this would be used.
            lock (_filterLock)
            {
                _underlyingList.Insert(index, item);
            }
        }

        private void DoUpdate()
        {
            this.UpdateFilteredItems();
            this.RaiseCollectionChanged();
        }

        private void OnUnderlyingList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._rangeOperationCount == 0)
            {
                DoUpdate();
            }
            else
            {
                this._collectionChangedDuringRangeOperation = true;
            }
        }

        private void RaiseCollectionChanged()
        {
            NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
            if (collectionChanged != null)
            {
                collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool Remove(T item)
        {
            lock (_filterLock)
            {
                return _underlyingList.Remove(item);
            }
        }

        public void Remove(object value)
        {
            if (!(value is T))
            {
                throw new ArgumentException("Incorrect object type", "value");
            }
            T item = (T)value;
            // TODO: inserting into a specific index isn't really going to work...need to figure out how this would be used.
            lock (_filterLock)
            {
                _underlyingList.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_filterLock)
            {
                _underlyingList.RemoveAt(index);
            }
        }

        public void StopFiltering()
        {
            if (this._isFiltering)
            {
                this._filterPredicate = null;
                this._isFiltering = false;
                this.UpdateFilteredItems();
                this.RaiseCollectionChanged();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this._isFiltering)
            {
                return this._filteredList.GetEnumerator();
            }
            return this._underlyingList.GetEnumerator();
        }

        private void UpdateFilteredItems()
        {
            this._filteredList.Clear();
            if (this._isFiltering)
            {
                for (int i = 0; i < _underlyingList.Count; i++)
                {
                    if (this._filterPredicate(_underlyingList[i]))
                    {
                        this._filteredList.Add(_underlyingList[i]);
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                if (this._isFiltering)
                {
                    return this._filteredList.Count;
                }
                return this._underlyingList.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                if (this._isFiltering)
                {
                    return this._filteredList[index];
                }
                return this._underlyingList[index];
            }
            set
            {
                lock (_filterLock)
                {
                    _underlyingList[index] = value;
                }
            }
        }

        public object SyncRoot
        {
            get
            {
                if (this._isFiltering)
                {
                    return ((ICollection)this._filteredList).SyncRoot;
                }
                return ((IList)this._underlyingList).SyncRoot;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (!(value is T))
                {
                    throw new ArgumentException("Incorrect object type", "value");
                }
                T item = (T)value;
                lock (_filterLock)
                {
                    _underlyingList[index] = item;
                }
            }
        }
    }
}
