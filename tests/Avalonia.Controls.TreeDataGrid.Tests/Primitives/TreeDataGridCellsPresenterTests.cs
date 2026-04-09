// using Avalonia.Controls.Models.TreeDataGrid;
// using Avalonia.Controls.Primitives;
// using Avalonia.Media;
// using Avalonia.Styling;
// using Avalonia.Threading;
// using Avalonia.VisualTree;
// using TUnit.Assertions.Enums;

// namespace Avalonia.Controls.TreeDataGridTests.Primitives;

// public class TreeDataGridCellsPresenterTests
// {
//     [Test]
//     public async Task Creates_Initial_Cells()
//     {
//         var (target, _) = CreateTarget();

//         await AssertColumnIndexes(target, 0, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Updates_Column_ActualWidth()
//     {
//         var (target, _) = CreateTarget();

//         for (var i = 0; i < target.Items!.Count; ++i)
//         {
//             var column = target.Items[i];
//             await Assert.That(column.ActualWidth).IsEqualTo(i < 10 ? 10 : double.NaN);
//         }
//     }

//     [Test]
//     public async Task Scrolls_Right_One_Cell()
//     {
//         var (target, scroll) = CreateTarget();

//         scroll.Offset = new Vector(10, 0);
//         Layout(target);

//         await AssertColumnIndexes(target, 1, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Scrolls_Right_More_Than_A_Page()
//     {
//         var (target, scroll) = CreateTarget();

//         scroll.Offset = new Vector(200, 0);
//         Layout(target);

//         await AssertColumnIndexes(target, 20, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task Scrolls_Left_More_Than_A_Page()
//     {
//         var (target, scroll) = CreateTarget();

//         scroll.Offset = new Vector(200, 0);
//         Layout(target);

//         scroll.Offset = new Vector(0, 0);
//         Layout(target);

//         await AssertColumnIndexes(target, 0, 10);
//         await AssertRecyclable(target, 0);
//     }

//     [Test]
//     public async Task DesiredSize_Takes_Min_Star_Column_Width_Into_Account()
//     {
//         var minWidth = new ColumnOptions<Model>
//         {
//             MinWidth = new GridLength(100),
//         };

//         var columns = new ColumnList<Model>
//         {
//             new LayoutTestColumn<Model>("Col0", GridLength.Star, minWidth),
//             new LayoutTestColumn<Model>("Col1", GridLength.Star, minWidth),
//         };

//         var (target, scroll) = CreateTarget(columns);

//         await Assert.That(target.DesiredSize.Width).IsEqualTo(200);
//     }

//     [Test]
//     public async Task Star_Cells_Are_Measured_With_Final_Column_Width()
//     {
//         // Issue #70
//         var columns = new ColumnList<Model>
//         {
//             new LayoutTestColumn<Model>("Col0", GridLength.Star),
//             new LayoutTestColumn<Model>("Col1", GridLength.Star),
//         };

//         var (target, _) = CreateTarget(columns);

//         for (var i = 0; i < target.RealizedElements.Count; ++i)
//         {
//             var cell = (LayoutTestCellControl)target.RealizedElements[i]!;

//             await Assert.That(cell!.MeasureConstraints).IsEquivalentTo([Size.Infinity, new Size(0, double.PositiveInfinity), Size.Infinity, new Size(50, double.PositiveInfinity)], CollectionOrdering.Matching);
//         }
//     }

//     [Test]
//     public async Task Nth_Child_Handles_Deletion_And_Addition_Correctly()
//     {
//         var (target, scroll) = CreateTarget(additionalStyles:
//             [
//                 new Style(x => x.OfType<TreeDataGridCellsPresenter>().Descendant().Is<TreeDataGridCell>().NthChild(2,0))
//                 {
//                     Setters =
//                     {
//                         new Setter(TreeDataGridRow.BackgroundProperty,new SolidColorBrush(Colors.Red)),
//                     }
//                 }
//             ]);

//         Layout(target);

//         int CountEvenRedRows(TreeDataGridCellsPresenter presenter)
//         {
//             return target.GetVisualChildren().Cast<TreeDataGridCell>().Select(x => x.Background)
//                 .Count(x => x is SolidColorBrush brush && brush.Color == Colors.Red);
//         }

//         await Assert.That(CountEvenRedRows(target)).IsEqualTo(5);
//     }

//     private static async Task AssertColumnIndexes(
//         TreeDataGridCellsPresenter? target,
//         int firstColumnIndex,
//         int columnCount)
//     {
//         Assert.NotNull(target);

//         var rowIndexes = target!.GetVisualChildren()
//             .Cast<TreeDataGridCell>()
//             .Where(x => x.IsVisible)
//             .Select(x => x.ColumnIndex)
//             .OrderBy(x => x)
//             .ToList();

//         await Assert.That(rowIndexes).IsEquivalentTo(Enumerable.Range(firstColumnIndex, columnCount), CollectionOrdering.Matching);
//     }

//     private static async Task AssertRecyclable(TreeDataGridCellsPresenter? target, int count)
//     {
//         Assert.NotNull(target);

//         var recyclableCells = target!.GetVisualChildren()
//             .Cast<TreeDataGridCell>()
//             .Where(x => !x.IsVisible)
//             .ToList();
//         await Assert.That(recyclableCells.Count).IsEqualTo(count);
//     }

//     private static (TreeDataGridCellsPresenter, ScrollViewer) CreateTarget(
//         ColumnList<Model>? columns = null,
//         List<IStyle>? additionalStyles = null)
//     {
//         if (columns is null)
//         {
//             columns = [];

//             for (var i = 0; i < 100; ++i)
//             {
//                 columns.Add(new LayoutTestColumn<Model>("Column " + i));
//             }
//         }

//         var items = new Model[1];
//         var rows = new AnonymousSortableRows<Model>(new TreeDataGridItemsSourceView<Model>(items), null);

//         var target = new TreeDataGridCellsPresenter
//         {
//             ElementFactory = new TestElementFactory(),
//             Items = columns,
//             Rows = rows,
//         };

//         // The column list's effective viewport would usually be updated by the rows presenter
//         // but in this case we don't have one, so do it manually.
//         target.EffectiveViewportChanged += (s, e) =>
//         {
//             columns.ViewportChanged(e.EffectiveViewport);
//         };

//         target.Realize(0);

//         var scrollViewer = new ScrollViewer
//         {
//             Template = TestTemplates.ScrollViewerTemplate(),
//             Content = target,
//             HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
//         };

//         var root = new TestWindow(scrollViewer);

//         if (additionalStyles != null)
//         {
//             foreach (var item in additionalStyles)
//             {
//                 root.Styles.Add(item);
//             }
//         }

//         root.UpdateLayout();
//         Dispatcher.UIThread.RunJobs();

//         return (target, scrollViewer);
//     }

//     private static void Layout(TreeDataGridCellsPresenter target)
//     {
//         target.UpdateLayout();
//     }

//     private class Model : NotifyingBase
//     {
//         public int Id { get; set; }
//         public string? Title { get; set; }
//     }
// }
