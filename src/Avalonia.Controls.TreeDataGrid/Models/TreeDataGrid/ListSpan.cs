using System;
using System.Collections;

namespace Avalonia.Controls.Models.TreeDataGrid;

/// <summary>
/// Exposes a range of an existing list as a read-only <see cref="IList"/>.
/// </summary>
/// <remarks>
/// This is a window onto the underlying list, not a copy: it is used to hand a range of rows to
/// a collection changed event without allocating, so its contents follow any later change to
/// that list. Consumers must read it while handling the event.
/// </remarks>
internal class ListSpan : IList
{
    private readonly IList _items;
    private readonly int _index;
    private readonly int _count;

    public ListSpan(IList items, int index, int count)
    {
        _items = items;
        _index = index;
        _count = count;
    }

    public object? this[int index]
    {
        get
        {
            if (index >= _count)
                throw new ArgumentOutOfRangeException();
            return _items[_index + index];
        }
        set => throw new NotSupportedException();
    }

    bool IList.IsFixedSize => true;
    bool IList.IsReadOnly => true;
    int ICollection.Count => _count;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    public IEnumerator GetEnumerator()
    {
        for (var i = 0; i < _count; ++i)
            yield return _items[_index + i];
    }

    int IList.Add(object? value) => throw new NotSupportedException();
    void IList.Clear() => throw new NotSupportedException();
    bool IList.Contains(object? value) => throw new NotSupportedException();
    void ICollection.CopyTo(Array array, int index) => throw new NotSupportedException();
    int IList.IndexOf(object? value) => throw new NotSupportedException();
    void IList.Insert(int index, object? value) => throw new NotSupportedException();
    void IList.Remove(object? value) => throw new NotSupportedException();
    void IList.RemoveAt(int index) => throw new NotSupportedException();
}
