using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Utils;

namespace Avalonia.Controls.Models.TreeDataGrid;

/// <summary>
/// An <see cref="IRows"/> collection which supports sorting.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
/// <typeparam name="TRow">The row type.</typeparam>
public abstract class SortableRowsBase<TModel, TRow> : ReadOnlyListBase<TRow>, IDisposable
    where TRow : IRow<TModel>, IModelIndexableRow, IDisposable
{
    private readonly RowIndexMap _indexes = new();
    private TreeDataGridItemsSourceView<TModel> _items;
    private Comparison<TModel>? _comparison;
    private Func<TModel, bool>? _filter;
    private List<TRow>? _unsortedRows;

    public override int Count
    {
        get
        {
            // When a filter is set the rows must be materialized for the filtered count
            // to be known; otherwise the unfiltered item count would be reported.
            if (_unsortedRows is null && _filter is not null)
            {
                GetOrCreateRows();
            }
            return _indexes.RowCount ?? _unsortedRows?.Count ?? _items.Count;
        }
    }
    public bool IsFiltered => _filter is not null;

    /// <summary>
    /// Gets all materialized rows, including those currently hidden by the filter, or null if
    /// the rows have not yet been materialized.
    /// </summary>
    internal IReadOnlyList<TRow>? UnfilteredRows => _unsortedRows;


    public override TRow this[int index] => GetOrCreateRows()[_indexes.RowToModelIndex(index)];

    private List<TRow> UnsortedRows => GetOrCreateRows();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public SortableRowsBase(TreeDataGridItemsSourceView<TModel> items, Comparison<TModel>? comparison)
    {
        _items = items;
        _items.CollectionChanged += OnItemsCollectionChanged;
        _comparison = comparison;
        _indexes.SetComparison(comparison is null ? null : CompareItemsByIndex);
    }

    public virtual void Dispose()
    {
        SetItems(TreeDataGridItemsSourceView<TModel>.Empty);
        GC.SuppressFinalize(this);
    }

    public override IEnumerator<TRow> GetEnumerator()
    {
        var rows = GetOrCreateRows();
        var count = Count;

        for (var i = 0; i < count; ++i)
        {
            yield return rows[_indexes.RowToModelIndex(i)];
        }
    }

    public void Filter(Func<TModel, bool>? filter)
    {
        if (_filter != filter)
        {
            _filter = filter;
            _indexes.SetFilter(filter is null ? null : FilterByIndex);
            RebuildSortedIndexes();
        }
    }

    public void SetItems(TreeDataGridItemsSourceView<TModel> items)
    {
        _items.CollectionChanged -= OnItemsCollectionChanged;
        _items = items;

        if (!ReferenceEquals(items, TreeDataGridItemsSourceView<TModel>.Empty))
            _items.CollectionChanged += OnItemsCollectionChanged;

        OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
    }

    public virtual void Sort(Comparison<TModel>? comparison)
    {
        _comparison = comparison;
        _indexes.SetComparison(comparison is null ? null : CompareItemsByIndex);
        RebuildSortedIndexes();
    }

    public void RefreshFilter()
    {
        RebuildSortedIndexes();
    }

    protected abstract TRow CreateRow(int modelIndex, TModel model);

    private List<TRow> GetOrCreateRows()
    {
        if (_unsortedRows is not null)
        {
            return _unsortedRows;
        }

        _unsortedRows = new List<TRow>(_items.Count);
        for (var i = 0; i < _items.Count; i++)
        {
            _unsortedRows.Add(CreateRow(i, _items[i]));
        }

        _indexes.Rebuild(_items.Count);
        return _unsortedRows;
    }

    private void ResetRows()
    {
        if (_unsortedRows is not null)
        {
            foreach (var row in _unsortedRows)
            {
                row.Dispose();
            }
        }

        _unsortedRows = null;
        _indexes.Reset();
    }

    private void RebuildSortedIndexes()
    {
        if (_unsortedRows is null)
        {
            return;
        }

        _indexes.Rebuild(_items.Count);
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Without a sort or a filter, rows are the items: a change to a range of items is a
        // change to the same range of rows and can be reported in one event. Once the rows are
        // ordered or filtered a range of items maps to scattered rows, so each affected row has
        // to be reported separately.
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
        if (_unsortedRows is null)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add(e.NewStartingIndex, e.NewItems!);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new ListSpan(_unsortedRows, e.NewStartingIndex, e.NewItems!.Count), e.NewStartingIndex));
                break;
            case NotifyCollectionChangedAction.Remove:
                {
                    var changedItems = CollectionChanged is null ? null : _unsortedRows.Slice(e.OldStartingIndex, e.OldItems!.Count);
                    Remove(e.OldStartingIndex, e.OldItems!.Count);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, e.OldStartingIndex));
                    break;
                }
            case NotifyCollectionChangedAction.Replace:
                {
                    var oldStartingIndex = e.OldStartingIndex;
                    var count = e.OldItems!.Count;
                    var oldRows = _unsortedRows.Slice(oldStartingIndex, count);
                    for (var i = 0; i < count; i++)
                    {
                        _unsortedRows[oldStartingIndex + i] = CreateRow(oldStartingIndex + i, (TModel)e.NewItems![i]!);
                    }
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new ListSpan(_unsortedRows, oldStartingIndex, count), oldRows, oldStartingIndex));

                    // Dispose the replaced rows after raising the event, so that events
                    // triggered by disposal don't find stale rows in consumers' state, and
                    // so that expanded rows stop listening to their child collections.
                    foreach (var row in oldRows)
                    {
                        row.Dispose();
                    }
                    break;
                }
            case NotifyCollectionChangedAction.Move:
                Remove(e.OldStartingIndex, e.OldItems!.Count);
                Add(e.NewStartingIndex, e.NewItems!);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new ListSpan(_unsortedRows, e.NewStartingIndex, e.NewItems!.Count), e.NewStartingIndex, e.OldStartingIndex));
                break;
            case NotifyCollectionChangedAction.Reset:
                ResetRows();
                CollectionChanged?.Invoke(this, e);
                break;
            default:
                throw new NotSupportedException();
        }

        void Add(int index, IList items)
        {
            var rows = new List<TRow>(items.Count);
            foreach (TModel item in items)
            {
                rows.Add(CreateRow(index + rows.Count, item));
            }
            _unsortedRows.InsertRange(index, rows);
            index += rows.Count;
            while (index < _unsortedRows.Count)
            {
                _unsortedRows[index++].UpdateModelIndex(items.Count);
            }
        }
        void Remove(int index, int num)
        {
            for (int j = index; j < index + num; j++)
            {
                _unsortedRows[j].Dispose();
            }
            _unsortedRows.RemoveRange(index, num);
            while (index < _unsortedRows.Count)
            {
                _unsortedRows[index++].UpdateModelIndex(-num);
            }
        }
    }

    private void OnItemsCollectionChangedSorted(NotifyCollectionChangedEventArgs e)
    {
        if (_unsortedRows is null)
            return;

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
                ResetRows();
                CollectionChanged?.Invoke(this, e);
                break;
            default:
                throw new NotSupportedException();
        }

        void Add(int startIndex, int count)
        {
            // Add the new rows to the unsorted rows.
            var newRows = new List<TRow>(count);
            for (var i = startIndex; i < startIndex + count; ++i)
            {
                newRows.Add(CreateRow(i, _items[i]));
            }
            _unsortedRows.InsertRange(startIndex, newRows);

            // Update the indexes of subsequent rows.
            for (var i = startIndex + count; i < _unsortedRows.Count; ++i)
            {
                _unsortedRows[i].UpdateModelIndex(count);
            }

            _indexes.ItemsAdded(startIndex, count, (modelIndex, rowIndex) =>
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        _unsortedRows[modelIndex],
                        rowIndex)));
        }

        void Remove(int startIndex, IList removed)
        {
            var count = removed.Count;
            var removedRows = _unsortedRows.Slice(startIndex, count);

            // Remove the rows from the unsorted rows.
            _unsortedRows.RemoveRange(startIndex, count);

            // Update the model indexes of all subsequent rows, including those currently
            // hidden by the filter.
            for (var i = startIndex; i < _unsortedRows.Count; ++i)
            {
                _unsortedRows[i].UpdateModelIndex(-count);
            }

            _indexes.ItemsRemoved(startIndex, count, (modelIndex, rowIndex) =>
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removedRows[modelIndex - startIndex],
                        rowIndex)));

            // Dispose the removed rows after raising events, so that events triggered by
            // disposal don't find stale rows in consumers' state.
            foreach (var row in removedRows)
            {
                row.Dispose();
            }
        }
    }

    private int CompareItemsByIndex(int index1, int index2)
    {
        var c = _comparison!(_items[index1], _items[index2]);

        if (c == 0)
        {
            return index1 - index2; // ensure stability of sort
        }

        // -c will result in a negative value for int.MinValue (-int.MinValue == int.MinValue).
        // Flipping keys earlier is more likely to trigger something strange in a comparer,
        // particularly as it comes to the sort being stable.
        return (c > 0) ? 1 : -1;
    }

    private bool FilterByIndex(int index) => _filter!(_items[index]);
}
