using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.TreeDataGridTests;

internal class LayoutTestColumn<TModel>(string header, GridLength? width = null, ColumnOptions<TModel>? options = null) : ColumnBase<TModel>(header, width, options ?? DefaultOptions())
{
    public override ICell CreateCell(IRow<TModel> row)
    {
        var indexable = (IModelIndexableRow)row;
        return new TextCell<string>($"{Header} Row {indexable.ModelIndex}");
    }

    public override Comparison<TModel?>? GetComparison(ListSortDirection direction)
    {
        throw new NotImplementedException();
    }

    private static ColumnOptions<TModel> DefaultOptions() => new()
    {
        MinWidth = new GridLength(0, GridUnitType.Pixel)
    };
}
