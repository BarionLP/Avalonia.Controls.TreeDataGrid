using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid;

public class HierarchicalRows<TModel> : ReadOnlyListBase<HierarchicalRow<TModel>>,
    IRows,
    IDisposable,
    IExpanderRowController<TModel>
{
    private readonly IExpanderRowController<TModel> _controller;
    private readonly RootRows _roots;
    private readonly IExpanderColumn<TModel> _expanderColumn;
    private readonly List<HierarchicalRow<TModel>> _flattenedRows;
    private Comparison<TModel>? _comparison;
    private bool _ignoreCollectionChanges;
    private Func<TModel, bool>? _filter;

    public override HierarchicalRow<TModel> this[int index] => _flattenedRows[index];
    IRow IReadOnlyList<IRow>.this[int index] => _flattenedRows[index];
    public override int Count => _flattenedRows.Count;
    public bool IsFiltered => _roots.IsFiltered;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public HierarchicalRows(IExpanderRowController<TModel> controller, TreeDataGridItemsSourceView<TModel> items, IExpanderColumn<TModel> expanderColumn, Comparison<TModel>? comparison)
    {
        _controller = controller;
        _flattenedRows = [];
        _roots = new RootRows(this, items, comparison);
        _roots.CollectionChanged += OnRootsCollectionChanged;
        _expanderColumn = expanderColumn;
        _comparison = comparison;
        InitializeRows();
    }

    public void Dispose()
    {
        _ignoreCollectionChanges = true;
        _roots.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Expand(IndexPath index)
    {
        var count = index.Count;
        var rows = (IReadOnlyList<HierarchicalRow<TModel>>)_roots;

        for (var i = 0; i < count; ++i)
        {
            if (rows is null)
            {
                break;
            }

            var modelIndex = index[i];
            var found = false;

            foreach (var row in rows)
            {
                if (row.ModelIndex == modelIndex)
                {
                    row.IsExpanded = true;
                    rows = row.Children;
                    found = true;
                    break;
                }
            }

            if (!found) break;
        }
    }

    internal void ExpandCollapseRecursive(Func<TModel, bool> predicate, HierarchicalRow<TModel>? row = null)
    {
        _ignoreCollectionChanges = true;

        try
        {
            row?.IsExpanded = predicate(row.Model);

            var children = row is null ? _roots : row.Children;

            if (children is not null)
            {
                ExpandCollapseRecursiveCore(children, predicate);
            }
        }
        finally
        {
            _ignoreCollectionChanges = false;
        }

        _flattenedRows.Clear();
        InitializeRows();
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public void Collapse(IndexPath index)
    {
        var count = index.Count;
        var rows = (IReadOnlyList<HierarchicalRow<TModel>>?)_roots;

        for (var i = 0; i < count; ++i)
        {
            if (rows is null) break;

            var modelIndex = index[i];
            var found = false;

            foreach (var row in rows)
            {
                if (row.ModelIndex == modelIndex)
                {
                    if (i == count - 1)
                    {
                        row.IsExpanded = false;
                    }

                    rows = row.Children;
                    found = true;
                    break;
                }
            }

            if (!found) break;
        }
    }

    public (int index, double y) GetRowAt(double y)
    {
        return MathUtilities.IsZero(y) ? (0, 0) : (-1, -1);
    }

    public ICell RealizeCell(IColumn column, int columnIndex, int rowIndex)
    {
        if (column is IColumn<TModel> c)
        {
            return c.CreateCell(this[rowIndex]);
        }
        throw new InvalidOperationException("Invalid column.");
    }

    public void Filter(Func<TModel, bool>? filter)
    {
        _filter = filter;
        _ignoreCollectionChanges = true;

        try
        {
            _roots.Filter(filter);
            FilterChildren(filter);
        }
        finally
        {
            _ignoreCollectionChanges = false;
        }

        _flattenedRows.Clear();
        InitializeRows();
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public void SetItems(TreeDataGridItemsSourceView<TModel> items)
    {
        _ignoreCollectionChanges = true;

        try { _roots.SetItems(items); }
        finally { _ignoreCollectionChanges = false; }

        _flattenedRows.Clear();
        InitializeRows();
        if(_filter is not null) FilterChildren(_filter);
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public void Sort(Comparison<TModel>? comparison)
    {
        _comparison = comparison;
        _ignoreCollectionChanges = true;

        try
        {
            _roots.Sort(comparison);

            // Propagate to all materialized rows, including those hidden by the current
            // filter, so that their subtrees are correct when they become visible again.
            if (_roots.UnfilteredRows is { } rows)
            {
                foreach (var row in rows)
                {
                    row.SortChildren(comparison);
                }
            }
        }
        finally
        {
            _ignoreCollectionChanges = false;
        }

        _flattenedRows.Clear();
        InitializeRows();
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public void UnrealizeCell(ICell cell, int rowIndex, int columnIndex)
    {
        (cell as IDisposable)?.Dispose();
    }

    public void RefreshFilter()
    {
        _ignoreCollectionChanges = true;

        try
        {
            _roots.RefreshFilter();
            RefreshChildrenFilter();
        }
        finally
        {
            _ignoreCollectionChanges = false;
        }

        _flattenedRows.Clear();
        InitializeRows();
        CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
    }

    public int GetParentRowIndex(IndexPath modelIndex) => ModelIndexToRowIndex(modelIndex[..^1]);

    public int ModelIndexToRowIndex(IndexPath modelIndex)
    {
        if (modelIndex == default) return -1;

        for (var i = 0; i < _flattenedRows.Count; ++i)
        {
            if (_flattenedRows[i].ModelIndexPath == modelIndex)
            {
                return i;
            }
        }

        return -1;
    }

    public IndexPath RowIndexToModelIndex(int rowIndex)
    {
        if (rowIndex >= 0 && rowIndex < _flattenedRows.Count)
        {
            return _flattenedRows[rowIndex].ModelIndexPath;
        }

        return default;
    }

    public override IEnumerator<HierarchicalRow<TModel>> GetEnumerator() => _flattenedRows.GetEnumerator();
    IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => _flattenedRows.GetEnumerator();

    void IExpanderRowController<TModel>.OnBeginExpandCollapse(IExpanderRow<TModel> row)
    {
        _controller.OnBeginExpandCollapse(row);
    }

    void IExpanderRowController<TModel>.OnEndExpandCollapse(IExpanderRow<TModel> row)
    {
        _controller.OnEndExpandCollapse(row);
    }

    void IExpanderRowController<TModel>.OnChildCollectionChanged(IExpanderRow<TModel> row, NotifyCollectionChangedEventArgs e)
    {
        if (_ignoreCollectionChanges) return;

        if (row is not HierarchicalRow<TModel> h)
        {
            throw new NotSupportedException("Unexpected row type.");
        }
        OnCollectionChanged(h.ModelIndexPath, e);
    }

    internal bool TryGetRowIndex(in IndexPath modelIndex, out int rowIndex, int fromRowIndex = 0)
    {
        if (modelIndex.Count == 0)
        {
            rowIndex = -1;
            return true;
        }

        for (var i = fromRowIndex; i < _flattenedRows.Count; ++i)
        {
            if (modelIndex == _flattenedRows[i].ModelIndexPath)
            {
                rowIndex = i;
                return true;
            }
        }

        rowIndex = -1;
        return false;
    }

    private void InitializeRows()
    {
        foreach (var row in _roots)
        {
            Flatten(row, _flattenedRows);
        }
    }

    private static void Flatten(HierarchicalRow<TModel> row, List<HierarchicalRow<TModel>> output)
    {
        output.Add(row);

        if (row.Children is not null)
        {
            foreach (var childRow in row.Children)
            {
                Flatten(childRow, output);
            }
        }
    }

    private void FilterChildren(Func<TModel, bool>? filter)
    {
        // Propagate to all materialized rows, including those hidden by the current filter,
        // so that their subtrees are correct when a later RefreshFilter makes them visible.
        if (_roots.UnfilteredRows is { } rows)
        {
            foreach (var row in rows)
            {
                row.FilterChildren(filter);
            }
        }
    }

    private void RefreshChildrenFilter()
    {
        if (_roots.UnfilteredRows is { } rows)
        {
            foreach (var row in rows)
            {
                row.RefreshFilter();
            }
        }
    }

    private static void ExpandCollapseRecursiveCore(IReadOnlyList<HierarchicalRow<TModel>> rows, Func<TModel, bool> predicate)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (predicate(row.Model))
            {
                row.IsExpanded = true;
                if (row.Children is not null)
                {
                    ExpandCollapseRecursiveCore(row.Children, predicate);
                }
            }
            else
            {
                if (row.Children is not null)
                {
                    ExpandCollapseRecursiveCore(row.Children, predicate);
                }
                row.IsExpanded = false;
            }
        }
    }

    private void OnCollectionChanged(in IndexPath parentIndex, NotifyCollectionChangedEventArgs e)
    {
        if (_ignoreCollectionChanges)
            return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (TryGetRowIndex(parentIndex, out var parentRowIndex))
                {
                    var insert = Advance(parentRowIndex + 1, e.NewStartingIndex);
                    Add(insert, e.NewItems, raise: true);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (TryGetRowIndex(parentIndex, out parentRowIndex))
                {
                    var start = Advance(parentRowIndex + 1, e.OldStartingIndex);
                    var end = Advance(start, e.OldItems!.Count);
                    Remove(start, end - start, raise: true);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                if (TryGetRowIndex(parentIndex, out parentRowIndex))
                {
                    var start = Advance(parentRowIndex + 1, e.OldStartingIndex);
                    var end = Advance(start, e.OldItems!.Count);
                    Remove(start, end - start, raise: true);
                    Add(start, e.NewItems, raise: true);
                }
                break;
            case NotifyCollectionChangedAction.Move:
                if (TryGetRowIndex(parentIndex, out parentRowIndex))
                {
                    var fromStart = Advance(parentRowIndex + 1, e.OldStartingIndex);
                    var fromEnd = Advance(fromStart, e.OldItems!.Count);
                    var to = Advance(parentRowIndex + 1, e.NewStartingIndex);
                    Remove(fromStart, fromEnd - fromStart, raise: true);
                    Add(to, e.NewItems, raise: true);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                if (TryGetRowIndex(parentIndex, out parentRowIndex))
                {
                    var children = parentRowIndex >= 0 ? _flattenedRows[parentRowIndex].Children : _roots;
                    var count = GetDescendentRowCount(parentRowIndex);
                    Remove(parentRowIndex + 1, count, raise: true);
                    Add(parentRowIndex + 1, children, raise: true);
                }
                break;
            default:
                throw new NotSupportedException();
        }

        void Add(int index, IEnumerable? items, bool raise)
        {
            if (items is null)
                return;

            // Flatten into a buffer and insert in one operation so that rows after the
            // insertion point are only shifted once.
            var buffer = new List<HierarchicalRow<TModel>>();

            foreach (HierarchicalRow<TModel> row in items)
            {
                Flatten(row, buffer);
            }

            if (buffer.Count == 0)
                return;

            _flattenedRows.InsertRange(index, buffer);

            if (raise)
            {
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        new ListSpan(_flattenedRows, index, buffer.Count),
                        index));
            }
        }

        void Remove(int index, int count, bool raise)
        {
            if (count is 0)
                return;

            var oldItems = raise && CollectionChanged is not null ?
                new HierarchicalRow<TModel>[count] : null;

            for (var i = 0; i < count; ++i)
            {
                var row = _flattenedRows[i + index];
                oldItems?[i] = row;
            }

            _flattenedRows.RemoveRange(index, count);

            if (oldItems is not null)
            {
                CollectionChanged!(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        oldItems,
                        index));
            }
        }

        int Advance(int rowIndex, int count)
        {
            var i = rowIndex;

            while (count > 0)
            {
                var row = _flattenedRows[i];
                if (row.Children?.Count > 0)
                    i = Advance(i + 1, row.Children.Count);
                else
                    i += 1;
                --count;
            }

            return i;
        }

        int GetDescendentRowCount(int rowIndex)
        {
            if (rowIndex == -1)
                return _flattenedRows.Count;

            var row = _flattenedRows[rowIndex];
            var depth = row.ModelIndexPath.Count;
            var i = rowIndex + 1;

            while (i < _flattenedRows.Count && _flattenedRows[i].ModelIndexPath.Count > depth)
                ++i;

            return i - (rowIndex + 1);
        }
    }

    private void OnRootsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnCollectionChanged(default, e);
    }

    private sealed class RootRows(HierarchicalRows<TModel> owner, TreeDataGridItemsSourceView<TModel> items, Comparison<TModel>? comparison) : SortableRowsBase<TModel, HierarchicalRow<TModel>>(items, comparison), IReadOnlyList<HierarchicalRow<TModel>>
    {
        private readonly HierarchicalRows<TModel> _owner = owner;

        protected override HierarchicalRow<TModel> CreateRow(int modelIndex, TModel model)
        {
            var row = new HierarchicalRow<TModel>(_owner, _owner._expanderColumn, new IndexPath(modelIndex), model, _owner._comparison);
            row.FilterChildren(_owner._filter);
            return row;
        }
    }
}
