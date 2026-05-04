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
    private TreeDataGridItemsSourceView<TModel> _items;
    private Comparison<TModel>? _comparison;
    private Func<TModel, bool>? _filter;
    private List<TRow>? _unsortedRows;
    private List<int>? _sortedIndexes;

    public override int Count => _sortedIndexes?.Count ?? _unsortedRows?.Count ?? _items.Count;
    public bool IsFiltered => _filter is not null;


    public override TRow this[int index]
    {
        get
        {
            GetOrCreateRows();

            return _sortedIndexes is null ? UnsortedRows[index] : UnsortedRows[_sortedIndexes[index]];
        }
    }

    private List<TRow> UnsortedRows => GetOrCreateRows();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public SortableRowsBase(TreeDataGridItemsSourceView<TModel> items, Comparison<TModel>? comparison)
    {
        _items = items;
        _items.CollectionChanged += OnItemsCollectionChanged;
        _comparison = comparison;
    }

    public virtual void Dispose()
    {
        SetItems(TreeDataGridItemsSourceView<TModel>.Empty);
        GC.SuppressFinalize(this);
    }

    public override IEnumerator<TRow> GetEnumerator()
    {
        GetOrCreateRows();
        return _sortedIndexes is null ? UnsortedRows.GetEnumerator() : GetSortedEnumerator();

        IEnumerator<TRow> GetSortedEnumerator()
        {
            var rows = UnsortedRows;
            foreach (int item in _sortedIndexes)
            {
                yield return rows[item];
            }
        }
    }

    public void Filter(Func<TModel, bool>? filter)
    {
        if (_filter != filter)
        {
            _filter = filter;
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
        RebuildSortedIndexes();
    }

    public void RefreshFilter()
    {
        RebuildSortedIndexes();
    }

    protected abstract TRow CreateRow(int modelIndex, TModel model);

    protected int ModelIndexToRowIndex(int modelIndex)
    {
        if (_sortedIndexes is null)
        {
            return modelIndex >= 0 && modelIndex < _items.Count ? modelIndex : -1;
        }

        return SortHelper<int>.BinarySearch(_sortedIndexes, modelIndex, CompareItemsByIndex);
    }

    protected int RowIndexToModelIndex(int rowIndex) => _sortedIndexes?[rowIndex] ?? rowIndex;

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
        if (_comparison != null || _filter != null)
        {
            _sortedIndexes = StableSort.SortedMap(_items, _comparison is null ? null : CompareItemsByIndex, _filter is null ? null : FilterByIndex);
        }
        else
        {
            _sortedIndexes = null;
        }
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
        _sortedIndexes = null;
    }

    private void RebuildSortedIndexes()
    {
        if (_unsortedRows is null)
        {
            return;
        }

        if (_comparison is not null || _filter is not null)
        {
            _sortedIndexes = StableSort.SortedMap(_items, _comparison is null ? null : CompareItemsByIndex, _filter is null ? null : FilterByIndex);
        }
        else
        {
            _sortedIndexes = null;
        }
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    private void OnItemsCollectionChanged(object? a, NotifyCollectionChangedEventArgs b)
    {
        if (_comparison is null && _filter is null)
        {
            OnItemsCollectionChangedUnsorted(b);
        }
        else
        {
            OnItemsCollectionChangedSorted(b);
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
                    var oldItems = CollectionChanged is null ? null : _unsortedRows.Slice(oldStartingIndex, count);
                    for (var i = 0; i < count; i++)
                    {
                        _unsortedRows[oldStartingIndex + i] = CreateRow(oldStartingIndex + i, (TModel)e.NewItems![i]!);
                    }
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new ListSpan(_unsortedRows, oldStartingIndex, count), oldItems!, oldStartingIndex));
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
            foreach (TModel item in items)
            {
                _unsortedRows.Insert(index, CreateRow(index, item));
                index++;
            }
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
            for (var i = startIndex; i < startIndex + count; ++i)
            {
                _unsortedRows.Insert(i, CreateRow(i, _items[i]));
            }

            // Update the indexes of subsequent rows.
            for (var i = startIndex + count; i < _unsortedRows.Count; ++i)
            {
                _unsortedRows[i].UpdateModelIndex(count);
            }

            // Update the indexes of subsequent sorted indexes.
            for (var i = 0; i < _sortedIndexes!.Count; i++)
            {
                var ix = _sortedIndexes[i];
                if (ix >= startIndex)
                {
                    _sortedIndexes[i] = ix + count;
                }
            }

            // Insert the new row into the correct place in the sorted indexes.
            for (var i = 0; i < count; ++i)
            {
                int myIndex = startIndex + i;

                if (_filter is null || _filter(_items[myIndex]))
                {
                    var index = _comparison is null ? _sortedIndexes.Count : SortHelper<int>.BinarySearch(_sortedIndexes, myIndex, CompareItemsByIndex);
                    if (index < 0)
                    {
                        index = ~index;
                    }
                    _sortedIndexes.Insert(index, myIndex);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _unsortedRows[myIndex], index));
                }
            }
        }

        void Remove(int startIndex, IList removed)
        {
            var count = removed.Count;
            var endIndex = startIndex + count;

            // Dispose the removed rows.
            for (var i = 0; i < count; ++i)
            {
                _unsortedRows[startIndex + i].Dispose();
            }

            // Remove the rows from the unsorted rows.
            _unsortedRows.RemoveRange(startIndex, count);

            // Iterate the sorted indexes, raising a collection changed event for the
            // items removed, and updating the indexes of the subsequent items.
            for (var i = 0; i < _sortedIndexes!.Count; i++)
            {
                var ix = _sortedIndexes[i];
                if (ix >= startIndex && ix < endIndex)
                {
                    _sortedIndexes.RemoveAt(i);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            (TModel)removed[ix - startIndex]!,
                            i));
                    --i;
                }
                else if (ix >= endIndex)
                {
                    _sortedIndexes[i] = ix - count;
                    _unsortedRows[_sortedIndexes[i]].UpdateModelIndex(-removed.Count);
                }
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

    private bool FilterByIndex(int a) => _filter!(_items[a]);
}
