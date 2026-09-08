using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using TUnit.Assertions.Enums;

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

    [Test]
    public async Task SortBy_And_ClearSort_Each_Raise_Sorted_Once()
    {
        var target = CreateTarget(CreateData());
        var column = target.Columns[0];
        var raised = 0;

        target.Sorted += () => ++raised;

        await Assert.That(target.SortBy(column, ListSortDirection.Descending)).IsTrue();
        await Assert.That(raised).IsEqualTo(1);
        await Assert.That(column.SortDirection).IsEqualTo(ListSortDirection.Descending);
        await Assert.That(target.IsSorted).IsTrue();

        target.ClearSort(column);

        await Assert.That(raised).IsEqualTo(2);
        await Assert.That(column.SortDirection).IsNull();
        await Assert.That(target.IsSorted).IsFalse();
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
        public async Task ModelIndexToRowIndex_Works_When_Filtered_But_Not_Sorted()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            target.Filter(x => x.Id % 2 == 0);

            // Visible model indexes are [0, 2, 4, 6, 8].
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(4))).IsEqualTo(2);
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(3)) == -1).IsTrue();
        }

        [Test]
        public async Task Supports_Adding_Row_When_Filtered_But_Not_Sorted()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            target.Filter(x => x.Id % 2 == 0);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            data.Insert(3, new Row { Id = 12, Caption = "New Row" });

            await Assert.That(target.Rows.Count).IsEqualTo(6);
            await Assert.That(((IRow<Row>)target.Rows[2]).Model.Id).IsEqualTo(12);
            await Assert.That(((IRow<Row>)target.Rows[3]).Model.Id).IsEqualTo(4);
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

    /// <summary>
    /// Covers the row indexes and change notifications produced when a source is sorted and
    /// filtered at the same time, where a change to a range of models affects scattered rows.
    /// </summary>
    public class SortedAndFiltered
    {
        [Test]
        public async Task Rows_Are_Sorted_And_Filtered()
        {
            var (target, _) = CreateTarget();

            await Assert.That(RowIds(target)).IsEquivalentTo(new[] { 50, 40, 20, 10, 0 }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task ModelIndexToRowIndex_Maps_Visible_Rows_And_Returns_Minus_One_Otherwise()
        {
            var (target, _) = CreateTarget();

            // Model 5 (id 50) is the first row, model 0 (id 0) the last.
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(5))).IsEqualTo(0);
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(0))).IsEqualTo(4);

            // Hidden by the filter.
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(3))).IsEqualTo(-1);

            // Not in the source at all.
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(99))).IsEqualTo(-1);
            await Assert.That(target.Rows.ModelIndexToRowIndex(default)).IsEqualTo(-1);
        }

        [Test]
        public async Task RowIndexToModelIndex_Round_Trips()
        {
            var (target, _) = CreateTarget();

            for (var i = 0; i < target.Rows.Count; ++i)
            {
                var modelIndex = target.Rows.RowIndexToModelIndex(i);
                await Assert.That(target.Rows.ModelIndexToRowIndex(modelIndex)).IsEqualTo(i);
            }
        }

        [Test]
        public async Task Adding_Visible_Model_Raises_Add_At_Its_Sorted_Row()
        {
            var (target, data) = CreateTarget();
            var changes = RecordChanges(target.Rows);

            // Appended to the source, but sorts between 40 and 20, which are rows 1 and 2.
            data.Add(new Row { Id = 25, Caption = "Row 25" });

            await Assert.That(changes).IsEquivalentTo(
                new[] { new RowChange(NotifyCollectionChangedAction.Add, 2, 25) },
                CollectionOrdering.Matching);
            await Assert.That(RowIds(target)).IsEquivalentTo(new[] { 50, 40, 25, 20, 10, 0 }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task Adding_Filtered_Out_Model_Raises_Nothing()
        {
            var (target, data) = CreateTarget();
            var changes = RecordChanges(target.Rows);

            data.Insert(0, new Row { Id = 30, Caption = "Hidden" });

            await Assert.That(changes).IsEmpty();
            await Assert.That(RowIds(target)).IsEquivalentTo(new[] { 50, 40, 20, 10, 0 }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task Removing_Visible_Model_Raises_Remove_At_Its_Sorted_Row()
        {
            var (target, data) = CreateTarget();
            var changes = RecordChanges(target.Rows);

            data.Remove(data.Single(x => x.Id == 40));

            await Assert.That(changes).IsEquivalentTo(
                new[] { new RowChange(NotifyCollectionChangedAction.Remove, 1, 40) },
                CollectionOrdering.Matching);
            await Assert.That(RowIds(target)).IsEquivalentTo(new[] { 50, 20, 10, 0 }, CollectionOrdering.Matching);
        }

        [Test]
        public async Task Removing_Filtered_Out_Model_Raises_Nothing_But_Remaps_Later_Rows()
        {
            var (target, data) = CreateTarget();
            var changes = RecordChanges(target.Rows);

            // Removing model 3 (id 30) shifts models 4 and 5 (ids 40 and 50) down by one.
            data.Remove(data.Single(x => x.Id == 30));

            await Assert.That(changes).IsEmpty();
            await Assert.That(RowIds(target)).IsEquivalentTo(new[] { 50, 40, 20, 10, 0 }, CollectionOrdering.Matching);
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(4))).IsEqualTo(0);
            await Assert.That(target.Rows.ModelIndexToRowIndex(new IndexPath(3))).IsEqualTo(1);
        }

        private static IReadOnlyList<int> RowIds(FlatTreeDataGridSource<Row> target)
        {
            return Enumerable.Range(0, target.Rows.Count)
                .Select(i => ((IRow<Row>)target.Rows[i]).Model.Id)
                .ToList();
        }

        /// <summary>
        /// Records the changes raised by a rows collection. The rows carried by the event args
        /// are only valid while the event is being raised, so the model is read there.
        /// </summary>
        private static List<RowChange> RecordChanges(IRows rows)
        {
            var result = new List<RowChange>();

            rows.CollectionChanged += (_, e) =>
            {
                var items = e.Action == NotifyCollectionChangedAction.Remove ? e.OldItems : e.NewItems;
                var index = e.Action == NotifyCollectionChangedAction.Remove ? e.OldStartingIndex : e.NewStartingIndex;
                var id = items?.Count > 0 && items[0] is IRow<Row> row ? row.Model.Id : (int?)null;
                result.Add(new RowChange(e.Action, index, id));
            };

            return result;
        }

        private record struct RowChange(NotifyCollectionChangedAction Action, int RowIndex, int? ModelId);

        /// <summary>
        /// Creates a source of models with ids 0, 10, 20, 30, 40 and 50, sorted by descending id
        /// with the model of id 30 filtered out, so that rows are 50, 40, 20, 10, 0. The ids are
        /// spaced so that a model can be added between two existing rows.
        /// </summary>
        private static (FlatTreeDataGridSource<Row> target, AvaloniaList<Row> data) CreateTarget()
        {
            AvaloniaList<Row> data = [.. Enumerable.Range(0, 6).Select(x => new Row { Id = x * 10, Caption = $"Row {x * 10}" })];
            var target = FlatTreeDataGridSourceTests.CreateTarget(data);

            target.SortBy(target.Columns[0], ListSortDirection.Descending);
            target.Filter(x => x.Id != 30);

            // Read the rows so that the map is built and changes are reported per row.
            _ = target.Rows.Count;
            return (target, data);
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
