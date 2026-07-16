using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

namespace Avalonia.Controls.TreeDataGridTests;

public class HierarchicalTreeDataGridSourceTests
{
    public class RowsAndCells
    {
        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public Task Creates_Cells_For_Root_Models(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            return AssertState(target, data, 5, sorted);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public Task Expanding_Root_Node_Creates_Child_Cells(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));

            return AssertState(target, data, 10, sorted, new IndexPath(0));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Collapsing_Root_Node_Removes_Child_Cells(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            target.Collapse(new IndexPath(0));

            await AssertState(target, data, 5, sorted);
        }

        [Test]
        public async Task Replacing_Expanded_Row_Detaches_Old_Row_From_Child_Models()
        {
            var data = CreateData(2, 2);
            var target = CreateTarget(data, false);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(4);

            var oldNode = data[0];
            data[0] = new Node { Id = 100, Caption = "Replacement", Children = [] };

            await Assert.That(target.Rows.Count).IsEqualTo(2);

            // Changing the old (replaced) node's children must not affect the grid.
            oldNode.Children!.Add(new Node { Id = 101, Caption = "New Child", Children = [] });

            await Assert.That(target.Rows.Count).IsEqualTo(2);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Adding_Root_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data.Add(new Node { Id = 100, Caption = "New Node 1" });

            await AssertState(target, data, 6, sorted);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Inserting_Root_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data.Insert(1, new Node { Id = 100, Caption = "New Node 1" });

            await AssertState(target, data, 6, sorted);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Removing_Root_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data.RemoveAt(1);

            await AssertState(target, data, 4, sorted);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Removing_Root_Row_With_Earlier_Row_Expanded_To_Grandchildren(bool sorted)
        {
            var data = CreateData();
            data[0].Children![0].Children =
            [
                new() {
                    Id = 100,
                    Caption = "Node 0-0-0",
                }
            ];

            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));
            target.Expand(new IndexPath(0, 0));

            await Assert.That(target.Rows.Count).IsEqualTo(11);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data.RemoveAt(1);

            await AssertState(target, data, 10, sorted, new IndexPath(0), new IndexPath(0, 0));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Removing_Root_Row_With_Later_Row_Expanded(bool sorted)
        {
            var data = CreateData();

            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(4));

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data.RemoveAt(1);

            await AssertState(target, data, 9, sorted, new IndexPath(3));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Removing_Expanded_Root_Row_Unsubscribes_From_CollectionChanged(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);
            var toRemove = data[1];

            target.Expand(1);
            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(1);

            data.RemoveAt(1);
            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(0);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Removing_Expanded_Root_Row_With_Expanded_Child_Unsubscribes_From_CollectionChanged(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);
            var toRemove = data[1].Children![1];

            toRemove.Children = [new Node()];

            target.Expand(new IndexPath(1, 1));
            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(1);

            data.RemoveAt(1);
            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(0);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Adding_Child_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data[0].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

            await AssertState(target, data, 11, sorted, new IndexPath(0));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Adding_Child_To_Expanded_Then_Unexpanded_Root_Node(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));
            target.Collapse(new IndexPath(0));

            data[0].Children!.Add(new Node { Id = 100, Caption = "New Node 1" });

            await AssertState(target, data, 5, sorted);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Inserting_Child_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data[0].Children!.Insert(1, new Node { Id = 100, Caption = "New Node 1" });

            await AssertState(target, data, 11, sorted, new IndexPath(0));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Removing_Child_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));
            await Assert.That(target.Rows.Count).IsEqualTo(10);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data[0].Children!.RemoveAt(3);

            await AssertState(target, data, 9, sorted, new IndexPath(0));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Removing_Child_Rows_At_Start(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            target.Expand(new IndexPath(0));
            await Assert.That(target.Rows.Count).IsEqualTo(10);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data[0].Children!.RemoveRange(0, 2);

            await AssertState(target, data, 8, sorted, new IndexPath(0));
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Replacing_Root_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data[2] = new Node { Id = 100, Caption = "Replaced" };

            await AssertState(target, data, 5, sorted);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Supports_Moving_Root_Row(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            var raised = 0;
            target.Rows.CollectionChanged += (s, e) => ++raised;

            data.Move(2, 4);

            await AssertState(target, data, 5, sorted);
        }

        [Test]
        public async Task Setting_Sort_Updates_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data, false);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            target.Sort((x, y) => y!.Id - x!.Id);

            await AssertState(target, data, 10, true, new IndexPath(0));
        }

        [Test]
        public async Task Clearing_Sort_Updates_Rows()
        {
            var data = CreateData();
            var target = CreateTarget(data, true);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(10);

            target.Sort(null);

            await AssertState(target, data, 10, false, new IndexPath(0));
        }
    }

    public class Expansion
    {
        [Test]
        public async Task Expanding_Updates_Cell_IsExpanded()
        {
            var data = CreateData();
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);
            var raised = 0;

            expander.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "IsExpanded")
                    ++raised;
            };

            target.Expand(new IndexPath(0));

            await Assert.That(expander.IsExpanded).IsTrue();
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Expanding_Previously_Expanded_Node_Creates_Expanded_Descendent()
        {
            var data = CreateData();
            var target = CreateTarget(data, false);

            data[0].Children![0].Children =
            [
                new Node { Id = 100, Caption = "Grandchild" }
            ];

            // Expand first root node.
            target.Expand(new IndexPath(0));

            await AssertState(target, data, 10, false, new IndexPath(0));

            // Expand first child node.
            target.Expand(new IndexPath(0, 0));

            // Grandchild should now be visible.
            await AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 0));

            // Collapse root node.
            target.Collapse(new IndexPath(0));
            await AssertState(target, data, 5, false);

            // And expand again. Grandchild should now be visible once more.
            target.Expand(new IndexPath(0));
            await AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 0));
        }

        [Test]
        public async Task Shows_Expander_For_Row_With_Children()
        {
            var data = CreateData();
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

            await Assert.That(expander.ShowExpander).IsTrue();
        }

        [Test]
        public async Task Hides_Expander_For_Row_Without_Children()
        {
            var data = new[] { new Node { Id = 0, Caption = "Node 0" } };
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

            await Assert.That(expander.ShowExpander).IsFalse();
        }

        [Test]
        public async Task Attempting_To_Expand_Node_That_Has_No_Children_Hides_Expander()
        {
            var data = new Node { Id = 0, Caption = "Node 0" };

            // Here we return true from hasChildren selector, but there are actually no children.
            // This may happen if calculating the children is expensive.
            var target = new HierarchicalTreeDataGridSource<Node>(data)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<Node>(
                        new TextColumn<Node, int>("ID", x => x.Id),
                        x => x.Children,
                        x => true),
                    new TextColumn<Node, string?>("Caption", x => x.Caption),
                }
            };

            var expander = (IExpanderCell)target.Rows.RealizeCell(target.Columns[0], 0, 0);

            target.Expand(new IndexPath(0));

            await Assert.That(expander.ShowExpander).IsFalse();
            await Assert.That(expander.IsExpanded).IsFalse();
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task ExpandAll_Expands_All_Rows(bool sorted)
        {
            var data = CreateData(5, 3, 3);
            var target = CreateTarget(data, sorted);

            target.ExpandAll();

            await Assert.That(target.Rows.Count).IsEqualTo(65);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task CollapseAll_Collapses_All_Rows(bool sorted)
        {
            var data = CreateData(5, 3, 3);
            var target = CreateTarget(data, sorted);

            // We need to expand before we can collapse.
            target.ExpandAll();
            await Assert.That(target.Rows.Count).IsEqualTo(65);

            // Now we can test collapsing.
            target.CollapseAll();
            await Assert.That(target.Rows.Count).IsEqualTo(5);

            // Ensure that nested rows were collapsed, i.e. only the first level of rows is
            // visible after expanding now.
            target.Expand(0);
            await Assert.That(target.Rows.Count).IsEqualTo(8);
        }
    }

    [Test]
    public async Task Adding_Second_Expander_Column_Throws()
    {
        var data = CreateData();
        var target = CreateTarget(data, false);

        Assert.Throws<InvalidOperationException>(() =>
        {
            target.Columns.Add(new HierarchicalExpanderColumn<Node>(
                new TextColumn<Node, int>("ID", x => x.Id),
                x => x.Children,
                null,
                x => x.IsExpanded));
        });
    }

    [Test]
    public async Task Removing_Expander_Column_Throws()
    {
        var data = CreateData();
        var target = CreateTarget(data, false);

        var expander = target.Columns.OfType<IExpanderColumn<Node>>().First();

        Assert.Throws<InvalidOperationException>(() =>
        {
            target.Columns.Remove(expander);
        });
    }

    public class ExpansionBinding
    {
        [Test]
        public async Task Root_Is_Initially_Expanded()
        {
            var data = CreateData();
            data[0].IsExpanded = true;

            var target = CreateTarget(data, false, bindExpanded: true);
            RealizeCells(target);

            await AssertState(target, data, 10, false, new IndexPath(0));
        }

        [Test]
        public async Task Child_Is_Initially_Expanded()
        {
            var data = CreateData();
            data[0].IsExpanded = true;
            data[0].Children![1].IsExpanded = true;
            data[0].Children![1].Children!.Add(new Node());

            var target = CreateTarget(data, false, bindExpanded: true);
            RealizeCells(target);

            await AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 1));
        }

        [Test]
        public async Task Handles_Initial_Expanded_Row_With_No_Children()
        {
            var data = CreateData();
            data[0].IsExpanded = true;

            // This node has no children.
            data[0].Children![1].IsExpanded = true;

            var target = CreateTarget(data, false, bindExpanded: true);
            RealizeCells(target);

            await AssertState(target, data, 10, false, new IndexPath(0));
        }

        [Test]
        public async Task Root_Can_Be_Expanded_Via_Model()
        {
            var data = CreateData();
            var target = CreateTarget(data, false, bindExpanded: true);

            RealizeCells(target);
            await AssertState(target, data, 5, false);

            data[0].IsExpanded = true;

            await AssertState(target, data, 10, false, new IndexPath(0));
        }

        [Test]
        public async Task Child_Can_Be_Expanded_Via_Model()
        {
            var data = CreateData();
            data[0].Children![1].Children!.Add(new Node());

            var target = CreateTarget(data, false, bindExpanded: true);

            RealizeCells(target);
            await AssertState(target, data, 5, false);

            data[0].IsExpanded = true;
            await RealizeRow(target, new IndexPath(0, 1));
            data[0].Children![1].IsExpanded = true;

            await AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 1));
        }

        [Test]
        public async Task Expanding_Collapsing_Root_Row_Writes_To_Model()
        {
            var data = CreateData();
            var target = CreateTarget(data, false, bindExpanded: true);

            RealizeCells(target);
            await AssertState(target, data, 5, false);

            ((IExpander)target.Rows[0]).IsExpanded = true;

            await AssertState(target, data, 10, false, new IndexPath(0));

            ((IExpander)target.Rows[0]).IsExpanded = false;

            await AssertState(target, data, 5, false);
        }

        [Test]
        public async Task Expanding_Collapsing_Child_Row_Writes_To_Model()
        {
            var data = CreateData();
            data[0].Children![1].Children!.Add(new Node());

            var target = CreateTarget(data, false, bindExpanded: true);

            RealizeCells(target);
            await AssertState(target, data, 5, false);

            ((IExpander)target.Rows[0]).IsExpanded = true;
            ((IExpander)target.Rows[2]).IsExpanded = true;

            await AssertState(target, data, 11, false, new IndexPath(0), new IndexPath(0, 1));

            ((IExpander)target.Rows[2]).IsExpanded = false;

            await AssertState(target, data, 10, false, new IndexPath(0));
        }

        private static async Task AssertState(
            HierarchicalTreeDataGridSource<Node> target,
            IList<Node> data,
            int expectedRows,
            bool sorted,
            params IndexPath[] expanded)
        {
            await HierarchicalTreeDataGridSourceTests.AssertState(target, data, expectedRows, sorted, expanded);
            await AssertDataState(default, data, expanded);
        }

        private static async Task AssertDataState(IndexPath parentIndex, IList<Node> data, IndexPath[] expanded)
        {
            for (var i = 0; i < data.Count; ++i)
            {
                var node = data[i];
                var nodeIndex = parentIndex.Append(i);
                await Assert.That(node.IsExpanded).IsEqualTo(expanded.Contains(nodeIndex));

                if (node.Children is not null)
                {
                    await AssertDataState(nodeIndex, node.Children, expanded);
                }
            }
        }

        private static void RealizeCells(HierarchicalTreeDataGridSource<Node> target)
        {
            for (var c = 0; c < target.Columns.Count; c++)
            {
                var column = target.Columns[c];
                for (var r = 0; r < target.Rows.Count; ++r)
                    target.Rows.RealizeCell(column, c, r);
            }
        }

        private static async Task RealizeRow(
            HierarchicalTreeDataGridSource<Node> target,
            IndexPath modelIndex)
        {
            var rowIndex = target.Rows.ModelIndexToRowIndex(modelIndex);

            await Assert.That(rowIndex).IsNotEqualTo(-1);

            for (var c = 0; c < target.Columns.Count; c++)
            {
                var column = target.Columns[c];
                target.Rows.RealizeCell(column, c, rowIndex);
            }
        }
    }

    public class ShowExpander
    {
        [Test]
        public async Task Initially_Hides_Expander_With_No_Children()
        {
            var data = CreateData(count: 1, childCount: 0);
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

            await Assert.That(expander.ShowExpander).IsFalse();
        }

        [Test]
        public async Task Initially_Shows_Expander_With_Children()
        {
            var data = CreateData(count: 1, childCount: 1);
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

            await Assert.That(expander.ShowExpander).IsTrue();
        }

        [Test]
        public async Task Shows_Expander_When_First_Child_Added()
        {
            var data = CreateData(count: 1, childCount: 0);
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);
            var raised = 0;

            expander.PropertyChanged += (s, e) =>
            {
                // await Assert.That(e.PropertyName).IsEqualTo("ShowExpander");
                ++raised;
            };

            data[0].Children!.Add(new Node());

            await Assert.That(expander.ShowExpander).IsTrue();
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Hides_Expander_When_Last_Child_Removed()
        {
            var data = CreateData(count: 1, childCount: 1);
            var target = CreateTarget(data, false);
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);
            var raised = 0;

            expander.PropertyChanged += (s, e) =>
            {
                // Assert.Equal("ShowExpander", e.PropertyName);
                ++raised;
            };

            data[0].Children!.RemoveAt(0);

            await Assert.That(expander.ShowExpander).IsFalse();
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Cell_Synchronizes_Row_ShowExpander()
        {
            var data = CreateData(count: 1, childCount: 1);
            var target = CreateTarget(data, false);
            var row = (HierarchicalRow<Node>)target.Rows[0];
            var expander = (ExpanderCell<Node>)target.Rows.RealizeCell(target.Columns[0], 0, 0);

            await Assert.That(expander.ShowExpander).IsTrue();
            await Assert.That(row.ShowExpander).IsTrue();

            data[0].Children!.RemoveAt(0);

            await Assert.That(expander.ShowExpander).IsFalse();
            await Assert.That(row.ShowExpander).IsFalse();
        }
    }

    public class Selection
    {
        [Test]
        public async Task Reassigning_Source_Updates_Selection_Model_Source()
        {
            var data1 = CreateData();
            var data2 = CreateData(5);
            var target = CreateTarget(data1, false);

            // Ensure selection model is created.
            await Assert.That(data1).IsSameReferenceAs(((ITreeDataGridSelection?)target.RowSelection)!.Source);

            target.Items = data2;

            await Assert.That(data2).IsSameReferenceAs(((ITreeDataGridSelection?)target.RowSelection)!.Source);
        }
    }

    public class Items
    {
        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Can_Reassign_Items(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);
            var raised = 0;

            await Assert.That(target.Rows.Count).IsEqualTo(5);

            target.Rows.CollectionChanged += (s, e) =>
            {
                // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
                ++raised;
            };

            target.Items = CreateData(10);

            await Assert.That(target.Rows.Count).IsEqualTo(10);
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Can_Reassign_Items_With_Expanded_Node(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);
            var raised = 0;

            target.Expand(0);
            await Assert.That(target.Rows.Count).IsEqualTo(10);

            target.Rows.CollectionChanged += (s, e) =>
            {
                // await Assert.That(e.Action).IsEqualTo(NotifyCollectionChangedAction.Reset);
                ++raised;
            };

            target.Items = CreateData(12);

            await Assert.That(target.Rows.Count).IsEqualTo(12);
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Reassigning_Items_With_Expanded_Root_Node_Unsubscribes_From_CollectionChanged(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);
            var toRemove = data[1];

            target.Expand(1);
            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(1);

            target.Items = CreateData(12);

            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(0);
        }

        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task Reassigning_Items_With_Expanded_Child_Node_Unsubscribes_From_CollectionChanged(bool sorted)
        {
            var data = CreateData();
            var target = CreateTarget(data, sorted);
            var toRemove = data[1].Children![1];

            toRemove.Children = new AvaloniaListDebug<Node> { new Node() };

            target.Expand(new IndexPath(1, 1));
            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(1);

            target.Items = CreateData(12);

            await Assert.That(toRemove.Children!.CollectionChangedSubscriberCount()).IsEqualTo(0);
        }

        [Test]
        public async Task Selects_Correct_Item_After_Items_Reassigned()
        {
            var data = CreateData();
            var target = CreateTarget(data, false);
            var raised = 0;

            target.RowSelection!.Select(new IndexPath(1, 0));

            var newData = CreateData(10);
            newData[1].Children![0].Caption = "New Selection";
            target.Items = newData;

            target.RowSelection!.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(1, 0));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("New Selection");
                ++raised;
            };

            target.RowSelection!.Select(new IndexPath(1, 0));

            await Assert.That(raised).IsEqualTo(1);
        }
    }

    public class Filtered
    {
        [Test]
        public async Task Filter_Filters_Expanded_Children()
        {
            var data = CreateData(2, 2);
            var target = CreateTarget(data, false);

            target.ExpandAll();

            await Assert.That(target.Rows.Count).IsEqualTo(6);

            var visible = AllNodes(data);
            visible.Remove(data[0].Children![1]);

            target.Filter(visible.Contains);

            await Assert.That(target.Rows.Count).IsEqualTo(5);
        }

        [Test]
        public async Task RefreshFilter_Applies_Filter_To_Children_Of_Previously_Hidden_Row()
        {
            var data = CreateData(2, 2);
            var target = CreateTarget(data, false);

            target.ExpandAll();

            var visible = AllNodes(data);
            Func<Node, bool> predicate = visible.Contains;

            // Hide the second root; its children remain in the visible set.
            visible.Remove(data[1]);
            target.Filter(predicate);

            await Assert.That(target.Rows.Count).IsEqualTo(3);

            // Make the second root visible again, but hide one of its children.
            visible.Add(data[1]);
            visible.Remove(data[1].Children![0]);
            target.RefreshFilter();

            var models = target.Rows.Cast<HierarchicalRow<Node>>().Select(x => x.Model).ToList();

            await Assert.That(target.Rows.Count).IsEqualTo(5);
            await Assert.That(models.Contains(data[1].Children![1])).IsTrue();
            await Assert.That(models.Contains(data[1].Children![0])).IsFalse();
        }

        [Test]
        public async Task Expanding_Row_After_Filter_Applies_Filter_To_Children()
        {
            var data = CreateData(2, 2);
            var target = CreateTarget(data, false);

            await Assert.That(target.Rows.Count).IsEqualTo(2);

            var visible = AllNodes(data);
            visible.Remove(data[0].Children![1]);
            target.Filter(visible.Contains);

            target.Expand(new IndexPath(0));

            // Root 0, its single visible child, and root 1.
            await Assert.That(target.Rows.Count).IsEqualTo(3);
        }

        [Test]
        public async Task Adding_Item_To_Filtered_Unsorted_Source_Inserts_Row_At_Correct_Position()
        {
            var data = CreateData(3, 1);
            var target = CreateTarget(data, false);

            await Assert.That(target.Rows.Count).IsEqualTo(3);

            var visible = AllNodes(data);
            target.Filter(visible.Contains);

            var newNode = new Node { Id = 100, Caption = "New Node", Children = [] };
            visible.Add(newNode);
            data.Insert(1, newNode);

            await Assert.That(target.Rows.Count).IsEqualTo(4);
            await Assert.That(((HierarchicalRow<Node>)target.Rows[1]).Model).IsSameReferenceAs(newNode);
            await Assert.That(((HierarchicalRow<Node>)target.Rows[1]).ModelIndexPath).IsEqualTo(new IndexPath(1));
        }

        [Test]
        public async Task Removing_Item_Updates_Model_Indexes_Of_Rows_Hidden_By_Filter()
        {
            var data = CreateData(4, 1);
            var target = CreateTarget(data, false);

            await Assert.That(target.Rows.Count).IsEqualTo(4);

            var visible = AllNodes(data);
            visible.Remove(data[3]);
            target.Filter(visible.Contains);

            await Assert.That(target.Rows.Count).IsEqualTo(3);

            data.RemoveAt(0);

            // The previously hidden row should now be at model index 2; make it visible again.
            visible.Add(data[2]);
            target.RefreshFilter();

            await Assert.That(target.Rows.Count).IsEqualTo(3);

            var row = (HierarchicalRow<Node>)target.Rows[2];
            await Assert.That(row.Model).IsSameReferenceAs(data[2]);
            await Assert.That(row.ModelIndexPath).IsEqualTo(new IndexPath(2));
        }

        [Test]
        public async Task RefreshFilter_Restores_Expander_When_Children_Become_Visible()
        {
            var data = CreateData(1, 2);
            var target = CreateTarget(data, false);

            var visible = AllNodes(data);
            visible.Remove(data[0].Children![0]);
            visible.Remove(data[0].Children![1]);
            target.Filter(visible.Contains);

            var row = (HierarchicalRow<Node>)target.Rows[0];
            row.IsExpanded = true;

            await Assert.That(row.IsExpanded).IsFalse();
            await Assert.That(row.ShowExpander).IsFalse();

            visible.Add(data[0].Children![0]);
            target.RefreshFilter();

            await Assert.That(row.ShowExpander).IsTrue();

            row.IsExpanded = true;

            await Assert.That(target.Rows.Count).IsEqualTo(2);
        }

        [Test]
        public async Task Expanding_Row_With_All_Children_Filtered_Out_Does_Not_Expand()
        {
            var data = CreateData(1, 2);
            var target = CreateTarget(data, false);

            var visible = AllNodes(data);
            visible.Remove(data[0].Children![0]);
            visible.Remove(data[0].Children![1]);
            target.Filter(visible.Contains);

            target.Expand(new IndexPath(0));

            await Assert.That(target.Rows.Count).IsEqualTo(1);
            await Assert.That(((HierarchicalRow<Node>)target.Rows[0]).IsExpanded).IsFalse();
        }

        [Test]
        public async Task Sorting_Applies_To_Children_Of_Rows_Hidden_By_Filter()
        {
            var data = CreateData(2, 3);
            var target = CreateTarget(data, false);

            target.ExpandAll();

            var visible = AllNodes(data);
            visible.Remove(data[1]);
            target.Filter(visible.Contains);

            target.Sort((x, y) => y!.Id - x!.Id);

            visible.Add(data[1]);
            target.RefreshFilter();

            // The hidden root's children must also be sorted descending by id.
            var models = target.Rows.Cast<HierarchicalRow<Node>>().Select(x => x.Model).ToList();
            var index = models.IndexOf(data[1]);

            await Assert.That(models[index + 1]).IsSameReferenceAs(data[1].Children![2]);
            await Assert.That(models[index + 2]).IsSameReferenceAs(data[1].Children![1]);
            await Assert.That(models[index + 3]).IsSameReferenceAs(data[1].Children![0]);
        }

        [Test]
        public async Task Clearing_Filter_Restores_Hidden_Children()
        {
            var data = CreateData(2, 2);
            var target = CreateTarget(data, false);

            target.ExpandAll();

            var visible = AllNodes(data);
            visible.Remove(data[1]);
            visible.Remove(data[0].Children![0]);
            target.Filter(visible.Contains);

            await Assert.That(target.Rows.Count).IsEqualTo(2);

            target.Filter(null);

            await Assert.That(target.Rows.Count).IsEqualTo(6);
        }

        private static HashSet<Node> AllNodes(IEnumerable<Node> data)
        {
            var result = new HashSet<Node>();

            void Add(IEnumerable<Node> nodes)
            {
                foreach (var node in nodes)
                {
                    result.Add(node);
                    if (node.Children is not null)
                        Add(node.Children);
                }
            }

            Add(data);
            return result;
        }
    }

    private static AvaloniaListDebug<Node> CreateData(int count = 5, int childCount = 5)
    {
        var id = 0;
        var result = new AvaloniaListDebug<Node>();

        for (var i = 0; i < count; ++i)
        {
            var node = new Node
            {
                Id = id++,
                Caption = $"Node {i}",
                Children = [],
            };

            result.Add(node);

            for (var j = 0; j < childCount; ++j)
            {
                node.Children.Add(new Node
                {
                    Id = id++,
                    Caption = $"Node {i}-{j}",
                    Children = [],
                });
            }
        }

        return result;
    }

    private static AvaloniaListDebug<Node> CreateData(params int[] counts)
    {
        var id = 0;

        void Create(int[] counts, int index, IList<Node> result)
        {
            var count = counts[index];

            for (var i = 0; i < count; ++i)
            {
                var node = new Node
                {
                    Id = id++,
                    Caption = $"Node {i}",
                    Children = [],
                };

                if (index < counts.Length - 1)
                    Create(counts, index + 1, node.Children!);

                result.Add(node);
            }
        }

        var result = new AvaloniaListDebug<Node>();
        Create(counts, 0, result);
        return result;
    }

    private static HierarchicalTreeDataGridSource<Node> CreateTarget(
        IEnumerable<Node> roots,
        bool sorted,
        bool bindExpanded = false)
    {
        var result = new HierarchicalTreeDataGridSource<Node>(roots)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<Node>(
                    new TextColumn<Node, int>("ID", x => x.Id),
                    x => x.Children,
                    null,
                    bindExpanded ? x => x.IsExpanded : null),
                new TextColumn<Node, string?>("Caption", x => x.Caption),
            }
        };

        if (sorted)
            result.Sort((x, y) => y!.Id - x!.Id);

        return result;
    }

    private static async Task AssertState(
        HierarchicalTreeDataGridSource<Node> target,
        IList<Node> data,
        int expectedRows,
        bool sorted,
        params IndexPath[] expanded)
    {
        await Assert.That(target.Columns.Count).IsEqualTo(2);
        await Assert.That(target.Rows.Count).IsEqualTo(expectedRows);

        var rowIndex = 0;

        async Task AssertLevel(IndexPath parent, IList<Node> levelData)
        {
            var sortedData = levelData;

            if (sorted)
            {
                var s = new List<Node>(levelData);
                s.Sort((x, y) => y.Id - x.Id);
                sortedData = s;
            }

            for (var i = 0; i < levelData.Count; ++i)
            {
                var modelIndex = parent.Append(levelData.IndexOf(sortedData[i]));
                var model = GetModel(data, modelIndex);
                var row = await Assert.That(target.Rows[rowIndex]).IsTypeOf<HierarchicalRow<Node>>().And.IsNotNull();
                var shouldBeExpanded = expanded.Contains(modelIndex);

                await Assert.That(modelIndex).IsEqualTo(row.ModelIndexPath);
                await Assert.That(row.IsExpanded == shouldBeExpanded).IsTrue();

                ++rowIndex;

                if (row.IsExpanded)
                {
                    Assert.NotNull(model.Children);
                    await AssertLevel(modelIndex, model.Children!);
                }
            }
        }

        await AssertLevel(default, data);
    }

    private static Node GetModel(IList<Node> data, IndexPath path)
    {
        var depth = path.Count;
        Node? node = null;

        if (depth == 0)
            throw new NotSupportedException();

        for (var i = 0; i < depth; ++i)
        {
            var j = path[i];
            node = node is null ? data[j] : node.Children![j];
        }

        return node!;
    }

    internal class Node : NotifyingBase
    {
        private int _id;
        private string? _caption;
        private AvaloniaListDebug<Node>? _children;
        private bool _isExpanded;

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

        public AvaloniaListDebug<Node>? Children
        {
            get => _children;
            set => RaiseAndSetIfChanged(ref _children, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => RaiseAndSetIfChanged(ref _isExpanded, value);
        }
    }
}
