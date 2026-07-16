using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

namespace Avalonia.Controls.TreeDataGridTests;

public class FlatTreeDataGridSourceTests
{
    [Test]
    public async Task Creates_Initial_Rows()
    {
        var data = CreateData();
        var target = CreateTarget(data);

        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Supports_Adding_Row()
    {
        var data = CreateData();
        var target = CreateTarget(data);

        await Assert.That(target.Rows.Count).IsEqualTo(10);

        var raised = 0;
        target.Rows.CollectionChanged += (s, e) =>
        {
            // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Add);
            // await Assert.That(e.NewStartingIndex).IsEqualTo(10);
            ++raised;
        };

        data.Add(new Row { Id = 10, Caption = "New Row 10" });

        await Assert.That(target.Rows.Count).IsEqualTo(11);
        await Assert.That(raised).IsEqualTo(1);

        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Supports_Removing_Row()
    {
        var data = CreateData();
        var target = CreateTarget(data);

        await Assert.That(target.Rows.Count).IsEqualTo(10);

        var raised = 0;
        target.Rows.CollectionChanged += (s, e) =>
        {
            // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Remove);
            // await Assert.That(e.OldStartingIndex).IsEqualTo(5);
            ++raised;
        };

        data.RemoveAt(5);

        await Assert.That(raised).IsEqualTo(1);
        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Supports_Replacing_Row()
    {
        var data = CreateData();
        var target = CreateTarget(data);

        await Assert.That(target.Rows.Count).IsEqualTo(10);

        var raised = 0;
        target.Rows.CollectionChanged += (s, e) =>
        {
            // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Replace);
            // await Assert.That(e.NewStartingIndex).IsEqualTo(5);
            // await Assert.That(e.OldStartingIndex).IsEqualTo(5);
            ++raised;
        };

        data[5] = new Row { Id = 10, Caption = "New Row 10" };

        await Assert.That(raised).IsEqualTo(1);
        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Supports_Moving_Row()
    {
        var data = CreateData();
        var target = CreateTarget(data);

        await Assert.That(target.Rows.Count).IsEqualTo(10);

        var raised = 0;
        target.Rows.CollectionChanged += (s, e) =>
        {
            // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Move);
            // await Assert.That(e.NewStartingIndex).IsEqualTo(8);
            // await Assert.That(e.OldStartingIndex).IsEqualTo(5);
            ++raised;
        };

        data.Move(5, 8);

        await Assert.That(raised).IsEqualTo(1);
        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Supports_Clearing_Rows()
    {
        var data = CreateData();
        var target = CreateTarget(data);

        await Assert.That(target.Rows.Count).IsEqualTo(10);

        var raised = 0;
        target.Rows.CollectionChanged += (s, e) =>
        {
            // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
            ++raised;
        };

        data.Clear();

        await Assert.That(raised).IsEqualTo(1);
        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Can_Reassign_Items()
    {
        var data = CreateData();
        var target = CreateTarget(data);
        var raised = 0;

        await AssertRows(target.Rows, data);

        target.Rows.CollectionChanged += (s, e) =>
        {
            // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
            ++raised;
        };

        target.Items = data = CreateData(20);

        await Assert.That(raised).IsEqualTo(1);
        await AssertRows(target.Rows, data);
    }

    [Test]
    public async Task Raises_Rows_Reset_When_Reassigning_Items_But_Rows_Not_Yet_Read()
    {
        var data = CreateData();
        var target = CreateTarget(data);
        var raised = 0;

        target.Rows.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                ++raised;
        };

        target.Items = CreateData();

        await Assert.That(raised).IsEqualTo(1);
    }

    public class Filtered
    {
        [Test]
        public async Task Filter_Raises_Reset_And_Filters_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            target.Rows.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    ++raised;
            };

            target.Filter(x => x.Id % 2 == 0);

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Rows.Count).IsEqualTo(5);
            await Assert.That(((IRow<Row>)target.Rows[1]).Model.Id).IsEqualTo(2);
        }

        [Test]
        public async Task RefreshFilter_Raises_Reset_And_Reapplies_Filter()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var visible = new HashSet<Row>(data.Where(x => x.Id < 5));
            var raised = 0;

            target.Filter(visible.Contains);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            target.Rows.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    ++raised;
            };

            visible.Remove(data[0]);
            visible.Add(data[7]);
            target.RefreshFilter();

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Rows.Count).IsEqualTo(5);
            await Assert.That(((IRow<Row>)target.Rows[0]).Model.Id).IsEqualTo(1);
        }

        [Test]
        public async Task Clearing_Filter_Raises_Reset_And_Restores_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            target.Filter(x => x.Id % 2 == 0);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            target.Rows.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    ++raised;
            };

            target.Filter(null);

            await Assert.That(raised).IsEqualTo(1);
            await AssertRows(target.Rows, data);
        }
    }

    public class Sorted
    {
        [Test]
        public async Task Sorts_Initial_Cells()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Supports_Adding_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            await AssertRows(target.Rows, data);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Add);
                // await Assert.That(e.NewStartingIndex).IsZero();
                // await Assert.That(e.NewItems!.Count).IsEqualTo(1);
                // await Assert.That(((IModelIndexableRow)e.NewItems[0]!).ModelIndex).IsEqualTo(10);
                ++raised;
            };

            data.Add(new Row { Id = 10, Caption = "New Row 10" });

            await Assert.That(target.Rows.Count).IsEqualTo(11);
            await Assert.That(raised).IsEqualTo(1);

            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Supports_Removing_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            await AssertRows(target.Rows, data);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Remove);
                // await Assert.That(e.OldStartingIndex).IsEqualTo(4);
                // await Assert.That(e.OldItems!.Count).IsEqualTo(1);
                // await Assert.That(((IModelIndexableRow)e.OldItems[0]!).ModelIndex).IsEqualTo(5);
                ++raised;
            };

            data.RemoveAt(5);

            await Assert.That(raised).IsEqualTo(1);
            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Supports_Replacing_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            await AssertRows(target.Rows, data);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                // if (e.Action == NotifyCollectionChangedAction.Remove)
                //     await Assert.That(e.OldStartingIndex).IsEqualTo(4);
                // else if (e.Action == NotifyCollectionChangedAction.Add)
                //     await Assert.That(e.NewStartingIndex).IsZero();
                // else
                //     Assert.Fail("Unexpected collection change");
                ++raised;
            };

            data[5] = new Row { Id = 10, Caption = "New Row 10" };

            await Assert.That(raised).IsEqualTo(2);
            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Supports_Moving_Row()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            await AssertRows(target.Rows, data);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                // if (e.Action == NotifyCollectionChangedAction.Remove)
                //     await Assert.That(e.OldStartingIndex).IsEqualTo(4);
                // else if (e.Action == NotifyCollectionChangedAction.Add)
                //     await Assert.That(e.NewStartingIndex).IsEqualTo(4);
                // else
                //     Assert.Fail("Unexpected collection change");
                ++raised;
            };

            data.Move(5, 8);

            await Assert.That(raised).IsEqualTo(2);
            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Supports_Clearing_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            await AssertRows(target.Rows, data);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) =>
            {
                // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
                ++raised;
            };

            data.Clear();

            await Assert.That(raised).IsEqualTo(1);
            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Can_Reassign_Items()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            await AssertRows(target.Rows, data);

            target.Rows.CollectionChanged += (s, e) =>
            {
                // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
                ++raised;
            };

            target.Items = data = CreateData(20);

            await Assert.That(raised).IsEqualTo(1);
            await AssertRows(target.Rows, data);
        }

        [Test]
        public async Task Raises_Rows_Reset_When_Reassigning_Items_But_Rows_Not_Yet_Read()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            target.Rows.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    ++raised;
            };

            target.Items = CreateData();

            await Assert.That(raised).IsEqualTo(1);
        }

        private static FlatTreeDataGridSource<Row> CreateTarget(IEnumerable<Row> rows)
        {
            var result = FlatTreeDataGridSourceTests.CreateTarget(rows);
            ((AnonymousSortableRows<Row>)result.Rows).Sort(new FuncComparer<Row>(
                new Comparison<Row?>((x, y) => (y?.Id ?? 0) - (x?.Id ?? 0))));
            return result;
        }

        private static async Task AssertRows(IRows rows, IList<Row> data)
        {
            await Assert.That(rows.Count).IsEqualTo(data.Count);

            var sortedData = data.OrderByDescending(x => x.Id).ToList();

            for (var i = 0; i < data.Count; ++i)
            {
                var row = (IRow<Row>)rows[i];
                var indexable = (IModelIndexableRow)row;
                await Assert.That(row.Model).IsSameReferenceAs(sortedData[i]);
                await Assert.That(indexable.ModelIndex).IsEqualTo(data.IndexOf(row.Model));
            }
        }
    }

    public class Selection
    {
        [Test]
        public async Task Reassigning_Source_Updates_Selection_Model_Source()
        {
            var data1 = CreateData();
            var data2 = CreateData(5);
            var target = CreateTarget(data1);

            // Ensure selection model is created.
            await Assert.That(((ITreeDataGridSelection?)target.RowSelection)!.Source!).IsSameReferenceAs(data1);

            target.Items = data2;

            await Assert.That(((ITreeDataGridSelection?)target.RowSelection)!.Source!).IsSameReferenceAs(data2);
        }
    }

    private static FlatTreeDataGridSource<Row> CreateTarget(IEnumerable<Row> rows)
    {
        return new FlatTreeDataGridSource<Row>(rows)
        {
            Columns =
            {
                new TextColumn<Row, int>("ID", x => x.Id),
                new TextColumn<Row, string?>("Caption", x => x.Caption),
            }
        };
    }

    private static AvaloniaList<Row> CreateData(int count = 10)
    {
        var rows = Enumerable.Range(0, count).Select(x => new Row { Id = x, Caption = $"Row {x}" });
        return [.. rows];
    }

    private static async Task AssertRows(IRows rows, IList<Row> data)
    {
        await Assert.That(rows.Count).IsEqualTo(data.Count);

        for (var i = 0; i < data.Count; ++i)
        {
            var row = (IRow<Row>)rows[i];
            var indexable = (IModelIndexableRow)row;
            await Assert.That(data[i]).IsSameReferenceAs(row.Model);
            await Assert.That(indexable.ModelIndex).IsEqualTo(i);
        }
    }

    private class Row : NotifyingBase
    {
        private int _id;
        private string? _caption;

        public int Id 
        {
            get => _id;
            set => RaiseAndSetIfChanged(ref _id, value);
        }

        public string? Caption 
        {
            get => _caption;
            set => RaiseAndSetIfChanged(ref _caption, value);
        }
    }
}
