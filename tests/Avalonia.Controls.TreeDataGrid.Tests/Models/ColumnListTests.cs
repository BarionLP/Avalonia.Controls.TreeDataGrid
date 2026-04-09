using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.TreeDataGridTests.Models;

public class ColumnListTests
{
    [Test]
    public async Task Columns_Are_Sized_At_End_Of_Measure()
    {
        var target = new ColumnList<Model>
        {
            new TextColumn<Model, string?>(null, x => x.Name, new GridLength(100, GridUnitType.Pixel)),
            new TextColumn<Model, string?>(null, x => x.Name, GridLength.Auto),
            new TextColumn<Model, string?>(null, x => x.Name, new GridLength(1, GridUnitType.Star)),
            new TextColumn<Model, string?>(null, x => x.Name, new GridLength(3, GridUnitType.Star)),
        };

        target.ViewportChanged(new Rect(0, 0, 500, 500));

        for (var row = 0; row < 10; ++row)
        {
            for (var col = 0; col < target.Count; ++col)
            {
                target.CellMeasured(col, row, new Size(51 + row, 10));
            }
        }

        target.CommitActualWidths();

        await Assert.That(target[0].ActualWidth).IsEqualTo(100);
        await Assert.That(target[1].ActualWidth).IsEqualTo(60);
        await Assert.That(target[2].ActualWidth).IsEqualTo(85);
        await Assert.That(target[3].ActualWidth).IsEqualTo(255);
    }

    [Test]
    public async Task Layout_Is_Invalidated_At_End_Of_Measure_If_AutoSized_Column_Changes_Width()
    {
        var target = new ColumnList<Model>
        {
            new TextColumn<Model, string?>(null, x => x.Name, GridLength.Auto),
            new TextColumn<Model, string?>(null, x => x.Country, GridLength.Auto),
        };


        target.ViewportChanged(new Rect(0, 0, 500, 500));

        for (var row = 0; row < 10; ++row)
        {
            for (var col = 0; col < target.Count; ++col)
            {
                target.CellMeasured(col, row, new Size(40, 10));
            }
        }

        target.CommitActualWidths();

        target.CellMeasured(0, 1, new Size(50, 10));

        var raised = 0;
        target.LayoutInvalidated += (s, e) => ++raised;

        target.CommitActualWidths();

        await Assert.That(raised).IsEqualTo(1);
    }

    private class Model
    {
        public string? Name { get; set; }
        public string? Country { get; set; }
    }
}
