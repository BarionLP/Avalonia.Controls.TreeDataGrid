// using System.ComponentModel;
// using Avalonia.Collections;
// using Avalonia.Controls.Models.TreeDataGrid;
// using Avalonia.Controls.Primitives;
// using Avalonia.Controls.Selection;
// using Avalonia.Styling;
// using Avalonia.Threading;
// using Avalonia.VisualTree;
// using TUnit.Assertions.Enums;
// using Enumerable = System.Linq.Enumerable;

// namespace Avalonia.Controls.TreeDataGridTests;

// public class TreeDataGridTests_Flat
// {
//     [Test]
//     public async Task Should_Display_Initial_Rows_And_Cells()
//     {
//         var (target, _) = CreateTarget();

//         Assert.NotNull(target.RowsPresenter);

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(10);

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
//     public async Task MultiSelection_Should_Work_Correctly_With_Duplicates()
//     {
//         var items = new List<Model>
//         {
//             new(){ Id=0, Title="Item 0"},
//             new(){ Id=1, Title="Item 1"},
//             new(){ Id=2, Title="Item 2"}
//         };
//         items.Add(items[0]);
//         items.Add(items[0]);
//         items.Add(items[0]);

//         var (target, aaa) = CreateTarget(items);

//         target.RowSelection!.Select(3);
//         target.RowSelection.Select(4);
//         target.RowSelection.Select(5);

//         await AssertInteractionSelection(target, 3, 4, 5);

//         target.Source!.SortBy(target.Columns![0], ListSortDirection.Ascending);

//         await AssertInteractionSelection(target, 1, 2, 3);
//     }

//     [Test]
//     public async Task Selection_Should_Be_Preserved_After_Sorting()
//     {
//         var (target, aaa) = CreateTarget();

//         target.RowSelection!.Select(0);
//         target.RowSelection.Select(5);

//         await AssertInteractionSelection(target, 0, 5);

//         target.Source!.SortBy(target.Columns![0], ListSortDirection.Descending);

//         ///There are 100 items in the collection.
//         ///Their IDs are in range 0..99 so when we order IDs column in Descending order the latest element of the collection would be with
//         ///ID 0(index 99 in collection),first with ID 99
//         await AssertInteractionSelection(target, 94, 99);
//     }

//     [Test]
//     public async Task Should_Subscribe_To_Models_For_Initial_Rows()
//     {
//         var (target, items) = CreateTarget();

//         for (var i = 0; i < items.Count; ++i)
//         {
//             var expected = i < 10 ? 2 : 0;
//             await Assert.That(items[i].PropertyChangedSubscriberCount()).IsEqualTo(expected);
//         }
//     }

//     [Test]
//     public async Task Should_Subscribe_To_Correct_Models_After_Scrolling_Down_One_Row()
//     {
//         var (target, items) = CreateTarget();

//         target.Scroll!.Offset = new Vector(0, 10);
//         Layout(target);

//         for (var i = 0; i < items.Count; ++i)
//         {
//             var expected = i > 0 && i <= 10 ? 2 : 0;
//             await Assert.That(items[i].PropertyChangedSubscriberCount()).IsEqualTo(expected);
//         }
//     }

//     [Test]
//     public async Task Should_Subscribe_To_Correct_Models_After_Scrolling_Down_One_Page()
//     {
//         var (target, items) = CreateTarget();

//         target.Scroll!.Offset = new Vector(0, 100);
//         Layout(target);

//         for (var i = 0; i < items.Count; ++i)
//         {
//             var expected = i >= 10 && i < 20 ? 2 : 0;
//             await Assert.That(items[i].PropertyChangedSubscriberCount()).IsEqualTo(expected);
//         }
//     }

//     [Test]
//     public async Task Should_Unsubscribe_From_Models_When_Detached_From_Logical_Tree()
//     {
//         var (target, items) = CreateTarget();

//         ((Window)target.Parent!).Content = null;

//         for (var i = 0; i < items.Count; ++i)
//         {
//             await Assert.That(items[i].PropertyChangedSubscriberCount()).IsEqualTo(0);
//         }
//     }

//     [Test]
//     public async Task Desired_Width_Should_Be_Total_Of_Fixed_Width_Columns()
//     {
//         var (target, items) = CreateTarget(
//             columns:
//             [
//                 new TextColumn<Model, int>("ID", x => x.Id, new GridLength(10, GridUnitType.Pixel), MinWidth(0)),
//                 new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(14, GridUnitType.Pixel), MinWidth(0))
//             ]
//         );

//         await Assert.That( target.DesiredSize.Width).IsEqualTo(2);
//     }

//     [Test]
//     public async Task Should_Size_Star_Columns()
//     {
//         var (target, items) = CreateTarget(
//             columns:
//             [
//                 new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star), MinWidth(0)),
//                 new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(3, GridUnitType.Star), MinWidth(0))
//             ]
//         );

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(10);

//         foreach (var row in rows)
//         {
//             var cells = row.CellsPresenter!
//                 .GetVisualChildren()
//                 .Cast<TreeDataGridCell>()
//                 .ToList();
//             await Assert.That(cells.Count).IsEqualTo(2);
//             await Assert.That( cells[0].Bounds.Width).IsEqualTo(2);
//             await Assert.That(cells[1].Bounds.Width).IsEqualTo(75);
//         }
//     }

//     [Test]
//     public async Task Should_Size_Star_Columns_With_Min_Width()
//     {
//         var (target, items) = CreateTarget(
//             columns:
//             [
//                 new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star), MinWidth(50)),
//                 new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(3, GridUnitType.Star))
//             ]
//         );

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(10);

//         foreach (var row in rows)
//         {
//             var cells = row.CellsPresenter!
//                 .GetVisualChildren()
//                 .Cast<TreeDataGridCell>()
//                 .ToList();
//             await Assert.That(cells.Count).IsEqualTo(2);
//             await Assert.That(cells[0].Bounds.Width).IsEqualTo(50);
//             await Assert.That(cells[1].Bounds.Width).IsEqualTo(50);
//         }
//     }

//     [Test]
//     public async Task Should_Size_Star_Columns_With_Max_Width()
//     {
//         var (target, items) = CreateTarget(
//             columns:
//             [
//                 new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star)),
//                 new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(1, GridUnitType.Star), MaxWidth(25))
//             ]
//         );

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(10);

//         foreach (var row in rows)
//         {
//             var cells = row?.CellsPresenter!
//                 .GetVisualChildren()
//                 .Cast<TreeDataGridCell>()
//                 .ToList();
//             await Assert.That(cells!.Count).IsEqualTo(2);
//             await Assert.That(cells[0].Bounds.Width).IsEqualTo(75);
//             await Assert.That( cells[1].Bounds.Width).IsEqualTo(2);
//         }
//     }

//     [Test]
//     public async Task Raises_CellPrepared_Events_On_Initial_Layout()
//     {
//         var (target, items) = CreateTarget(runLayout: false);
//         var raised = 0;

//         target.CellPrepared += (s, e) =>
//         {
//             ++raised;
//         };

//         target.UpdateLayout();

//         await Assert.That( raised).IsEqualTo(2);
//     }

//     [Test]
//     public async Task Raises_CellClearing_CellPrepared_Events_On_Scroll()
//     {
//         var (target, items) = CreateTarget();
//         var clearingRaised = 0;
//         var preparedRaised = 0;

//         target.CellClearing += (s, e) =>
//         {
//             // await Assert.That(e.ColumnIndex).IsEqualTo(clearingRaised % 2);
//             // await Assert.That(e.RowIndex).IsEqualTo(0);
//             ++clearingRaised;
//         };

//         target.CellPrepared += (s, e) =>
//         {
//             // await Assert.That(e.ColumnIndex).IsEqualTo(preparedRaised % 2);
//             // await Assert.That(e.RowIndex).IsEqualTo(10);
//             ++preparedRaised;
//         };

//         target.Scroll!.Offset = new Vector(0, 10);
//         Layout(target);

//         await Assert.That(clearingRaised).IsEqualTo(2);
//         await Assert.That(preparedRaised).IsEqualTo(2);
//     }

//     [Test]
//     public async Task Raises_CellValueChanged_When_Model_Value_Changed()
//     {
//         var (target, items) = CreateTarget();
//         var raised = 0;

//         target.CellValueChanged += (s, e) =>
//         {
//             // await Assert.That(e.ColumnIndex).IsEqualTo(1);
//             // await Assert.That(e.RowIndex).IsEqualTo(1);
//             ++raised;
//         };

//         items[1].Title = "Changed";

//         await Assert.That(raised).IsEqualTo(1);
//     }

//     [Test]
//     public async Task Raises_CellValueChanged_After_Cell_Edit()
//     {
//         var (target, items) = CreateTarget();
//         var raised = 0;

//         target.CellValueChanged += (s, e) =>
//         {
//             // await Assert.That(e.ColumnIndex).IsEqualTo(1);
//             // await Assert.That(e.RowIndex).IsEqualTo(1);
//             ++raised;
//         };

//         var cell = await Assert.That(target.TryGetRow(1)?.TryGetCell(1)).IsTypeOf<TreeDataGridTextCell>().And.IsNotNull();
//         cell.BeginEdit();
//         cell.Value = "Changed";

//         await Assert.That(raised).IsEqualTo(0);
//         await Assert.That(items[1].Title).IsEqualTo("Item 1");

//         cell.EndEdit();

//         await Assert.That(items[1].Title).IsEqualTo("Changed");
//         await Assert.That(raised).IsEqualTo(1);
//     }

//     [Test]
//     public async Task Does_Not_Raise_CellValueChanged_Events_On_Initial_Layout()
//     {
//         var (target, items) = CreateTarget(runLayout: false);
//         var raised = 0;

//         target.CellValueChanged += (s, e) => ++raised;

//         target.UpdateLayout();

//         await Assert.That(raised).IsEqualTo(0);
//     }

//     [Test]
//     public async Task Does_Not_Raise_CellValueChanged_Events_On_Scroll()
//     {
//         var (target, items) = CreateTarget();
//         var raised = 0;

//         target.CellValueChanged += (s, e) => ++raised;

//         target.Scroll!.Offset = new Vector(0, 10);
//         Layout(target);

//         await Assert.That(raised).IsEqualTo(0);
//     }

//     [Test]
//     public async Task Does_Not_Realize_Columns_Outside_Viewport()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(1, GridUnitType.Star)),
//             new TextColumn<Model, string?>("Title1", x => x.Title, options: MinWidth(50)),
//             new TextColumn<Model, string?>("Title2", x => x.Title, options: MinWidth(50)),
//             new TextColumn<Model, string?>("Title3", x => x.Title, options: MinWidth(50)),
//         ]);

//         await AssertColumnIndexes(target, 0, 3);

//         var columns = (ColumnList<Model>)target.Columns!;
//         await Assert.That(columns[0].ActualWidth).IsEqualTo(30);
//         await Assert.That(columns[1].ActualWidth).IsEqualTo(50);
//         await Assert.That(columns[2].ActualWidth).IsEqualTo(50);
//         await Assert.That(columns[3].ActualWidth).IsNaN();
//     }

//     [Test]
//     public async Task Header_Column_Indexes_Are_Updated_When_Columns_Are_Updated()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(1, GridUnitType.Star)),
//             new TextColumn<Model, string?>("Title1", x => x.Title,  width: new GridLength(1, GridUnitType.Star)),
//             new TextColumn<Model, string?>("Title2", x => x.Title,  width: new GridLength(1, GridUnitType.Star)),
//             new TextColumn<Model, string?>("Title3", x => x.Title,  width: new GridLength(1, GridUnitType.Star)),
//         ]);

//         await AssertColumnIndexes(target, 0, 4);

//         var source = (FlatTreeDataGridSource<Model>)target.Source!;

//         var movedColumn = source.Columns[1];
//         source.Columns.Remove(movedColumn);

//         await AssertColumnIndexes(target, 0, 3);

//         source.Columns.Add(movedColumn);

//         var root = (TestWindow)TopLevel.GetTopLevel(target)!;
//         root.UpdateLayout();
//         Dispatcher.UIThread.RunJobs();

//         await AssertColumnIndexes(target, 0, 4);
//     }

//     [Test]
//     public async Task Columns_Are_Correctly_Sized_After_Changing_Source()
//     {
//         // Create the initial target with 2 columns and make sure our preconditions are correct.
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(1, GridUnitType.Star)),
//             new TextColumn<Model, string?>("Title1", x => x.Title, options: MinWidth(50)),
//         ]);

//         await AssertColumnIndexes(target, 0, 2);

//         // Create a new source and assign it to the TreeDataGrid.
//         var newSource = new FlatTreeDataGridSource<Model>(items)
//         {
//             Columns =
//             {
//                 new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(1, GridUnitType.Star)),
//                 new TextColumn<Model, string?>("Title1", x => x.Title, options: MinWidth(20)),
//                 new TextColumn<Model, string?>("Title2", x => x.Title, options: MinWidth(20)),
//             }
//         };

//         target.Source = newSource;

//         // The columns should not have an ActualWidth yet.
//         await Assert.That(newSource.Columns[0].ActualWidth).IsNaN();
//         await Assert.That(newSource.Columns[1].ActualWidth).IsNaN();
//         await Assert.That(newSource.Columns[2].ActualWidth).IsNaN();

//         // Do a layout pass and check that the columns have been correctly sized.
//         target.UpdateLayout();
//         await AssertColumnIndexes(target, 0, 3);

//         var columns = (ColumnList<Model>)target.Columns!;
//         await Assert.That(columns[0].ActualWidth).IsEqualTo(60);
//         await Assert.That( columns[1].ActualWidth).IsEqualTo(2);
//         await Assert.That( columns[2].ActualWidth).IsEqualTo(2);
//     }

//     [Test]
//     public async Task Should_Correctly_Align_Columns_When_Vertically_Scrolling_With_First_Column_Unrealized()
//     {
//         // Issue #298
//         static async Task AssertRealizedCells(TreeDataGrid target)
//         {
//             var rows = target.RowsPresenter!.GetVisualChildren().Cast<TreeDataGridRow>();

//             foreach (var row in rows)
//             {
//                 var cells = row.CellsPresenter!.GetRealizedElements()
//                     .Cast<TreeDataGridCell>()
//                     .OrderBy(x => x.ColumnIndex)
//                     .ToList();

//                 await Assert.That(cells.Count).IsEqualTo(3);
//                 await Assert.That(cells[0].ColumnIndex).IsEqualTo(1);
//                 await Assert.That(cells[0].Bounds.Left).IsEqualTo(100);
//                 await Assert.That(cells[1].Bounds.Left).IsEqualTo(150);
//                 await Assert.That(cells[2].Bounds.Left).IsEqualTo(200);
//             }
//         }

//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(100, GridUnitType.Pixel)),
//             new TextColumn<Model, string?>("Title1", x => x.Title, width: new GridLength(50, GridUnitType.Pixel)),
//             new TextColumn<Model, string?>("Title2", x => x.Title, width: new GridLength(50, GridUnitType.Pixel)),
//             new TextColumn<Model, string?>("Title3", x => x.Title, width: new GridLength(50, GridUnitType.Pixel)),
//         ]);

//         // Scroll horizontally and check that the realized cells are positioned correctly.
//         target.Scroll!.Offset = new Vector(120, 0);
//         target.UpdateLayout();
//         await AssertRealizedCells(target);

//         // Scroll down a row and check that the realized cells are positioned correctly.
//         target.Scroll!.Offset = new Vector(120, 10);
//         target.UpdateLayout();
//         await AssertRealizedCells(target);

//         // Now scroll back vertically and check once more.
//         target.Scroll!.Offset = new Vector(120, 0);
//         target.UpdateLayout();
//         await AssertRealizedCells(target);
//     }

//     [Test]
//     public async Task Should_Use_TextCell_StringFormat()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, string?>("Title", x => x.Title, options: new()
//             {
//                 StringFormat = "Hello {0}"
//             }),
//         ]);

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(10);

//         for (var i = 0; i < rows.Count; i++)
//         {
//             var cell = await Assert.That(await Assert.That(rows[i].CellsPresenter!.GetVisualChildren().Cast<TreeDataGridCell>()).IsSingleElement()).IsTypeOf<TreeDataGridTextCell>().And.IsNotNull();
//             await Assert.That(cell.Value).IsEqualTo($"Hello Item {i}");
//         }
//     }

//     [Test]
//     public async Task Should_Use_TextCell_StringFormat_When_Model_Is_Updated()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, string?>("Title", x => x.Title, options: new()
//             {
//                 StringFormat = "Hello {0}"
//             }),
//         ]);

//         var rows = target.RowsPresenter!
//             .GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .ToList();

//         await Assert.That(rows.Count).IsEqualTo(10);
//         items[1].Title = "World";
//         var cell = await Assert.That(target.TryGetCell(0, 1)).IsTypeOf<TreeDataGridTextCell>().And.IsNotNull();

//         await Assert.That(cell.Value).IsEqualTo("Hello World");
//     }

//     public class RemoveItems
//     {
//         [Test]
//         public async Task Can_Remove_Range_Within_Realized_Elements()
//         {
//             var (target, items) = CreateTarget();

//             target.Scroll!.Offset = new Vector(0, 100);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);

//             items.RemoveRange(12, 4);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);
//             await Assert.That(target.Scroll.Offset).IsEqualTo(new Vector(0, 100));
//         }

//         [Test]
//         public async Task Can_Remove_Range_Within_Realized_Elements_When_Scrolled_To_End()
//         {
//             var (target, items) = CreateTarget(itemCount: 20);

//             target.Scroll!.Offset = new Vector(0, 100);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);

//             items.RemoveRange(12, 4);
//             Layout(target);

//             await AssertRowIndexes(target, 6, 10);
//             await Assert.That(target.Scroll.Offset).IsEqualTo(new Vector(0, 60));
//         }

//         [Test]
//         public async Task Can_Remove_Range_Of_All_Realized_Elements()
//         {
//             var (target, items) = CreateTarget();

//             target.Scroll!.Offset = new Vector(0, 100);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);

//             items.RemoveRange(10, 10);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);
//             await Assert.That(target.Scroll.Offset).IsEqualTo(new Vector(0, 100));
//         }

//         [Test]
//         public async Task Can_Remove_Range_Of_All_Realized_Elements_When_Scrolled_To_End()
//         {
//             var (target, items) = CreateTarget(itemCount: 20);

//             target.Scroll!.Offset = new Vector(0, 100);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);

//             items.RemoveRange(10, 10);
//             Layout(target);

//             await AssertRowIndexes(target, 0, 10);
//             await Assert.That(target.Scroll.Offset).IsEqualTo(Vector.Zero);
//         }

//         [Test]
//         public async Task Can_Remove_Range_Spanning_Beginning_Of_Realized_Elements_When_Scrolled_To_End()
//         {
//             var (target, items) = CreateTarget(itemCount: 20);

//             target.Scroll!.Offset = new Vector(0, 100);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);

//             items.RemoveRange(5, 10);
//             Layout(target);

//             await AssertRowIndexes(target, 0, 10);
//             await Assert.That(target.Scroll.Offset).IsEqualTo(Vector.Zero);
//         }

//         [Test]
//         public async Task Can_Remove_Range_Spanning_End_Of_Realized_Elements()
//         {
//             var (target, items) = CreateTarget();

//             target.Scroll!.Offset = new Vector(0, 100);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);

//             items.RemoveRange(15, 10);
//             Layout(target);

//             await AssertRowIndexes(target, 10, 10);
//             await Assert.That(target.Scroll.Offset).IsEqualTo(new Vector(0, 100));
//         }

//         [Test]
//         public async Task Can_Remove_Selected_Item()
//         {
//             var (target, items) = CreateTarget();

//             Layout(target);
//             target.RowSelection!.Select(3);

//             await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(3));

//             items.RemoveAt(3);

//             await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(-1));
//         }

//         [Test]
//         public async Task Can_Remove_Selected_Item_Sorted()
//         {
//             var (target, items) = CreateTarget();
//             target.Source!.SortBy(target.Columns![0], ListSortDirection.Descending);

//             Layout(target);
//             target.RowSelection!.Select(3);

//             await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(3));

//             items.RemoveAt(3);

//             await Assert.That(target.RowSelection.SelectedIndex).IsEqualTo(new IndexPath(-1));
//         }
//     }

//     [Test]
//     public async Task Should_Show_Horizontal_ScrollBar()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(100, GridUnitType.Pixel)),
//             new TextColumn<Model, string?>("Title1", x => x.Title,  width: new GridLength(100, GridUnitType.Pixel)),
//         ]);
//         var scroll = await Assert.That(target.Scroll).IsTypeOf<ScrollViewer>().And.IsNotNull();
//         var headerScroll = await Assert.That(target.GetVisualDescendants().Single(x => x.Name == "PART_HeaderScrollViewer")).IsTypeOf<ScrollViewer>().And.IsNotNull();

//         await Assert.That(scroll.Viewport).IsEqualTo(new(100, 100));
//         await Assert.That(scroll.Extent).IsEqualTo(new(200, 1000));
//         await Assert.That(headerScroll.Viewport).IsEqualTo(new(100, 0));
//         await Assert.That(headerScroll.Extent).IsEqualTo(new(200, 0));
//     }

//     [Test]
//     public async Task Should_Show_Horizontal_ScrollBar_With_No_Initial_Rows()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(100, GridUnitType.Pixel)),
//             new TextColumn<Model, string?>("Title1", x => x.Title,  width: new GridLength(100, GridUnitType.Pixel)),
//         ], itemCount: 0);
//         var scroll = await Assert.That(target.Scroll).IsTypeOf<ScrollViewer>().And.IsNotNull();
//         var headerScroll = await Assert.That(target.GetVisualDescendants().Single(x => x.Name == "PART_HeaderScrollViewer")).IsTypeOf<ScrollViewer>().And.IsNotNull();

//         await Assert.That(scroll.Viewport).IsEqualTo(new(100, 100));
//         await Assert.That(scroll.Extent).IsEqualTo(new(200, 100));
//         await Assert.That(headerScroll.Viewport).IsEqualTo(new(100, 0));
//         await Assert.That(headerScroll.Extent).IsEqualTo(new(200, 0));
//     }

//     [Test]
//     public async Task Should_Preserve_Horizontal_ScrollBar_When_Rows_Removed()
//     {
//         var (target, items) = CreateTarget(columns:
//         [
//             new TextColumn<Model, int>("ID", x => x.Id, width: new GridLength(100, GridUnitType.Pixel)),
//             new TextColumn<Model, string?>("Title1", x => x.Title,  width: new GridLength(100, GridUnitType.Pixel)),
//         ]);
//         var scroll = await Assert.That(target.Scroll).IsTypeOf<ScrollViewer>().And.IsNotNull();
//         var headerScroll = await Assert.That(target.GetVisualDescendants().Single(x => x.Name == "PART_HeaderScrollViewer")).IsTypeOf<ScrollViewer>().And.IsNotNull();

//         scroll.PropertyChanged += (s, e) =>
//         {
//             if (e.Property == ScrollViewer.ExtentProperty)
//             {
//             }
//         };
//         items.Clear();
//         target.UpdateLayout();

//         await Assert.That(scroll.Viewport).IsEqualTo(new(100, 100));
//         await Assert.That(scroll.Extent).IsEqualTo(new(200, 100));
//         await Assert.That(headerScroll.Viewport).IsEqualTo(new(100, 0));
//         await Assert.That(headerScroll.Extent).IsEqualTo(new(200, 0));
//     }

//     private static async Task AssertRowIndexes(TreeDataGrid target, int firstRowIndex, int rowCount)
//     {
//         var presenter = target.RowsPresenter;

//         Assert.NotNull(presenter);

//         var rowIndexes = presenter?.GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.RowIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(firstRowIndex, rowCount), CollectionOrdering.Matching);

//         rowIndexes = presenter!.RealizedElements
//             .Cast<TreeDataGridRow>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.RowIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(firstRowIndex, rowCount), CollectionOrdering.Matching);
//     }

//     private static async Task AssertColumnIndexes(TreeDataGrid target, int firstColumnIndex, int columnCount)
//     {
//         var presenter = target.ColumnHeadersPresenter;

//         Assert.NotNull(presenter);

//         var columnIndexes = presenter?.GetVisualChildren()
//             .Cast<TreeDataGridColumnHeader>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.ColumnIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(columnIndexes).IsEquivalentTo(Enumerable.Range(firstColumnIndex, columnCount), CollectionOrdering.Matching);

//         columnIndexes = presenter!.RealizedElements
//             .Cast<TreeDataGridColumnHeader>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.ColumnIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(columnIndexes).IsEquivalentTo(Enumerable.Range(firstColumnIndex, columnCount), CollectionOrdering.Matching);
//     }

//     private static async Task AssertInteractionSelection(TreeDataGrid target, params int[] selected)
//     {
//         var selection = (ITreeDataGridSelectionInteraction)target.RowSelection!;

//         for (var i = 0; i < target.Rows!.Count; ++i)
//         {
//             await Assert.That(selection.IsRowSelected(i)).IsEqualTo(selected.Contains(i));
//         }
//     }

//     private static (TreeDataGrid, AvaloniaList<Model>) CreateTarget(IEnumerable<Model>? models = null,
//         IEnumerable<IColumn<Model>>? columns = null,
//         int itemCount = 100,
//         bool runLayout = true)
//     {
//         AvaloniaList<Model>? items = null;
//         if (models == null)
//         {
//             items = [.. Enumerable.Range(0, itemCount).Select(x =>
//                 new Model
//                 {
//                     Id = x,
//                     Title = "Item " + x,
//                 })];
//         }
//         else
//         {
//             items = [.. models];
//         }


//         var source = new FlatTreeDataGridSource<Model>(items);
//         source.RowSelection!.SingleSelect = false;

//         if (columns is object)
//         {
//             foreach (var column in columns)
//                 source.Columns.Add(column);
//         }
//         else
//         {
//             source.Columns.Add(new TextColumn<Model, int>("ID", x => x.Id));
//             source.Columns.Add(new TextColumn<Model, string?>("Title", x => x.Title, (o, v) => o.Title = v));
//         }

//         var target = new TreeDataGrid
//         {
//             Template = TestTemplates.TreeDataGridTemplate(),
//             Source = source,
//         };

//         var root = new TestWindow(target)
//         {
//             Styles =
//             {
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
//         {
//             root.UpdateLayout();
//             Dispatcher.UIThread.RunJobs();
//         }

//         return (target, items);
//     }

//     private static void Layout(TreeDataGrid target)
//     {
//         target.UpdateLayout();
//     }

//     private static TextColumnOptions<Model> MinWidth(double min) => new()
//     {
//         MinWidth = new GridLength(min, GridUnitType.Pixel),
//     };

//     private static TextColumnOptions<Model> MaxWidth(double max) => new()
//     {
//         MaxWidth = new GridLength(max, GridUnitType.Pixel),
//     };

//     private class Model : NotifyingBase
//     {
//         private string? _title;

//         public int Id { get; set; }
//         public string? Title
//         {
//             get => _title;
//             set => RaiseAndSetIfChanged(ref _title, value);
//         }
//     }
// }
