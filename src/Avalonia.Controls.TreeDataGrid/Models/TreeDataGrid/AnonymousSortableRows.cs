using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Utils;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid;

/// <summary>
/// Exposes a sortable collection of models as anonymous rows.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <remarks>
/// In a flat grid where rows cannot be resized, it is not necessary to persist any information
/// about rows; the same row object can be updated and reused when a new row is requested.
/// </remarks>
public sealed class AnonymousSortableRows<TModel> : ReadOnlyListBase<IRow<TModel>>, IRows, IDisposable
{
    private readonly AnonymousRow<TModel> _row;
    private TreeDataGridItemsSourceView<TModel> _items;
    private IComparer<TModel>? _comparer;
    private Func<TModel, bool>? _filter;
    private List<int>? _sortedIndexes;

    public override IRow<TModel> this[int index]
    {
        get
        {
            if (_comparer is null && _filter is null)
            {
                return _row.Update(index, _items[index]);
            }
            if (_sortedIndexes is null)
            {
                RebuildSortedIndexes();
            }
            var modelIndex = _sortedIndexes![index];
            return _row.Update(modelIndex, _items[modelIndex]);
        }
    }

    IRow IReadOnlyList<IRow>.this[int index] => this[index];
    public override int Count => _sortedIndexes?.Count ?? _items.Count;
    public bool IsFiltered => _filter is not null;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public AnonymousSortableRows(TreeDataGridItemsSourceView<TModel> items, IComparer<TModel>? comparer)
    {
        _items = items;
        _items.CollectionChanged += OnItemsCollectionChanged;
        _comparer = comparer;
        _row = new AnonymousRow<TModel>();
    }

    public void Dispose()
    {
        SetItems(TreeDataGridItemsSourceView<TModel>.Empty);
        GC.SuppressFinalize(this);
    }

    public (int index, double y) GetRowAt(double y)
    {
        // Rows in an AnonymousSortableRows collection have Auto height so we only
        // know the start position of the first row.
        if (MathUtilities.IsZero(y))
            return (0, 0);
        return (-1, -1);
    }

    public override IEnumerator<IRow<TModel>> GetEnumerator()
    {
        for (var i = 0; i < Count; ++i)
        {
            yield return this[i];
        }
    }

    public ICell RealizeCell(IColumn column, int columnIndex, int rowIndex)
    {
        if (column is IColumn<TModel> c)
        {
            return c.CreateCell(this[rowIndex]);
        }
        throw new InvalidOperationException("Invalid column.");
    }

    public void SetItems(TreeDataGridItemsSourceView<TModel> items)
    {
        _items.CollectionChanged -= OnItemsCollectionChanged;
        _items = items;
        if (items != TreeDataGridItemsSourceView<TModel>.Empty)
        {
            _items.CollectionChanged += OnItemsCollectionChanged;
        }
        OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
    }

    public void Filter(Func<TModel, bool>? filter)
    {
        if (_filter != filter)
        {
            _filter = filter;
            RebuildSortedIndexes();
            CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
        }
    }

    public int ModelIndexToRowIndex(IndexPath modelIndex)
    {
        if (modelIndex.Count is not 1) return -1;

        var i = modelIndex[0];
        if (_sortedIndexes is null)
        {
            return i >= 0 && i < _items.Count ? modelIndex[0] : -1;
        }
        return SortHelper<int>.BinarySearch(_sortedIndexes, i, CompareItemsByIndex);
    }

    public IndexPath RowIndexToModelIndex(int rowIndex) => _sortedIndexes?[rowIndex] ?? rowIndex;

    public void Sort(IComparer<TModel>? comparer)
    {
        _comparer = comparer;
        RebuildSortedIndexes();
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public void UnrealizeCell(ICell cell, int columnIndex, int rowIndex)
    {
        (cell as IDisposable)?.Dispose();
    }

    public void RefreshFilter()
    {
        RebuildSortedIndexes();
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => GetEnumerator();

    private void RebuildSortedIndexes()
    {
        if (_comparer is null && _filter is null)
        {
            _sortedIndexes = null;
        }
        else
        {
            _sortedIndexes = StableSort.SortedMap(_items, (_comparer is null) ? null : CompareItemsByIndex, (_filter is null) ? null : FilterByIndex);
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_comparer is null && _filter is null)
        {
            OnItemsCollectionChangedUnsorted(e);
        }
        else
        {
            OnItemsCollectionChangedSorted(e);
        }
    }

    private void OnItemsCollectionChangedUnsorted(NotifyCollectionChangedEventArgs e)
    {
        if (CollectionChanged is null) return;

        var ev = e.Action switch
        {
            NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(e.Action, new AnonymousRowItems<TModel>(e.NewItems!), e.NewStartingIndex),
            NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(e.Action, new AnonymousRowItems<TModel>(e.OldItems!), e.OldStartingIndex),
            NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(e.Action, new AnonymousRowItems<TModel>(e.NewItems!), new AnonymousRowItems<TModel>(e.OldItems!), e.OldStartingIndex),
            NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(e.Action, new AnonymousRowItems<TModel>(e.NewItems!), e.NewStartingIndex, e.OldStartingIndex),
            NotifyCollectionChangedAction.Reset => e,
            _ => throw new NotSupportedException(),
        };

        CollectionChanged(this, ev);
    }

    private void OnItemsCollectionChangedSorted(NotifyCollectionChangedEventArgs e)
    {
        // If the rows have not yet been read then the type of collection change shouldn't be
        // important; the only thing we need to do is inform the presenter that the collection
        // has changed so that it can display the new items if the previous items were empty.
        if (_sortedIndexes is null)
        {
            CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add(e.NewStartingIndex, e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                Remove(e.OldStartingIndex, e.OldItems!);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                Remove(e.OldStartingIndex, e.OldItems!);
                Add(e.NewStartingIndex, e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Reset:
                RebuildSortedIndexes();
                CollectionChanged?.Invoke(this, e);
                break;
            default:
                throw new NotSupportedException();
        }

        void Add(int startIndex, IList items)
        {
            int count = items.Count;
            for (var i = 0; i < _sortedIndexes.Count; i++)
            {
                var ix = _sortedIndexes[i];
                if (ix >= startIndex)
                {
                    _sortedIndexes[i] = ix + count;
                }
            }
            for (var i = 0; i < count; i++)
            {
                var myindex = startIndex + i;
                if (_filter is null || FilterByIndex(myindex))
                {
                    var index = SortHelper<int>.BinarySearch(_sortedIndexes, myindex, CompareItemsByIndex);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    _sortedIndexes.Insert(index, myindex);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _row.Update(myindex, _items[myindex]), index));
                }
            }
        }

        void Remove(int startIndex, IList removed)
        {
            var count = removed.Count;
            var endIndex = startIndex + count;

            for (var i = 0; i < _sortedIndexes.Count; i++)
            {
                var ix = _sortedIndexes[i];
                if (ix >= startIndex && ix < endIndex)
                {
                    _sortedIndexes.RemoveAt(i);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _row.Update(ix, (TModel)removed[ix - startIndex]!), i));
                    i--;
                }
                else if (ix >= endIndex)
                {
                    _sortedIndexes[i] = ix - count;
                }
            }
        }
    }

    private int CompareItemsByIndex(int index1, int index2)
    {
        var c = _comparer!.Compare(_items[index1], _items[index2]);

        if (c == 0)
        {
            return index1 - index2; // ensure stability of sort
        }

        // -c will result in a negative value for int.MinValue (-int.MinValue == int.MinValue).
        // Flipping keys earlier is more likely to trigger something strange in a comparer,
        // particularly as it comes to the sort being stable.
        return (c > 0) ? 1 : -1;
    }

    private bool FilterByIndex(int a) => _filter!(_items[a]);
}
