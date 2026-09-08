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
    private readonly RowIndexMap _indexes = new();
    private TreeDataGridItemsSourceView<TModel> _items;
    private IComparer<TModel>? _comparer;
    private Func<TModel, bool>? _filter;

    public override IRow<TModel> this[int index]
    {
        get
        {
            if (_indexes.IsActive && !_indexes.IsBuilt)
                _indexes.Rebuild(_items.Count);

            var modelIndex = _indexes.RowToModelIndex(index);
            return _row.Update(modelIndex, _items[modelIndex]);
        }
    }

    IRow IReadOnlyList<IRow>.this[int index] => this[index];
    public override int Count => _indexes.RowCount ?? _items.Count;
    public bool IsFiltered => _filter is not null;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public AnonymousSortableRows(TreeDataGridItemsSourceView<TModel> items, IComparer<TModel>? comparer)
    {
        _items = items;
        _items.CollectionChanged += OnItemsCollectionChanged;
        _comparer = comparer;
        _indexes.SetComparison(comparer is null ? null : CompareItemsByIndex);
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
            _indexes.SetFilter(filter is null ? null : FilterByIndex);
            _indexes.Rebuild(_items.Count);
            CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
        }
    }

    public int ModelIndexToRowIndex(IndexPath modelIndex)
    {
        if (modelIndex.Count is not 1) return -1;
        return _indexes.ModelToRowIndex(modelIndex[0], _items.Count);
    }

    public IndexPath RowIndexToModelIndex(int rowIndex) => _indexes.RowToModelIndex(rowIndex);

    public void Sort(IComparer<TModel>? comparer)
    {
        _comparer = comparer;
        _indexes.SetComparison(comparer is null ? null : CompareItemsByIndex);
        _indexes.Rebuild(_items.Count);
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public void UnrealizeCell(ICell cell, int columnIndex, int rowIndex)
    {
        (cell as IDisposable)?.Dispose();
    }

    public void RefreshFilter()
    {
        _indexes.Rebuild(_items.Count);
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => GetEnumerator();

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Without a sort or a filter, rows are the items: a change can be forwarded as-is, one
        // event per change. Once the rows are ordered or filtered a change to a range of items
        // affects scattered rows, so it has to be reported a row at a time.
        if (_indexes.IsActive)
        {
            OnItemsCollectionChangedSorted(e);
        }
        else
        {
            OnItemsCollectionChangedUnsorted(e);
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
        if (!_indexes.IsBuilt)
        {
            CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add(e.NewStartingIndex, e.NewItems!.Count);
                break;
            case NotifyCollectionChangedAction.Remove:
                Remove(e.OldStartingIndex, e.OldItems!);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                Remove(e.OldStartingIndex, e.OldItems!);
                Add(e.NewStartingIndex, e.NewItems!.Count);
                break;
            case NotifyCollectionChangedAction.Reset:
                _indexes.Rebuild(_items.Count);
                CollectionChanged?.Invoke(this, e);
                break;
            default:
                throw new NotSupportedException();
        }

        void Add(int startIndex, int count)
        {
            _indexes.ItemsAdded(startIndex, count, (modelIndex, rowIndex) =>
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        _row.Update(modelIndex, _items[modelIndex]),
                        rowIndex)));
        }

        void Remove(int startIndex, IList removed)
        {
            _indexes.ItemsRemoved(startIndex, removed.Count, (modelIndex, rowIndex) =>
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        _row.Update(modelIndex, (TModel)removed[modelIndex - startIndex]!),
                        rowIndex)));
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
