using System.Collections;
using System.Collections.Specialized;

namespace Utils.Collections;

public class ReorderableCollection<T> : IList<T>, IList, ICollection<T>, INotifyCollectionChanged
{
    public delegate void ElementInserted(ReorderableCollection<T> collection, T item, int index);
    public delegate void ElementRemoved(ReorderableCollection<T> collection, T item, int oldIndex);

    #region Collection Changed Events
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    private void CollectionChangedInvoke(NotifyCollectionChangedAction action, T item, int index)
        => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
    private void CollectionChangedInvoke(NotifyCollectionChangedAction action, T item)
        => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
    private void CollectionChangedInvoke(NotifyCollectionChangedAction action)
        => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
    #endregion
    public event ElementInserted? OnElementInserted;
    public event ElementRemoved? OnElementRemoved;

    private List<T> items = [];

    public int Count => items.Count;

    #region Internal Interface Impls
    bool IList.IsReadOnly => (items as IList).IsReadOnly;
    bool ICollection<T>.IsReadOnly => (items as IList).IsReadOnly;
    bool IList.IsFixedSize => (items as IList).IsFixedSize;
    bool ICollection.IsSynchronized => (items as IList).IsSynchronized;
    object ICollection.SyncRoot => (items as IList).SyncRoot;
    #endregion

    object? IList.this[int index] { get => this[index]; set => this[index] = (T)value!; }
    public T this[int index] { get => items[index]; set => items[index] = value; }

    public ReorderableCollection() { }
    public ReorderableCollection(IEnumerable<T> values) { items = [.. values]; }

    public void Insert(int index, T item)
    {
        items.Insert(index, item);
        OnElementInserted?.Invoke(this, item, index);
        CollectionChangedInvoke(NotifyCollectionChangedAction.Add, item, index);
    }
    void IList.Insert(int index, object? value)
    {
        if (value is not T item) return;
        Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        var item = items[index];
        items.RemoveAt(index);
        OnElementRemoved?.Invoke(this, item, index);
        CollectionChangedInvoke(NotifyCollectionChangedAction.Remove, item, index);
    }

    public void Add(T item)
    {
        items.Add(item);
        CollectionChangedInvoke(NotifyCollectionChangedAction.Add, item, items.Count - 1);
    }
    int IList.Add(object? value)
    {
        if (value is not T item) return -1;

        Add(item);
        return Count - 1;
    }

    public void Clear()
    {
        items.Clear();
        CollectionChangedInvoke(NotifyCollectionChangedAction.Replace);
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;

        items.Remove(item);
        CollectionChangedInvoke(NotifyCollectionChangedAction.Remove, item, index);
        return true;
    }
    void IList.Remove(object? value)
    {
        if (value is not T item) return;
        Remove(item);
    }

    public int IndexOf(T item) => items.IndexOf(item);
    public bool Contains(T item) => items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    bool IList.Contains(object? value) => (items as IList).Contains(value);
    int IList.IndexOf(object? value) => (items as IList).IndexOf(value);
    void ICollection.CopyTo(Array array, int index) => (items as IList).CopyTo(array, index);
}
