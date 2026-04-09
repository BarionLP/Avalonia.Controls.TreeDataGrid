// using System.Collections;
// using System.ComponentModel;
// using Avalonia.Collections;
// using Avalonia.Controls.Models.TreeDataGrid;
// using Avalonia.Controls.Primitives;
// using Avalonia.Controls.Templates;
// using Avalonia.Styling;
// using Avalonia.Threading;
// using Avalonia.VisualTree;
// using TUnit.Assertions.Enums;

// namespace Avalonia.Controls.TreeDataGridTests;

// public class TreeDataGridTests_Hierarchical
// {
//     [Test]
//     public async Task Should_Display_Initial_Row_And_Cells()
//     {
//         var (target, _) = CreateTarget();

//         Assert.NotNull(target.RowsPresenter);

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(2);

//         foreach (var row in rows)
//         {
//             var cells = row.CellsPresenter!
//                 .GetVisualChildren()
//                 .Cast<TreeDataGridCell>()
//                 .ToList();
//             await Assert.That(cells.Count).IsEqualTo(2);
//         }
//     }

//     [Test]
//     public async Task Should_Display_Expanded_Root_Node()
//     {
//         var (target, source) = CreateTarget();

//         Assert.NotNull(target.RowsPresenter);
//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(2);
//         await Assert.That(target.RowsPresenter!.GetVisualChildren().Count()).IsEqualTo(2);

//         source.Expand(new IndexPath(0));

//         await Assert.That(source.Rows.Count).IsEqualTo(102);
//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(102);
//         await Assert.That(target.RowsPresenter!.GetVisualChildren().Count()).IsEqualTo(2);

//         Layout(target);

//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(10);
//     }

//     [Test]
//     public async Task Should_Display_Added_Root_Node()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;

//         Layout(target);
//         items.Add(new Model { Id = -1, Title = "Added" });
//         Layout(target);

//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(3);
//         await Assert.That(target.RowsPresenter!.GetVisualChildren().Count()).IsEqualTo(3);
//     }

//     [Test]
//     public async Task Should_Display_Added_Child_Node()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;
//         var children = items[1].Children = new AvaloniaList<Model>
//         {
//             new Model { Id = -1, Title = "First" }
//         };

//         Layout(target);
//         source.Expand(new IndexPath(1));
//         Layout(target);
//         children.Add(new Model { Id = -2, Title = "Second" });
//         Layout(target);

//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(4);
//         await Assert.That(target.RowsPresenter!.GetVisualChildren().Count()).IsEqualTo(4);
//     }

//     [Test]
//     public async Task RowIndexes_Should_Be_Correct_After_Expanding_Node_While_Scrolled()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;
//         var children = items[0].Children![1].Children = new AvaloniaList<Model>
//         {
//             new Model { Id = -1, Title = "First" }
//         };

//         source.Expand(0);
//         target.Scroll!.Offset = new Vector(0, 20);
//         Layout(target);

//         var rowIndexes = target.RowsPresenter!.RealizedElements
//             .OfType<TreeDataGridRow>()
//             .Select(x => x.RowIndex)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(2, 10), CollectionOrdering.Matching);

//         source.Expand(new IndexPath(0, 1));
//         Layout(target);

//         rowIndexes = target.RowsPresenter!.RealizedElements
//             .OfType<TreeDataGridRow>()
//             .Select(x => x.RowIndex)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(2, 10), CollectionOrdering.Matching);
//     }

//     [Test]
//     public async Task Should_Subscribe_To_Models_For_Initial_Rows()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;

//         for (var i = 0; i < items.Count; ++i)
//         {
//             await Assert.That(items[i].PropertyChangedSubscriberCount()).IsEqualTo(2);
//         }
//     }

//     [Test]
//     public async Task Should_Subscribe_To_Models_For_Expanded_Rows()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;

//         source.Expand(new IndexPath(0));
//         Layout(target);

//         await Assert.That(items[0].PropertyChangedSubscriberCount()).IsEqualTo(2);
//         await Assert.That(items[1].PropertyChangedSubscriberCount()).IsEqualTo(0);

//         var children = items[0].Children!;
//         for (var i = 0; i < children.Count; ++i)
//         {
//             var expected = i < 9 ? 2 : 0;
//             await Assert.That(children[i].PropertyChangedSubscriberCount()).IsEqualTo(expected);
//         }
//     }

//     [Test]
//     public async Task Should_Subscribe_To_Correct_Models_After_Scrolling_Down_One_Row()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;

//         source.Expand(new IndexPath(0));
//         Layout(target);
//         target.Scroll!.Offset = new Vector(0, 10);
//         Layout(target);

//         await Assert.That(items[0].PropertyChangedSubscriberCount()).IsEqualTo(0);
//         await Assert.That(items[1].PropertyChangedSubscriberCount()).IsEqualTo(0);

//         var children = items[0].Children!;
//         for (var i = 0; i < children.Count; ++i)
//         {
//             var expected = i < 10 ? 2 : 0;
//             await Assert.That(children[i].PropertyChangedSubscriberCount()).IsEqualTo(expected);
//         }
//     }

//     [Test]
//     public async Task Scrolling_Should_Not_Rebuild_Templates_In_Expander_Columns()
//     {
//         var instantiations = 0;

//         Control Template(Model model, INameScope ns)
//         {
//             ++instantiations;
//             return new Border();
//         }

//         var columns = new IColumn<Model>[]
//         {
//             new HierarchicalExpanderColumn<Model>(
//                 new TemplateColumn<Model>("ID", new FuncDataTemplate<Model>(Template, true)),
//                 x => x.Children,
//                 x => true),
//             new TextColumn<Model, string?>("Title", x => x.Title),
//         };

//         // Create the TreeDataGrid but don't do an initial layout.
//         var (target, source) = CreateTarget(columns: columns, runLayout: false);
//         var items = (IList<Model>)source.Items;

//         // Expand the first root and do the initial layout now.
//         instantiations = 0;
//         source.Expand(new IndexPath(0));
//         InitialLayout(target);
//         await Assert.That(instantiations).IsEqualTo(9);

//         // Scroll down a row.
//         target.Scroll!.Offset = new Vector(0, 10);
//         Layout(target);

//         // Template should have been recycled and not rebuilt.
//         await Assert.That(instantiations).IsEqualTo(9);
//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(10);

//         for (var i = 0; i < 10; ++i)
//         {
//             var row = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[i]!;
//             var cell = (TreeDataGridExpanderCell)row.CellsPresenter!.RealizedElements[0]!;
//             var inner = cell.FindDescendantOfType<TreeDataGridTemplateCell>()!;
//             var innerModel = (TemplateCell)inner.DataContext!;
//             var rowModel = source.Rows[i + 1].Model;

//             await Assert.That(row.DataContext).IsEqualTo(rowModel);
//             await Assert.That(cell.DataContext).IsEqualTo(rowModel);
//             await Assert.That(innerModel.Value).IsEqualTo(rowModel);
//         }
//     }

//     [Test]
//     public async Task Should_Unsubscribe_From_Models_When_Detached_From_Logical_Tree()
//     {
//         var (target, source) = CreateTarget();
//         var items = (IList<Model>)source.Items;

//         ((Window)target.Parent!).Content = null;

//         for (var i = 0; i < items.Count; ++i)
//         {
//             await Assert.That(items[i].PropertyChangedSubscriberCount()).IsEqualTo(0);
//         }
//     }

//     [Test]
//     public async Task Should_Hide_Expander_When_Node_With_No_Children_Expanded()
//     {
//         var (target, source) = CreateTarget();
//         var cell = target.TryGetCell(0, 1);
//         var expander = await Assert.That(cell).IsTypeOf<TreeDataGridExpanderCell>().And.IsNotNull();

//         await Assert.That(expander.IsExpanded).IsFalse();
//         await Assert.That(expander.ShowExpander).IsTrue();

//         expander.IsExpanded = true;

//         await Assert.That(expander.IsExpanded).IsFalse();
//         await Assert.That(expander.ShowExpander).IsFalse();
//     }

//     [Test]
//     public async Task Can_Reassign_Items_When_Displaying_Child_Items_Followed_By_Root_Items()
//     {
//         var (target, source) = CreateTarget();
//         var cell = target.TryGetCell(0, 0);
//         var expander = await Assert.That(cell).IsTypeOf<TreeDataGridExpanderCell>().And.IsNotNull();

//         // Add a a few more root items.
//         ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

//         // Expand the first root item and scroll down such that we're displaying some children
//         // of the first root item together with subsequent root items.
//         source.Expand(new IndexPath(0));
//         Layout(target);
//         target.Scroll!.Offset = new Vector(0, 1970);
//         Layout(target);

//         var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
//         var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
//         var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
//         var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

//         await Assert.That(firstRowModel.Model.Title).IsEqualTo("Item 0-96");
//         await Assert.That(lastRowModel.Model.Title).IsEqualTo("Root 6");

//         // Replace the items with a single item.
//         source.Items =
//         [
//             new Model
//             {
//                 Id = 0,
//                 Title = "Root 0",
//             },
//         ];

//         Layout(target);

//         firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
//         await Assert.That(firstRow.RowIndex).IsEqualTo(0);
//         await Assert.That(target.Scroll!.Offset).IsEqualTo(Vector.Zero);
//     }

//     [Test]
//     public async Task Can_Reassign_Items_When_Displaying_Grandchild_Items_Followed_By_Root_Items()
//     {
//         var (target, source) = CreateTarget();
//         var cell = target.TryGetCell(0, 0);
//         var expander = await Assert.That(cell).IsTypeOf<TreeDataGridExpanderCell>().And.IsNotNull();

//         // Add a a few more root items.
//         ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

//         // Add some grandchildren.
//         ((AvaloniaList<Model>)source.Items)[0].Children!.AddRange(CreateModels("Item 0-0-", 100));

//         // Expand the first child item and scroll down such that we're displaying some children
//         // of the first root item together with subsequent root items.
//         source.Expand(new IndexPath(0, 0));
//         Layout(target);
//         target.Scroll!.Offset = new Vector(0, 1970);
//         Layout(target);

//         var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
//         var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
//         var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
//         var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

//         await Assert.That(firstRowModel.Model.Title).IsEqualTo("Item 0-0-96");
//         await Assert.That(lastRowModel.Model.Title).IsEqualTo("Root 6");

//         // Replace the items with a single item.
//         source.Items =
//         [
//             new Model
//             {
//                 Id = 0,
//                 Title = "Root 0",
//             },
//         ];

//         Layout(target);

//         firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
//         await Assert.That(firstRow.RowIndex).IsEqualTo(0);
//         await Assert.That(target.Scroll!.Offset).IsEqualTo(Vector.Zero);
//     }

//     [Test]
//     public async Task Can_Reset_Items_When_Displaying_Child_Items_Followed_By_Root_Items()
//     {
//         var (target, source) = CreateTarget();
//         var cell = target.TryGetCell(0, 0);
//         var expander = await Assert.That(cell).IsTypeOf<TreeDataGridExpanderCell>().And.IsNotNull();

//         // Add a a few more root items.
//         ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

//         // Expand the first root item and scroll down such that we're displaying some children
//         // of the first root item together with subsequent root items.
//         source.Expand(new IndexPath(0));
//         Layout(target);
//         target.Scroll!.Offset = new Vector(0, 1970);
//         Layout(target);

//         var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
//         var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[^1]!;
//         var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
//         var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

//         await Assert.That(firstRowModel.Model.Title).IsEqualTo("Item 0-96");
//         await Assert.That(lastRowModel.Model.Title).IsEqualTo("Root 6");

//         // Clear the items.
//         ((IList)source.Items).Clear();

//         Layout(target);

//         await Assert.That(target.RowsPresenter!.RealizedElements).IsEmpty();
//         await Assert.That(target.Scroll!.Offset).IsEqualTo(Vector.Zero);
//     }

//     [Test]
//     public async Task Can_Reset_Child_Items_When_Displaying_Grandchild_Items_Followed_By_Root_Items()
//     {
//         var (target, source) = CreateTarget();
//         var cell = target.TryGetCell(0, 0);
//         var expander = await Assert.That(cell).IsTypeOf<TreeDataGridExpanderCell>().And.IsNotNull();

//         // Add a a few more root items.
//         ((AvaloniaList<Model>)source.Items).AddRange(CreateModels("Root ", 5, firstIndex: 2));

//         // Add some grandchildren.
//         ((AvaloniaList<Model>)source.Items)[0].Children!.AddRange(CreateModels("Item 0-0-", 100));

//         // Expand the first child item and scroll down such that we're displaying some children
//         // of the first root item together with subsequent root items.
//         source.Expand(new IndexPath(0, 0));
//         Layout(target);
//         target.Scroll!.Offset = new Vector(0, 1970);
//         Layout(target);

//         var firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.First()!;
//         var lastRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements.Last()!;
//         var firstRowModel = (IRow<Model>)source.Rows[firstRow.RowIndex];
//         var lastRowModel = (IRow<Model>)source.Rows[lastRow.RowIndex];

//         await Assert.That(firstRowModel.Model.Title).IsEqualTo("Item 0-0-96");
//         await Assert.That(lastRowModel.Model.Title).IsEqualTo("Root 6");

//         // Clear the child items.
//         ((AvaloniaList<Model>)source.Items)[0].Children!.Clear();

//         Layout(target);

//         firstRow = (TreeDataGridRow)target.RowsPresenter!.RealizedElements[0]!;
//         await Assert.That(firstRow.RowIndex).IsEqualTo(0);
//         await Assert.That(target.Scroll!.Offset).IsEqualTo(new Vector(0, 0));
//     }

//     [Test]
//     public async Task Can_Remove_Selected_Item()
//     {
//         var (target, source) = CreateTarget();

//         source.Expand(new IndexPath(0, 0));
//         Layout(target);
//         target.RowSelection!.Select(new IndexPath(0, 3));

//         await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(0, 3));

//         ((AvaloniaList<Model>)source.Items)[0].Children!.RemoveAt(3);

//         await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(-1));
//     }

//     [Test]
//     public async Task Can_Remove_Selected_Item_Sorted()
//     {
//         var (target, source) = CreateTarget();
//         target.Source!.SortBy(target.Columns![0], ListSortDirection.Descending);

//         source.Expand(new IndexPath(0, 0));
//         Layout(target);
//         target.RowSelection!.Select(new IndexPath(0, 3));

//         await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(0, 3));

//         ((AvaloniaList<Model>)source.Items)[0].Children!.RemoveAt(3);

//         await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(-1));
//     }

//     [Test]
//     public async Task Should_Recycle_Focused_Cell_When_Row_Collapsed()
//     {
//         // Issue #210.
//         var (target, source) = CreateTarget();

//         source.Expand(new IndexPath(0));
//         Layout(target);

//         await Assert.That(target.RowsPresenter!.RealizedElements.Count).IsEqualTo(10);
//         var row = await Assert.That(target.TryGetRow(1)).IsTypeOf<TreeDataGridRow>().And.IsNotNull();
//         var cell = await Assert.That(row.TryGetCell(0)).IsTypeOf<TreeDataGridExpanderCell>().And.IsNotNull();

//         await Assert.That(cell.RowIndex).IsEqualTo(1);

//         cell.Focus();

//         source.Collapse(new IndexPath(0));

//         await Assert.That(row.RowIndex).IsEqualTo(-1);

//         // At this point, the cell should have been recycled and should have a row index of -1,
//         // otherwise it can may recyled in a subsequent layout pass and the cell will not
//         // be updated correctly.
//         await Assert.That(cell.RowIndex).IsEqualTo(-1);
//     }

//     private static (TreeDataGrid, HierarchicalTreeDataGridSource<Model>) CreateTarget(
//         IEnumerable<IColumn<Model>>? columns = null,
//         bool runLayout = true)
//     {
//         var items = new AvaloniaList<Model>
//         {
//             new Model
//             {
//                 Id = 0,
//                 Title = "Root 0",
//                 Children = new AvaloniaList<Model>(CreateModels("Item 0-", 100))
//             },
//             new Model
//             {
//                 Id = 1,
//                 Title = "Root 1",
//             },
//         };

//         columns ??=
//         [
//             new HierarchicalExpanderColumn<Model>(
//                 new TextColumn<Model, int>("ID", x => x.Id),
//                 x => x.Children,
//                 x => true),
//             new TextColumn<Model, string?>("Title", x => x.Title),
//         ];

//         var source = new HierarchicalTreeDataGridSource<Model>(items);
//         source.Columns.AddRange(columns);

//         var target = new TreeDataGrid
//         {
//             Template = TestTemplates.TreeDataGridTemplate(),
//             Source = source,
//         };

//         var root = new TestWindow(target)
//         {
//             Styles =
//             {
//                 TestTemplates.TreeDataGridExpanderCellStyle,
//                 TestTemplates.TreeDataGridTemplateCellStyle,
//                 new Style(x => x.Is<TreeDataGridRow>())
//                 {
//                     Setters =
//                     {
//                         new Setter(TreeDataGridRow.TemplateProperty, TestTemplates.TreeDataGridRowTemplate()),
//                     }
//                 },
//                 new Style(x => x.Is<TreeDataGridCell>())
//                 {
//                     Setters =
//                     {
//                         new Setter(TreeDataGridCell.HeightProperty, 10.0),
//                     }
//                 }
//             }
//         };

//         if (runLayout)
//             root.UpdateLayout();
//         Dispatcher.UIThread.RunJobs();

//         return (target, source);
//     }

//     private static void Layout(TreeDataGrid target)
//     {
//         target.UpdateLayout();
//     }

//     private static void InitialLayout(TreeDataGrid target)
//     {
//         target.UpdateLayout();
//     }

//     private static IEnumerable<Model> CreateModels(
//         string titlePrefix,
//         int count,
//         int firstIndex = 0,
//         int firstId = 100)
//     {
//         return Enumerable.Range(0, count).Select(x =>
//             new Model
//             {
//                 Id = firstId + firstIndex + x,
//                 Title = titlePrefix + (firstIndex + x),
//             });
//     }

//     private class Model : NotifyingBase
//     {
//         public int Id { get; set; }
//         public string? Title { get; set; }
//         public AvaloniaList<Model>? Children { get; set; }
//     }
// }
