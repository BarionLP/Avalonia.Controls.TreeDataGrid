using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.Controls;

public enum TreeDataGridRowDropPosition
{
    None,
    Before,
    After,
    Inside,
}

/// <summary>
/// Provides data for the <see cref="TreeDataGrid.RowDragOver"/> and
/// <see cref="TreeDataGrid.RowDrop"/> events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TreeDataGridRowDragEventArgs"/> class.
/// </remarks>
/// <param name="routedEvent">The event being raised.</param>
/// <param name="row">The row that is being dragged over.</param>
/// <param name="inner">The inner drag event args.</param>
public sealed class TreeDataGridRowDragEventArgs(RoutedEvent routedEvent, TreeDataGridRow? row, DragEventArgs inner) : RoutedEventArgs(routedEvent)
{

    /// <summary>
    /// Gets the <see cref="DragEventArgs"/> that describes the drag/drop operation.
    /// </summary>
    public DragEventArgs Inner { get; } = inner;

    /// <summary>
    /// Gets the row being dragged over.
    /// </summary>
    public TreeDataGridRow? TargetRow { get; } = row;

    /// <summary>
    /// Gets or sets a value indicating the how the data should be dropped into
    /// the <see cref="TargetRow"/>.
    /// </summary>
    /// <remarks>
    /// For drag operations, the value of this property controls the adorner displayed when
    /// dragging. For drop operations, controls the final location of the drop.
    /// </remarks>
    public TreeDataGridRowDropPosition Position { get; set; }
}
