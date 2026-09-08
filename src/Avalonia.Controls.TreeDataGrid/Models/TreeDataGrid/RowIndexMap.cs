using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Controls.Utils;

namespace Avalonia.Controls.Models.TreeDataGrid;

/// <summary>
/// Maps between the row indexes of a rows collection and the model indexes of its source, for a
/// collection which may be sorted, filtered, or both.
/// </summary>
/// <remarks>
/// The map works purely in terms of indexes: the comparison and the filter are supplied as
/// delegates over model indexes, so a single implementation serves both the rows collections
/// which materialize a row per model and those which reuse a single row object.
///
/// While neither a comparison nor a filter is set, no map is needed: row index and model index
/// are the same, and <see cref="IsActive"/> is false. The map is built lazily, so
/// <see cref="IsBuilt"/> can be false even when it is active.
/// </remarks>
internal sealed class RowIndexMap
{
    private Comparison<int>? _compare;
    private Func<int, bool>? _filter;
    private List<int>? _rowToModel;

    /// <summary>
    /// Gets a value indicating whether row indexes differ from model indexes, i.e. whether a
    /// comparison or a filter is set.
    /// </summary>
    public bool IsActive => _compare is not null || _filter is not null;

    /// <summary>
    /// Gets a value indicating whether the map has been built.
    /// </summary>
    public bool IsBuilt => _rowToModel is not null;

    /// <summary>
    /// Gets the number of rows, or null if the map isn't built and the source count should be
    /// used instead.
    /// </summary>
    public int? RowCount => _rowToModel?.Count;

    public void SetComparison(Comparison<int>? compare) => _compare = compare;
    public void SetFilter(Func<int, bool>? filter) => _filter = filter;

    /// <summary>
    /// Discards the map, so that row indexes are model indexes until it is rebuilt.
    /// </summary>
    public void Reset() => _rowToModel = null;

    /// <summary>
    /// Rebuilds the map from scratch over a source of <paramref name="itemCount"/> items.
    /// </summary>
    public void Rebuild(int itemCount)
    {
        if (!IsActive)
        {
            _rowToModel = null;
            return;
        }

        var map = new List<int>(itemCount);

        for (var i = 0; i < itemCount; ++i)
        {
            if (_filter is null || _filter(i))
                map.Add(i);
        }

        if (_compare is not null)
            SortHelper<int>.Sort(CollectionsMarshal.AsSpan(map), _compare);

        _rowToModel = map;
    }

    /// <summary>
    /// Returns the model index displayed by the specified row.
    /// </summary>
    public int RowToModelIndex(int rowIndex) => _rowToModel?[rowIndex] ?? rowIndex;

    /// <summary>
    /// Returns the row displaying the specified model index, or -1 if the model has no row
    /// because it is hidden by the filter or is out of range.
    /// </summary>
    /// <param name="modelIndex">The model index.</param>
    /// <param name="itemCount">The number of items in the source.</param>
    public int ModelToRowIndex(int modelIndex, int itemCount)
    {
        // Reject out of range model indexes before searching: the comparison reads the model at
        // the index, so it can't be used to look up an item which isn't in the source.
        if (modelIndex < 0 || modelIndex >= itemCount)
            return -1;

        if (IsActive && !IsBuilt)
            Rebuild(itemCount);

        if (_rowToModel is null)
            return modelIndex;

        var rowIndex = FindRowIndex(modelIndex);

        // A negative result is the bitwise complement of the insertion point: the model index
        // has no row because it's hidden by the filter.
        return rowIndex >= 0 ? rowIndex : -1;
    }

    /// <summary>
    /// Updates the map for items inserted into the source.
    /// </summary>
    /// <param name="modelIndex">The model index of the first inserted item.</param>
    /// <param name="count">The number of items inserted.</param>
    /// <param name="onRowAdded">
    /// Called for each inserted item which has a row, with the model index of the item and the
    /// row index at which it was inserted.
    /// </param>
    public void ItemsAdded(int modelIndex, int count, Action<int, int> onRowAdded)
    {
        var map = _rowToModel ?? throw new InvalidOperationException("The map has not been built.");

        // Shift the models at or after the insertion point.
        for (var i = 0; i < map.Count; ++i)
        {
            if (map[i] >= modelIndex)
                map[i] += count;
        }

        for (var i = 0; i < count; ++i)
        {
            var added = modelIndex + i;

            if (_filter is not null && !_filter(added))
                continue;

            var rowIndex = FindRowIndex(added);

            if (rowIndex < 0)
                rowIndex = ~rowIndex;

            map.Insert(rowIndex, added);
            onRowAdded(added, rowIndex);
        }
    }

    /// <summary>
    /// Updates the map for items removed from the source.
    /// </summary>
    /// <param name="modelIndex">The model index of the first removed item.</param>
    /// <param name="count">The number of items removed.</param>
    /// <param name="onRowRemoved">
    /// Called for each removed item which had a row, with the model index the item had before
    /// the removal and the row index it was removed from.
    /// </param>
    public void ItemsRemoved(int modelIndex, int count, Action<int, int> onRowRemoved)
    {
        var map = _rowToModel ?? throw new InvalidOperationException("The map has not been built.");
        var endIndex = modelIndex + count;

        for (var i = 0; i < map.Count; ++i)
        {
            var ix = map[i];

            if (ix >= modelIndex && ix < endIndex)
            {
                map.RemoveAt(i);
                onRowRemoved(ix, i);
                --i;
            }
            else if (ix >= endIndex)
            {
                map[i] = ix - count;
            }
        }
    }

    /// <summary>
    /// Binary searches the map for a model index, returning the bitwise complement of the
    /// insertion point if it isn't present.
    /// </summary>
    private int FindRowIndex(int modelIndex)
    {
        // With no comparison set the map holds model indexes in ascending order, so the default
        // integer comparison finds them.
        return _compare is null
            ? SortHelper<int>.BinarySearch(_rowToModel!, modelIndex)
            : SortHelper<int>.BinarySearch(_rowToModel!, modelIndex, _compare);
    }
}
