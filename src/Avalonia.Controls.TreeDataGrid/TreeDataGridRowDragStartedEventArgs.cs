using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.Controls;

/// <summary>
/// Provides data for the <see cref="TreeDataGrid.RowDragStarted"/> event.
/// </summary>
public sealed class TreeDataGridRowDragStartedEventArgs(IEnumerable<object> models) : RoutedEventArgs(TreeDataGrid.RowDragStartedEvent)
{
    public DragDropEffects AllowedEffects { get; set; }
    public IEnumerable<object> Models { get; } = models;
}
