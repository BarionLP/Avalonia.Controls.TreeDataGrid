using System;
using Avalonia.Controls.Primitives;

namespace Avalonia.Controls;

public sealed class TreeDataGridRowEventArgs
{
    public TreeDataGridRowEventArgs(TreeDataGridRow row, int rowIndex)
    {
        Row = row;
        RowIndex = rowIndex;
    }

    internal TreeDataGridRowEventArgs()
    {
        Row = null!;
    }

    public TreeDataGridRow Row { get; private set; }
    public int RowIndex { get; private set; }

    internal void Update(TreeDataGridRow? row, int rowIndex)
    {
        if (row is not null && Row is not null)
        {
            throw new NotSupportedException("Nested TreeDataGrid row prepared/clearing detected.");
        }

        Row = row!;
        RowIndex = rowIndex;
    }
}
