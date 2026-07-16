using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Models;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;

namespace Avalonia.Controls;

/// <summary>
/// A data source for a <see cref="TreeDataGrid"/> which displays a flat grid.
/// </summary>
/// <typeparam name="TModel">The model type.</typeparam>
public class FlatTreeDataGridSource<TModel>(IEnumerable<TModel> items) : NotifyingBase,
    ITreeDataGridSource<TModel>,
    IDisposable
        where TModel : class
{
    private IEnumerable<TModel> _items = items;
    private TreeDataGridItemsSourceView<TModel> _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(items);
    private AnonymousSortableRows<TModel>? _rows;
    private IComparer<TModel>? _comparer;
    private ITreeDataGridSelection? _selection;
    private bool _isSelectionSet;
    private IColumn? sortedColumn;

    public ColumnList<TModel> Columns { get; } = [];
    public IRows Rows => _rows ??= CreateRows();
    IColumns ITreeDataGridSource.Columns => Columns;

    public IEnumerable<TModel> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value;
                _itemsView = TreeDataGridItemsSourceView<TModel>.GetOrCreate(value);
                _rows?.SetItems(_itemsView);
                _selection?.Source = value;
                RaisePropertyChanged();
            }
        }
    }

    public ITreeDataGridSelection? Selection
    {
        get
        {
            if (_selection == null && !_isSelectionSet)
            {
                _selection = new TreeDataGridRowSelectionModel<TModel>(this);
            }
            return _selection;
        }
        set
        {
            if (_selection != value)
            {
                if (value?.Source != _items)
                {
                    throw new InvalidOperationException("Selection source must be set to Items.");
                }
                _selection = value;
                _isSelectionSet = true;
                RaisePropertyChanged();
            }
        }
    }

    IEnumerable<object> ITreeDataGridSource.Items => Items;

    public ITreeDataGridCellSelectionModel<TModel>? CellSelection => Selection as ITreeDataGridCellSelectionModel<TModel>;
    public ITreeDataGridRowSelectionModel<TModel>? RowSelection => Selection as ITreeDataGridRowSelectionModel<TModel>;

    public bool IsFiltered => _rows?.IsFiltered ?? false;
    public bool IsHierarchical => false;
    public bool IsSorted => _comparer is not null;

    public event Action? Sorted;

    public void ClearSort(IColumn column)
    {
        if (column == sortedColumn)
        {
            _comparer = null;
            _rows?.Sort(_comparer);
            column.SortDirection = null;
            Sorted?.Invoke();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _rows?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Filters the rows according to a predicate.
    /// </summary>
    /// <param name="filter">The filter predicate, or null to clear the filter.</param>
    public void Filter(Func<TModel, bool>? filter)
    {
        (_rows ??= CreateRows()).Filter(filter);
    }

    /// <summary>
    /// Refreshes the current filter applied to the collection.
    /// </summary>
    /// <remarks>
    /// Call this method if the filter predicate depends on external state which has changed,
    /// to re-apply the filter to the collection.
    /// </remarks>
    public void RefreshFilter()
    {
        _rows?.RefreshFilter();
    }

    void ITreeDataGridSource.DragDropRows(
        ITreeDataGridSource source,
        IEnumerable<IndexPath> indexes,
        IndexPath targetIndex,
        TreeDataGridRowDropPosition position,
        DragDropEffects effects)
    {
        if (effects is not DragDropEffects.Move)
        {
            throw new NotSupportedException("Only move is currently supported for drag/drop.");
        }
        if (IsSorted)
        {
            throw new NotSupportedException("Drag/drop is not supported on sorted data.");
        }
        if (position is TreeDataGridRowDropPosition.Inside)
        {
            throw new ArgumentException("Invalid drop position.", nameof(position));
        }
        if (indexes.Any(x => x.Count is not 1))
        {
            throw new ArgumentException("Invalid source index.", nameof(indexes));
        }
        if (targetIndex.Count != 1)
        {
            throw new ArgumentException("Invalid target index.", nameof(targetIndex));
        }
        if (_items is not IList<TModel> items)
        {
            throw new InvalidOperationException("Items does not implement IList<T>.");
        }
        if (position is TreeDataGridRowDropPosition.None)
        {
            return;
        }
        var ti = targetIndex[0];
        if (position == TreeDataGridRowDropPosition.After)
        {
            ti++;
        }
        var sourceItems = new List<TModel>();
        foreach (var src in indexes.OrderByDescending(x => x))
        {
            var i = src[0];
            sourceItems.Add(items[i]);
            items.RemoveAt(i);
            if (i < ti)
            {
                ti--;
            }
        }
        for (var si = sourceItems.Count - 1; si >= 0; si--)
        {
            items.Insert(ti++, sourceItems[si]);
        }
    }
    public bool SortBy(IColumn? column, ListSortDirection direction)
    {
        if (column is IColumn<TModel> typedColumn && Columns.Contains(typedColumn))
        {
            var comparison = typedColumn.GetComparison(direction);
            if (comparison is null) return false;

            Sort(comparison);

            foreach (var c in Columns)
            {
                c.SortDirection = c == column ? direction : null;
            }
            sortedColumn = column;
            return true;
        }
        return false;
    }

    private void Sort(Comparison<TModel?> comparison)
    {
        _comparer = new FuncComparer<TModel>(comparison);
        _rows?.Sort(_comparer);
        Sorted?.Invoke();
    }

    IEnumerable<object> ITreeDataGridSource.GetModelChildren(object model) => [];

    private AnonymousSortableRows<TModel> CreateRows() => new(_itemsView, _comparer);
}
