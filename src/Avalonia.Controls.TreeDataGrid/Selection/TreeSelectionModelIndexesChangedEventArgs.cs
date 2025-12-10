using System;

namespace Avalonia.Controls.Selection;

/// <summary>
/// Holds data for the <see cref="ITreeSelectionModel.IndexesChanged"/> event.
/// </summary>
public sealed class TreeSelectionModelIndexesChangedEventArgs(
    IndexPath parentIndex,
    int startIndex,
    int endIndex,
    int delta) : EventArgs
{

    /// <summary>
    /// Gets the index of the parent item.
    /// </summary>
    public IndexPath ParentIndex { get; } = parentIndex;

    /// <summary>
    /// Gets the inclusive start index of the range of indexes that changed.
    /// </summary>
    public int StartIndex { get; } = startIndex;

    /// <summary>
    /// Gets the exclusive end index of the range of indexes that changed.
    /// </summary>
    public int EndIndex { get; } = endIndex;

    /// <summary>
    /// Gets the delta of the change; i.e. the number of indexes added or removed.
    /// </summary>
    public int Delta { get; } = delta;
}
