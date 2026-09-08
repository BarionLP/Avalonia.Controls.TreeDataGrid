using System;
using System.Text;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Logging;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Primitives;

[PseudoClasses(":selected")]
public class TreeDataGridRow : TemplatedControl
{
    private const double DragDistance = 3;
    private static readonly Point s_InvalidPoint = new(double.NegativeInfinity, double.NegativeInfinity);

    public static readonly DirectProperty<TreeDataGridRow, IColumns?> ColumnsProperty =
        AvaloniaProperty.RegisterDirect<TreeDataGridRow, IColumns?>(
            nameof(Columns),
            o => o.Columns);

    public static readonly DirectProperty<TreeDataGridRow, TreeDataGridElementFactory?> ElementFactoryProperty =
        AvaloniaProperty.RegisterDirect<TreeDataGridRow, TreeDataGridElementFactory?>(
            nameof(ElementFactory),
            o => o.ElementFactory,
            (o, v) => o.ElementFactory = v);

    public static readonly DirectProperty<TreeDataGridRow, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<TreeDataGridRow, bool>(
            nameof(IsSelected),
            o => o.IsSelected);

    public static readonly DirectProperty<TreeDataGridRow, IRows?> RowsProperty =
        AvaloniaProperty.RegisterDirect<TreeDataGridRow, IRows?>(
            nameof(Rows),
            o => o.Rows);

    private IColumns? _columns;
    private TreeDataGridElementFactory? _elementFactory;
    private bool _isSelected;
    private IRows? _rows;
    private Point _mouseDownPosition = s_InvalidPoint;
    private PointerPressedEventArgs? _lastPointerPressed;
    private TreeDataGrid? _treeDataGrid;

    public IColumns? Columns
    {
        get => _columns;
        private set => SetAndRaise(ColumnsProperty, ref _columns, value);
    }

    public TreeDataGridElementFactory? ElementFactory
    {
        get => _elementFactory;
        set => SetAndRaise(ElementFactoryProperty, ref _elementFactory, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        private set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }

    public object? Model => DataContext;

    public IRows? Rows
    {
        get => _rows;
        private set => SetAndRaise(RowsProperty, ref _rows, value);
    }

    public TreeDataGridCellsPresenter? CellsPresenter { get; private set; }
    public int RowIndex { get; private set; }

    public void Realize(
        TreeDataGridElementFactory? elementFactory,
        ITreeDataGridSelectionInteraction? selection,
        IColumns? columns,
        IRows? rows,
        int rowIndex)
    {
        ElementFactory = elementFactory;
        Columns = columns;
        Rows = rows;
        DataContext = rows?[rowIndex].Model;
        IsSelected = selection?.IsRowSelected(rowIndex) ?? false;
        RowIndex = rowIndex;
        UpdateSelection(selection);
        CellsPresenter?.Realize(rowIndex);
        _treeDataGrid?.RaiseRowPrepared(this, RowIndex);
    }

    public Control? TryGetCell(int columnIndex)
    {
        return CellsPresenter?.TryGetElement(columnIndex);
    }

    public void UpdateIndex(int index)
    {
        if (RowIndex == -1)
            throw new InvalidOperationException("Row is not realized.");

        RowIndex = index;
        CellsPresenter?.UpdateRowIndex(index);
    }

    public void Unrealize()
    {
        _treeDataGrid?.RaiseRowClearing(this, RowIndex);
        RowIndex = -1;
        DataContext = null;
        IsSelected = false;
        CellsPresenter?.Unrealize();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _treeDataGrid = this.FindLogicalAncestorOfType<TreeDataGrid>();
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _treeDataGrid = null;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // The row may be realized before being parented. In this case raise the RowPrepared event here.
        if (_treeDataGrid is not null && RowIndex >= 0)
            _treeDataGrid.RaiseRowPrepared(this, RowIndex);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        CellsPresenter = e.NameScope.Find<TreeDataGridCellsPresenter>("PART_CellsPresenter");

        if (RowIndex >= 0)
            CellsPresenter?.Realize(RowIndex);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _mouseDownPosition = e.Handled ? s_InvalidPoint : e.GetPosition(this);
        _lastPointerPressed = e;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var currentPoint = e.GetCurrentPoint(this);
        var delta = currentPoint.Position - _mouseDownPosition;

        var pointerSupportsDrag = currentPoint.Pointer.Type switch
        {
            PointerType.Mouse => currentPoint.Properties.IsLeftButtonPressed,
            PointerType.Pen => currentPoint.Properties.IsRightButtonPressed,
            _ => false
        };

        if (!pointerSupportsDrag ||
            e.Handled ||
            double.Abs(delta.X) < DragDistance && double.Abs(delta.Y) < DragDistance ||
            _mouseDownPosition == s_InvalidPoint)
            return;

        _mouseDownPosition = s_InvalidPoint;

        // The press that started the gesture is the drag trigger; release it either way so
        // that the row doesn't keep the event args (and the pointer they reference) alive.
        var trigger = _lastPointerPressed;
        _lastPointerPressed = null;

        if (trigger is null ||
            Parent is not TreeDataGridRowsPresenter presenter ||
            presenter.TemplatedParent is not TreeDataGrid owner)
            return;

        StartDrag(owner, trigger);
    }

    /// <summary>
    /// Starts a drag which runs until the user drops or cancels it. Nothing awaits the
    /// operation, so log any failure rather than leaving the exception unobserved.
    /// </summary>
    private static async void StartDrag(TreeDataGrid owner, PointerPressedEventArgs trigger)
    {
        try
        {
            await owner.RaiseRowDragStarted(trigger);
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(
                owner,
                "Row drag and drop failed: {Exception}",
                ex);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _mouseDownPosition = s_InvalidPoint;
        _lastPointerPressed = null;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _mouseDownPosition = s_InvalidPoint;
        _lastPointerPressed = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsSelectedProperty)
        {
            PseudoClasses.Set(":selected", IsSelected);
        }
        
        base.OnPropertyChanged(change);
    }

    internal void UpdateSelection(ITreeDataGridSelectionInteraction? selection)
    {
        IsSelected = selection?.IsRowSelected(RowIndex) ?? false;
        CellsPresenter?.UpdateSelection(selection);
    }

    public void UnrealizeOnItemRemoved()
    {
        _treeDataGrid?.RaiseRowClearing(this, RowIndex);
        RowIndex = -1;
        DataContext = null;
        IsSelected = false;
        CellsPresenter?.UnrealizeOnRowRemoved();
    }
}
