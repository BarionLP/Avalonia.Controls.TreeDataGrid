// using Avalonia.Collections;
// using Avalonia.Controls.Models.TreeDataGrid;
// using Avalonia.Controls.Primitives;
// using Avalonia.LogicalTree;
// using Avalonia.Media;
// using Avalonia.Styling;
// using Avalonia.Threading;
// using Avalonia.VisualTree;
// using TUnit.Assertions.Enums;

// namespace Avalonia.Controls.TreeDataGridTests.Primitives;

// public sealed class TreeDataGridRowsPresenterTests
// {
//     [Test]
//     public async Task Nth_Child_Handles_Deletion_And_Addition_Correctly()
//     {
//         var (target, scroll, items) = CreateTarget(additionalStyles:
//             [
//                 new Style(x => x.OfType<TreeDataGridRowsPresenter>().Descendant().OfType<TreeDataGridRow>().NthChild(2,0))
//                 {
//                     Setters =
//                     {
//                         new Setter(TreeDataGridRow.BackgroundProperty,new SolidColorBrush(Colors.Red)),
//                     }
//                 }
//             ]);

//         Layout(target);

//         int CountEvenRedRows(TreeDataGridRowsPresenter presenter)
//         {
//             return target.GetVisualChildren().Cast<TreeDataGridRow>().Select(static x => x.Background)
//                 .Count(static x => x is SolidColorBrush brush && brush.Color == Colors.Red);
//         }

//         await Assert.That(CountEvenRedRows(target)).IsEqualTo(5);

//         await Assert.That(items.Count).IsEqualTo(100);

//         items.RemoveAt(0);
//         items.RemoveAt(0);

//         await Assert.That(items.Count).IsEqualTo(98);

//         Layout(target);

//         await Assert.That(CountEvenRedRows(target)).IsEqualTo(5);

//         items.Add(new Model() { Id = 101, Title = "Item 101" });

//         await Assert.That(items.Count).IsEqualTo(99);

//         Layout(target); 

//         await Assert.That(CountEvenRedRows(target)).IsEqualTo(5);
//     }

//     [Test]
//     public async Task Creates_Initial_Rows()
//     {
//         var (target, scroll, _) = CreateTarget();

//         await Assert.That(scroll.Extent).IsEqualTo(new Size(100, 1000));
//         await AssertRowIndexes(target, 0, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Scrolls_Down_One_Row()
//     {
//         var (target, scroll, _) = CreateTarget();

//         scroll.Offset = new Vector(0, 10);
//         Layout(target);

//         await AssertRowIndexes(target, 1, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Scrolls_Down_More_Than_A_Page()
//     {
//         var (target, scroll, _) = CreateTarget();

//         scroll.Offset = new Vector(0, 200);
//         Layout(target);

//         await AssertRowIndexes(target, 20, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Scrolls_Up_More_Than_A_Page()
//     {
//         var (target, scroll, _) = CreateTarget();

//         scroll.Offset = new Vector(0, 200);
//         Layout(target);

//         scroll.Offset = new Vector(0, 0);
//         Layout(target);

//         await AssertRowIndexes(target, 0, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Handles_Inserted_Row()
//     {
//         var (target, _, items) = CreateTarget();

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         items.Insert(2, new Model { Id = 100, Title = "New" });

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(11);

//         var indexes = GetRealizedRowIndexes(target);

//         // Blank space inserted in realized elements and subsequent row indexes updated.
//         await Assert.That(indexes).IsEquivalentTo([0, 1, -1, 3, 4, 5, 6, 7, 8, 9, 10], CollectionOrdering.Matching);

//         var elements = target.RealizedElements.ToList();
//         Layout(target);

//         indexes = GetRealizedRowIndexes(target);

//         // After layout an element for the new row is created.
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(0, 10), CollectionOrdering.Matching);

//         // But apart from the new row and the removed last row, all existing elements should be the same.
//         elements[2] = target.RealizedElements.ElementAt(2);
//         elements.RemoveAt(elements.Count - 1);
//         await Assert.That(target.RealizedElements).IsEquivalentTo(elements, CollectionOrdering.Matching);
//     }

//     [Test]
//     public async Task Handles_Removed_Row()
//     {
//         var (target, _, items) = CreateTarget();

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         var toRecycle = target.RealizedElements.ElementAt(2);
//         items.RemoveAt(2);

//         var indexes = GetRealizedRowIndexes(target);

//         // Item removed from realized elements and subsequent row indexes updated.
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(0, 9), CollectionOrdering.Matching);

//         var elements = target.RealizedElements.ToList();
//         Layout(target);

//         indexes = GetRealizedRowIndexes(target);

//         // After layout an element for the newly visible last row is created and indexes updated.
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(0, 10), CollectionOrdering.Matching);

//         // And the removed row should now have been recycled as the last row.
//         elements.Add(toRecycle);
//         await Assert.That(target.RealizedElements).IsEquivalentTo(elements, CollectionOrdering.Matching);
//     }

//     [Test]
//     public async Task Handles_Unrealized_Rows_Being_Removed_From_End()
//     {
//         var (target, scroll, items) = CreateTarget();

//         await Assert.That(scroll.Extent).IsEqualTo(new Size(100, 1000));
//         await AssertRowIndexes(target, 0, 10);
//         await AssertRecyclable(target, 0);

//         items.RemoveRange(90, 10);

//         await AssertRowIndexes(target, 0, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Handles_Unrealized_Rows_Being_Removed_From_Start()
//     {
//         var (target, scroll, items) = CreateTarget();

//         await Assert.That(scroll.Extent).IsEqualTo(new Size(100, 1000));
//         scroll.Offset = new Vector(0, 900);
//         Layout(target);

//         await AssertRowIndexes(target, 90, 10);
//         await AssertRecyclable(target, 0);

//         items.RemoveRange(0, 10);

//         await AssertRowIndexes(target, 80, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Realized_Children_Should_Not_Be_Removed()
//     {
//         var (target, _, items) = CreateTarget();

//         await Assert.That(target!.Items!.Count).IsEqualTo(100);
//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         items.RemoveRange(7, 93);
//         Layout(target);
//         var children = target.GetVisualChildren();

//         for (var i = 0; i < children.Count(); i++)
//         {
//             await Assert.That(target.RealizedElements[i]).IsSameReferenceAs(children.ElementAt(i));
//         }
//     }

//     [Test]
//     public async Task Should_Remove_Children_On_Empty_Collection_Assignment_To_Items()
//     {
//         var (target, _, items) = CreateTarget();
//         Layout(target);
//         await Assert.That(items.Count).IsEqualTo(100);
//         items.RemoveRange(1, 99);
//         Layout(target);
//         await Assert.That(target.Items).HasSingleItem();
//         await Assert.That(target.GetVisualChildren()).HasSingleItem();

//         target.Items = new AnonymousSortableRows<Model>(TreeDataGridItemsSourceView<Model>.Empty, null);
//         Layout(target);
//         await Assert.That(target.Items).IsEmpty();

//         await Assert.That(target.GetVisualChildren()).IsEmpty();
//         await Assert.That(target.GetLogicalChildren()).IsEmpty();

//         target.Items = new AnonymousSortableRows<Model>(new TreeDataGridItemsSourceView<Model>(Enumerable.Range(0, 5)
//             .Select(x => new Model { Id = x, Title = "Item " + x, })), null);
//         Layout(target);
//         await Assert.That(target.Items.Count).IsEqualTo(5);

//         await Assert.That(target.GetVisualChildren().Count()).IsEqualTo(5);
//     }

//     [Test]
//     public async Task Handles_Removed_And_Reinserted_Row()
//     {
//         var (target, _, items) = CreateTarget();

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         var toRecycle = target.RealizedElements.ElementAt(0);
//         var item = items[0];
//         items.RemoveAt(0);

//         var indexes = GetRealizedRowIndexes(target);

//         // Item removed from realized elements and subsequent row indexes updated.
//         await Assert.That(target.RealizedElements).DoesNotContain(toRecycle);
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(0, 9), CollectionOrdering.Matching);

//         items.Insert(0, item);

//         // Row indexes updated.
//         indexes = GetRealizedRowIndexes(target);

//         await Assert.That(indexes).IsEquivalentTo([-1, 1, 2, 3, 4, 5, 6, 7, 8, 9], CollectionOrdering.Matching);

//         var elements = target.RealizedElements.ToList();
//         Layout(target);

//         indexes = GetRealizedRowIndexes(target);

//         // After layout an element for the newly visible last row is created and indexes updated.
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(0, 10), CollectionOrdering.Matching);

//         // And the removed row should now have been recycled as the first row.
//         elements[0] = toRecycle;
//         await Assert.That(target.RealizedElements).IsEquivalentTo(elements, CollectionOrdering.Matching);
//     }

//     [Test]
//     public async Task Handles_Removing_Row_Range_That_Spans_Realized_And_Unrealized_Elements()
//     {
//         var (target, scroll, items) = CreateTarget();

//         // Scroll down one item.
//         scroll.Offset = new Vector(0, 10);
//         Layout(target);

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         var toRecycle = target.RealizedElements.Skip(4).Take(6).ToList();
//         items.RemoveRange(5, 10);

//         var indexes = GetRealizedRowIndexes(target);

//         // Item removed from realized elements and subsequent row indexes updated.
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(1, 4), CollectionOrdering.Matching);

//         var elements = target.RealizedElements.ToList();
//         Layout(target);

//         indexes = GetRealizedRowIndexes(target);

//         // After layout an element for the newly visible last row is created and indexes updated.
//         await Assert.That(indexes).IsEquivalentTo(Enumerable.Range(1, 10), CollectionOrdering.Matching);

//         // And the removed row should now have been recycled as the last row.
//         elements.AddRange(toRecycle);
//         await Assert.That(target.RealizedElements).IsEquivalentTo(elements, CollectionOrdering.Matching);
//     }

//     [Test]
//     public async Task Handles_Removing_All_Rows_When_Scrolled()
//     {
//         var (target, scroll, items) = CreateTarget();

//         // Scroll down one item.
//         scroll.Offset = new Vector(0, 10);
//         Layout(target);

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         // Remove all items using RemoveRange.
//         items.RemoveRange(0, items.Count);

//         // All items removed
//         await Assert.That(target.RealizedElements).IsEmpty();
//     }

//     [Test]
//     public async Task Handles_Removing_Row_Range_That_Invalidates_Current_Viewport()
//     {
//         var (target, scroll, items) = CreateTarget();

//         // Scroll down ten items.
//         scroll.Offset = new Vector(0, 100);
//         Layout(target);

//         await Assert.That(target.RealizedElements.Count).IsEqualTo(10);

//         // Remove all but the first five items.
//         items.RemoveRange(5, 95);

//         Layout(target);

//         // The target bounds should be updated, which will cause the scrollviewer to scroll back up.
//         await Assert.That(target.Bounds.Size).IsEqualTo(new Size(100, 100));
//     }

//     [Test]
//     public async Task Handles_Removing_Focused_Row_While_Outside_Viewport()
//     {
//         var (target, scroll, items) = CreateTarget();
//         var element = target.RealizedElements.ElementAt(0)!;

//         element.Focusable = true;
//         element.Focus();

//         // Scroll down one item.
//         scroll.Offset = new Vector(0, 10);
//         Layout(target);

//         // Remove the focused element.
//         items.RemoveAt(0);

//         // Scroll back to the beginning.
//         scroll.Offset = new Vector(0, 0);
//         Layout(target);

//         // The correct element should be shown.
//         await Assert.That(target.RealizedElements.ElementAt(0)!.DataContext).IsSameReferenceAs(items[0]);
//     }

//     [Test]
//     public async Task Handles_Replacing_Focused_Row_While_Outside_Viewport()
//     {
//         var (target, scroll, items) = CreateTarget();
//         var element = target.RealizedElements.ElementAt(0)!;

//         element.Focusable = true;
//         element.Focus();

//         // Scroll down one item.
//         scroll.Offset = new Vector(0, 10);
//         Layout(target);

//         // Replace the focused element.
//         items[0] = new Model { Id = 100, Title = "New Item" };

//         // Scroll back to the beginning.
//         scroll.Offset = new Vector(0, 0);
//         Layout(target);

//         // The correct element should be shown.
//         await Assert.That(target.RealizedElements.ElementAt(0)!.DataContext).IsSameReferenceAs(items[0]);
//     }

//     [Test]
//     public async Task Handles_Moving_Focused_Row_While_Outside_Viewport()
//     {
//         var (target, scroll, items) = CreateTarget();
//         var element = target.RealizedElements.ElementAt(0)!;

//         element.Focusable = true;
//         element.Focus();

//         // Scroll down one item.
//         scroll.Offset = new Vector(0, 10);
//         Layout(target);

//         // Move the focused element.
//         items.Move(0, items.Count - 1);

//         // Scroll back to the beginning.
//         scroll.Offset = new Vector(0, 0);
//         Layout(target);

//         // The correct element should be shown.
//         await Assert.That(target.RealizedElements.ElementAt(0)!.DataContext).IsSameReferenceAs(items[0]);
//     }

//     [Test]
//     public async Task Updates_Star_Column_ActualWidth()
//     {
//         var columns = new ColumnList<Model>
//         {
//             new TextColumn<Model, int>("ID", x => x.Id, new GridLength(1, GridUnitType.Star)),
//             new TextColumn<Model, string?>("Title", x => x.Title, new GridLength(1, GridUnitType.Star))
//         };

//         var (target, _, _) = CreateTarget(columns: columns);

//         foreach (var column in columns)
//         {
//             await Assert.That(column.ActualWidth).IsEqualTo(50);
//         }
//     }

//     [Test]
//     public async Task Brings_Next_Item_Into_View()
//     {
//         var (target, scroll, _) = CreateTarget();

//         target.BringIntoView(10);
//         Layout(target);

//         await AssertRowIndexes(target, 1, 10);
//     }

//     [Test]
//     public async Task Handles_Bringing_Item_Into_View_Which_Will_Already_Be_In_View_When_Created()
//     {
//         var (target, scroll, _) = CreateTarget();

//         // Clear the items and do a layout to simulate starting from an empty state.
//         var items = target.Items;
//         target.Items = null;
//         Layout(target);

//         // Assign the items.
//         target.Items = items;

//         // Now bring the first item into view before it's created. There was an issue here where
//         // the presenter will wait for a viewport update which will never come because the item
//         // will be placed in the existing viewport.
//         target.BringIntoView(0);

//         await AssertRowIndexes(target, 0, 10);
//     }

//     [Test]
//     public async Task Brings_Partially_Visible_New_Item_Into_View()
//     {
//         // Issue #77
//         var (target, scroll, items) = CreateTarget(itemCount: 9, rootSize: new Size(100, 95));

//         await AssertRowIndexes(target, 0, 9);

//         items.Add(new Model { Id = 100, Title = "New Item" });
//         target.BringIntoView(9);
//         Layout(target);

//         await AssertRowIndexes(target, 0, 10);
//     }

//     [Test]
//     public async Task Brings_New_Item_Outside_Viewport_Into_View()
//     {
//         // Issue #6
//         var (target, scroll, items) = CreateTarget(itemCount: 15, rootSize: new Size(100, 89));

//         await AssertRowIndexes(target, 0, 9);

//         items.Add(new Model { Id = 100, Title = "New Item" });

//         await AssertRowIndexes(target, 0, 9);

//         target.BringIntoView(15);
//         Layout(target);

//         await AssertRowIndexes(target, 7, 9);
//     }

//     [Test]
//     public async Task Assigns_Row_DataContexts()
//     {
//         var (target, scroll, items) = CreateTarget();
//         var lastRow = (TreeDataGridRow)target.RealizedElements.Last()!;

//         for (var i = 0; i < 10; ++i)
//         {
//             await Assert.That(target.RealizedElements[i]!.DataContext).IsSameReferenceAs(items[i]);
//         }

//         items.RemoveRange(0, 99);
//         Layout(target);

//         await Assert.That(lastRow.RowIndex).IsEqualTo(-1);
//         Assert.Null(lastRow.DataContext);
//     }

//     private static async Task AssertRowIndexes(TreeDataGridRowsPresenter? target, int firstRowIndex, int rowCount)
//     {
//         Assert.NotNull(target);

//         var rowIndexes = target!.GetVisualChildren()
//             .Cast<TreeDataGridRow>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.RowIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(firstRowIndex, rowCount), CollectionOrdering.Matching);

//         rowIndexes = target!.RealizedElements
//             .Cast<TreeDataGridRow>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.RowIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(firstRowIndex, rowCount), CollectionOrdering.Matching);
//     }

//     private static async Task AssertRecyclable(TreeDataGridRowsPresenter? target, int count)
//     {
//         Assert.NotNull(target);

//         var recyclableRows = target!.GetLogicalChildren()
//             .Cast<TreeDataGridRow>()
//             .Where(x => !x.IsVisible)
//             .ToList();
//         await Assert.That(recyclableRows.Count).IsEqualTo(count);
//     }

//     private static List<int> GetRealizedRowIndexes(TreeDataGridRowsPresenter? target)
//     {
//         Assert.NotNull(target);

//         return target!.RealizedElements
//             .Cast<TreeDataGridRow?>()
//             .Select(x => x?.RowIndex ?? -1)
//             .ToList();
//     }

//     private static (TreeDataGridRowsPresenter, ScrollViewer, AvaloniaList<Model>) CreateTarget(
//         IColumns? columns = null, 
//         List<IStyle>? additionalStyles = null,
//         int itemCount = 100,
//         Size? rootSize = null)
//     {
//         var items = new AvaloniaList<Model>(Enumerable.Range(0, itemCount).Select(x =>
//             new Model
//             {
//                 Id = x,
//                 Title = "Item " + x,
//             }));

//         var itemsView = new TreeDataGridItemsSourceView<Model>(items);
//         var rows = new AnonymousSortableRows<Model>(itemsView, null);

//         var target = new TreeDataGridRowsPresenter
//         {
//             ElementFactory = new TreeDataGridElementFactory(),
//             Items = rows,
//             Columns = columns,
//         };

//         var scrollViewer = new ScrollViewer
//         {
//             Template = TestTemplates.ScrollViewerTemplate(),
//             Content = target,
//         };

//         var root = new TestWindow(scrollViewer, rootSize)
//         {
//             Styles =
//             {
//                 new Style(x => x.OfType<TreeDataGridRow>())
//                 {
//                     Setters =
//                     {
//                         new Setter(TreeDataGridRow.HeightProperty, 10.0),
//                     }
//                 }
//             }
//         };

//         if (additionalStyles != null)
//         {
//             foreach (var item in additionalStyles)
//             {
//                 root.Styles.Add(item);
//             }
//         }

//         root.UpdateLayout();
//         Dispatcher.UIThread.RunJobs();

//         return (target, scrollViewer, items);
//     }

//     private static void Layout(TreeDataGridRowsPresenter target)
//     {
//         target.UpdateLayout();
//     }

//     private class Model
//     {
//         public int Id { get; set; }
//         public string? Title { get; set; }
//     }
// }
