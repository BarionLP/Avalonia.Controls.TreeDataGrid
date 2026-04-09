using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls.Selection;
using Avalonia.Controls.TreeDataGridTests.Collections;
using Avalonia.Controls.Utils;

namespace Avalonia.Controls.TreeDataGridTests.Selection;

public class TreeSelectionModelBaseTests_Multiple
{
    public class SelectedIndex
    {
        [Test]
        public async Task Can_Set_SelectedIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems).IsEmpty();
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
                ++raised;
            };

            target.SelectedIndex = new IndexPath(0, 2);

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-2");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
        }

        [Test]
        public async Task Can_Set_Grandchild_SelectedIndex()
        {
            var data = CreateData(depth: 3);
            var target = CreateTarget(data);
            var raised = 0;

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems).IsEmpty();
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 0, 2));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-0-2");
                ++raised;
            };

            target.SelectedIndex = new IndexPath(0, 0, 2);

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 0, 2));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 0, 2));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-0-2");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-0-2");
        }

        [Test]
        public async Task Setting_SelectedIndex_Clears_Old_Selection()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 1);
            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes.Single()).IsEqualTo(new IndexPath(0, 1));
                // await Assert.That(e.DeselectedItems.Single()!.Caption).IsEqualTo("Node 0-1");
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
                ++raised;
            };

            target.SelectedIndex = new IndexPath(0, 2);

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-2");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
        }

        [Test]
        public async Task Can_Set_SelectedIndex_To_Empty()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 2);
            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
                // await Assert.That(e.DeselectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++raised;
            };

            target.SelectedIndex = default;

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
        }

        [Test]
        public async Task Out_Of_Range_SelectedIndex_Clears_Selection()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 2);
            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
                // await Assert.That(e.DeselectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++raised;
            };

            target.SelectedIndex = new IndexPath(5, 10, 250);

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
        }

        [Test]
        public async Task Can_Select_Unexpanded_Item()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems).IsEmpty();
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(1, 2));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("Node 1-2");
                ++raised;
            };

            target.SelectedIndex = new IndexPath(1, 2);

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1, 2));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(1, 2));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1-2");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 1-2");
        }

        [Test]
        public async Task Setting_SelectedIndex_During_CollectionChanged_Results_In_Correct_Selection()
        {
            var data = new AvaloniaList<Node>();
            var target = CreateTarget(data);
            _ = new MockBinding(target, data);

            data.Add(new Node());

            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0));
        }

        [Test]
        public async Task PropertyChanged_Is_Raised()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = new IndexPath(0, 2);

            await Assert.That(raised).IsEqualTo(1);
        }

        private class MockBinding : ICollectionChangedListener
        {
            private readonly TestTreeSelectionModel _target;

            public MockBinding(TestTreeSelectionModel target, AvaloniaList<Node> data)
            {
                _target = target;
                CollectionChangedEventManager.Instance.AddListener(data, this);
            }

            public void Changed(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e)
            {
                _target.Select(new IndexPath(0));
            }

            public void PostChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e)
            {
            }

            public void PreChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e)
            {
            }
        }
    }

    public class SelectedItem
    {
        [Test]
        public async Task PropertyChanged_Is_Raised_When_SelectedIndex_Changes()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = new IndexPath(1);

            await Assert.That(raised).IsEqualTo(1);
        }
    }

    public class SelectedIndexes
    {
        [Test]
        public async Task Can_Get_Items_Via_Indexer()
        {
            var target = CreateTarget();

            target.Select(0);
            target.Select(1);
            target.Select(new IndexPath(1, 2));
            target.Select(new IndexPath(2, 3));

            await Assert.That(target.Count).IsEqualTo(4);
            await Assert.That(target.SelectedIndexes.Count).IsEqualTo(4);
            await Assert.That(target.SelectedIndexes[0]).IsEqualTo(new IndexPath(0));
            await Assert.That(target.SelectedIndexes[1]).IsEqualTo(new IndexPath(1));
            await Assert.That(target.SelectedIndexes[2]).IsEqualTo(new IndexPath(1, 2));
            await Assert.That(target.SelectedIndexes[3]).IsEqualTo(new IndexPath(2, 3));
        }

        [Test]
        public async Task PropertyChanged_Is_Raised_When_SelectedIndex_Changes()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndexes))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = new IndexPath(1);

            await Assert.That(raised).IsEqualTo(1);
        }
    }

    public class SelectedItems
    {
        [Test]
        public async Task Can_Get_Items_Via_Indexer()
        {
            var target = CreateTarget();

            target.Select(0);
            target.Select(1);
            target.Select(new IndexPath(1, 2));
            target.Select(new IndexPath(2, 3));

            await Assert.That(target.Count).IsEqualTo(4);
            await Assert.That(target.SelectedItems.Count).IsEqualTo(4);
            await Assert.That(target.SelectedItems[0]!.Caption).IsEqualTo("Node 0");
            await Assert.That(target.SelectedItems[1]!.Caption).IsEqualTo("Node 1");
            await Assert.That(target.SelectedItems[2]!.Caption).IsEqualTo("Node 1-2");
            await Assert.That(target.SelectedItems[3]!.Caption).IsEqualTo("Node 2-3");
        }

        [Test]
        public async Task PropertyChanged_Is_Raised_When_SelectedIndex_Changes()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedItems))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = new IndexPath(1);

            await Assert.That(raised).IsEqualTo(1);
        }
    }

    public class Select
    {
        [Test]
        public async Task Select_Sets_SelectedIndex_If_Previously_Unset()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems).IsEmpty();
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
                ++raised;
            };

            target.Select(new IndexPath(0, 2));

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-2");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
        }

        [Test]
        public async Task Select_Adds_To_Selection()
        {
            var target = CreateTarget();
            var raised = 0;

            target.Select(new IndexPath(0));

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems).IsEmpty();
                // await Assert.That(e.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
                // await Assert.That(e.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
                ++raised;
            };

            target.Select(new IndexPath(0, 2));

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(2);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0), new IndexPath(0, 2)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 0", "Node 0-2"]);
        }

        [Test]
        public async Task Select_With_Invalid_Index_Does_Nothing()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 2);
            target.SelectionChanged += (s, e) => ++raised;

            target.Select(new IndexPath(5, 10, 250));

            await Assert.That(raised).IsEqualTo(0);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-2");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 0-2");
        }

        [Test]
        public async Task Selecting_Already_Selected_Item_Doesnt_Raise_SelectionChanged()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 2);
            target.SelectionChanged += (s, e) => ++raised;

            target.Select(new IndexPath(0, 2));

            await Assert.That(raised).IsEqualTo(0);
        }
    }

    public class Deselect
    {
        [Test]
        public async Task Deselect_Clears_Selected_Item()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0);
            target.Select(new IndexPath(0, 1));

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEquivalentTo([new IndexPath(0, 1)]);
                // await Assert.That(e.DeselectedItems.Select(x => x?.Caption)).IsEquivalentTo(["Node 0-1"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++raised;
            };

            target.Deselect(new IndexPath(0, 1));

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0));
            await Assert.That(target.SelectedIndexes.Single()).IsEqualTo(new IndexPath(0));
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0");
            await Assert.That(target.SelectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 0"]);
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Deselect_Updates_SelectedItem_To_First_Selected_Item()
        {
            var target = CreateTarget();

            target.Select(new IndexPath(0, 2));
            target.Select(new IndexPath(0, 3));
            target.Select(new IndexPath(0, 4));
            target.Deselect(new IndexPath(0, 2));

            await Assert.That(target.Count).IsEqualTo(2);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 3));
        }
    }

    public class Clear
    {
        [Test]
        public async Task Clear_Raises_SelectionChanged()
        {
            var target = CreateTarget();
            var raised = 0;

            target.Select(new IndexPath(0, 1));
            target.Select(new IndexPath(0, 2));

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEquivalentTo([new IndexPath(0, 1), new IndexPath(0, 2)]);
                // await Assert.That(e.DeselectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1", "Node 0-2"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++raised;
            };

            target.Clear();

            await Assert.That(raised).IsEqualTo(1);
        }
    }

    public class AnchorIndex
    {
        [Test]
        public async Task Setting_SelectedIndex_Sets_AnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = new IndexPath(0, 1);

            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Setting_SelectedIndex_To_Empty_Doesnt_Clear_AnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 1);

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = default;

            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(raised).IsEqualTo(0);
        }

        [Test]
        public async Task Select_Sets_AnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 0);

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.Select(new IndexPath(0, 1));

            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 1)); 
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Deselect_Doesnt_Clear_AnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.Select(new IndexPath(0, 0));
            target.Select(new IndexPath(0, 1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.Deselect(new IndexPath(0, 1));

            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(raised).IsEqualTo(0);
        }
    }

    public class RangeAnchorIndex
    {
        [Test]
        public async Task Setting_SelectedIndex_Sets_RangeAnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.RangeAnchorIndex))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = new IndexPath(0, 1);

            await Assert.That(target.RangeAnchorIndex).IsEqualTo(new IndexPath(0, 1)); 
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Setting_SelectedIndex_To_Empty_Doesnt_Clear_RangeAnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 1);

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.SelectedIndex = default;

            await Assert.That(target.RangeAnchorIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(raised).IsEqualTo(0);
        }

        [Test]
        public async Task Select_Doesnt_Set_RangeAnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 0);

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.Select(new IndexPath(0, 1));

            await Assert.That(target.RangeAnchorIndex).IsEqualTo(new IndexPath(0, 0));
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Deselect_Doesnt_Clear_RangeAnchorIndex()
        {
            var target = CreateTarget();
            var raised = 0;

            target.SelectedIndex = new IndexPath(0, 0);

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.AnchorIndex))
                {
                    ++raised;
                }
            };

            target.Deselect(new IndexPath(0, 0));

            await Assert.That(target.RangeAnchorIndex).IsEqualTo(new IndexPath(0, 0));
            await Assert.That(raised).IsEqualTo(0);
        }
    }

    public class SingleSelect
    {
        [Test]
        public async Task Converting_To_Single_Selection_Removes_Multiple_Selection()
        {
            var target = CreateTarget();
            var raised = 0;

            target.Select(new IndexPath(0, 1));
            target.Select(new IndexPath(0, 2));
            target.Select(new IndexPath(0, 3));

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(new[] { new IndexPath(0, 2), new IndexPath(0, 3) }, e.DeselectedIndexes);
                // await Assert.That(new[] { "Node 0-2", "Node 0-3" }, e.DeselectedItems.Select(x => x?.Caption));
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++raised;
            };

            target.SingleSelect = true;

            await Assert.That(raised).IsEqualTo(1);
            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0, 1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-1");
            await Assert.That(target.SelectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1"]);
            await Assert.That(raised).IsEqualTo(1);
        }

        [Test]
        public async Task Raises_PropertyChanged()
        {
            var target = CreateTarget();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SingleSelect))
                {
                    ++raised;
                }
            };

            target.SingleSelect = true;

            await Assert.That(raised).IsEqualTo(1);
        }
    }

    public class CollectionChanges
    {
        [Test]
        public async Task Adding_Root_Item_Before_Selected_Root_Item_Updates_Indexes()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.SelectedIndex = new IndexPath(1);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsDefault();
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(e.Delta).IsEqualTo(1);
                ++indexesChangedRaised;
            };

            data.Insert(0, new Node { Caption = "new" });

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(2));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(2)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1");
            await Assert.That(target.SelectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(2));
            await Assert.That(indexesChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Adding_Child_Item_Before_Selected_Child_Item_Updates_Indexes()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.SelectedIndex = new IndexPath(0, 1);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsEqualTo(new IndexPath(0));
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(e.Delta).IsEqualTo(1);
                ++indexesChangedRaised;
            };

            data[0].Children!.Insert(0, new Node { Caption = "new" });

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0, 2)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-1");
            await Assert.That(target.SelectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 2));
            await Assert.That(indexesChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Adding_Root_Item_Before_Selected_Child_Item_Updates_Indexes()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.SelectedIndex = new IndexPath(0, 1);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsDefault();
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(e.Delta).IsEqualTo(1);
                ++indexesChangedRaised;
            };

            data.Insert(0, new Node { Caption = "new" });

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1, 1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(1, 1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-1");
            await Assert.That(target.SelectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(1, 1));
            await Assert.That(indexesChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Adding_Root_Item_Before_Selected_Grandchild_Item_Updates_Indexes()
        {
            var data = CreateData(depth: 3);
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.SelectedIndex = new IndexPath(0, 0, 1);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsDefault();
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(e.Delta).IsEqualTo(1);
                ++indexesChangedRaised;
            };

            data.Insert(0, new Node { Caption = "new" });

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1, 0, 1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(1, 0, 1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-0-1");
            await Assert.That(target.SelectedItems.Select(x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-0-1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(1, 0, 1));
            await Assert.That(indexesChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Adding_Root_Item_After_Selected_Root_Item_Doesnt_Raise_Events()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            target.SelectedIndex = new IndexPath(1);

            target.PropertyChanged += (s, e) => ++raised;
            target.SelectionChanged += (s, e) => ++raised;
            target.IndexesChanged += (s, e) => ++raised;

            data.Insert(2, new Node { Caption = "new" });

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(1));
            await Assert.That(raised).IsEqualTo(0);
        }

        [Test]
        public async Task Removing_Root_Selected_Item_Updates_State()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.Select(new IndexPath(1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++selectionChangedRaised;
            };

            data.RemoveAt(1);

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(target.AnchorIndex).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Removing_Child_Selected_Item_Updates_State()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.Select(new IndexPath(0, 1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++selectionChangedRaised;
            };

            data[0].Children!.RemoveAt(1);

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(target.AnchorIndex).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Removing_Parent_Of_Selected_Item_Updates_State()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;

            target.Select(new IndexPath(0, 1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }
            };

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++selectionChangedRaised;
            };

            data.RemoveAt(0);

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(target.AnchorIndex).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Removing_Root_Item_Before_Selected_Root_Item_Updates_Indexes()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedraised = 0;

            target.SelectedIndex = new IndexPath(1);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(-1, e.Delta);
                ++indexesChangedraised;
            };

            data.RemoveAt(0);

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0));
            await Assert.That(indexesChangedraised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Removing_Root_Item_Before_Selected_Child_Item_Updates_Indexes()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedraised = 0;

            target.SelectedIndex = new IndexPath(1, 1);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(e.Delta).IsEqualTo(-1);
                ++indexesChangedraised;
            };

            data.RemoveAt(0);

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0, 1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1-1");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1-1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(indexesChangedraised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Removing_Child_Item_Before_Selected_Grandhild_Item_Updates_Indexes()
        {
            var data = CreateData(depth: 3);
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedraised = 0;

            target.SelectedIndex = new IndexPath(1, 1, 2);

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(-1, e.Delta);
                ++indexesChangedraised;
            };

            data[1].Children!.RemoveAt(0);

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1, 0, 2));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(1, 0, 2)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1-1-2");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1-1-2"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(1, 0, 2));
            await Assert.That(indexesChangedraised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
        }

        [Test]
        public async Task Removing_Child_Range_Updates_State()
        {
            var data = CreateData(depth: 3);
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var indexesChangedraised = 0;

            target.Select(new IndexPath(0, 1));
            target.Select(new IndexPath(0, 2));
            target.Select(new IndexPath(0, 3));

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-1"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++selectionChangedRaised;
            };

            target.IndexesChanged += (s, e) =>
            {
                // await Assert.That(e.StartIndex).IsEqualTo(0);
                // await Assert.That(-2, e.Delta);
                ++indexesChangedraised;
            };

            data[0].Children!.RemoveRange(0, 2);

            await Assert.That(target.Count).IsEqualTo(2);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0, 0));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0, 0), new IndexPath(0, 1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 0-2");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 0-2", "Node 0-3"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0, 1));
            await Assert.That(indexesChangedraised).IsEqualTo(1);
            await Assert.That(selectionChangedRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Removing_Root_Item_After_Selected_Root_Item_Doesnt_Raise_Events()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var raised = 0;

            target.SelectedIndex = new IndexPath(1);

            target.PropertyChanged += (s, e) => ++raised;
            target.SelectionChanged += (s, e) => ++raised;
            target.IndexesChanged += (s, e) => ++raised;

            data.RemoveAt(2);

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(1));
            await Assert.That(raised).IsEqualTo(0);
        }

        [Test]
        public async Task Replacing_Selected_Root_Item_Updates_State()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;
            var selectedItemRaised = 0;

            target.Select(new IndexPath(1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }

                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++selectedItemRaised;
                }
            };

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(e.DeselectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1"]);
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++selectionChangedRaised;
            };

            data[1] = new Node { Caption = "new" };

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectedItemRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Replacing_Selected_Child_Item_Updates_State()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;
            var selectedItemRaised = 0;

            target.Select(new IndexPath(1, 1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }

                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++selectedItemRaised;
                }
            };

            target.SelectionChanged += (s, e) =>
            {
                // await Assert.That(e.DeselectedIndexes).IsEmpty();
                // await Assert.That(new[] { "Node 1-1" }, e.DeselectedItems.Select(static x => x?.Caption ?? ""));
                // await Assert.That(e.SelectedIndexes).IsEmpty();
                // await Assert.That(e.SelectedItems).IsEmpty();
                ++selectionChangedRaised;
            };

            data[1].Children![1] = new Node { Caption = "new" };

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(1);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectedItemRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Resetting_Root_Items_Clears_Selection()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;
            var selectedItemRaised = 0;
            var sourceResetRaised = 0;

            target.Select(new IndexPath(1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }

                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++selectedItemRaised;
                }
            };

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;
            target.SourceReset += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsDefault();
                ++sourceResetRaised;
            };

            data.Clear();

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectedItemRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Resetting_Child_Items_Clears_Selection()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;
            var selectedItemRaised = 0;
            var sourceResetRaised = 0;

            target.Select(new IndexPath(1, 1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }

                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++selectedItemRaised;
                }
            };

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;
            target.SourceReset += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsEqualTo(new IndexPath(1));
                ++sourceResetRaised;
            };

            data[1].Children!.Clear();

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectedItemRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Resetting_Child_Items_Updates_SelectedItem_To_First_Selected_Item()
        {
            var data = CreateData();
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;
            var selectedItemRaised = 0;
            var sourceResetRaised = 0;

            target.Select(new IndexPath(1, 1));
            target.Select(new IndexPath(2, 1));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }

                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++selectedItemRaised;
                }
            };

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;
            target.SourceReset += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsEqualTo(new IndexPath(1));
                ++sourceResetRaised;
            };

            data[1].Children!.Clear();

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(2, 1));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(2, 1)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 2-1");
            await Assert.That(target.SelectedItems.Single()!.Caption).IsEqualTo("Node 2-1");
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectedItemRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Resetting_Root_Items_To_Non_Empty_Collection_Clears_Selection()
        {
            var data = new ResettingCollection<Node>(CreateData());
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var selectedIndexRaised = 0;
            var selectedItemRaised = 0;
            var sourceResetRaised = 0;

            target.Select(new IndexPath(0));

            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(target.SelectedIndex))
                {
                    ++selectedIndexRaised;
                }

                if (e.PropertyName == nameof(target.SelectedItem))
                {
                    ++selectedItemRaised;
                }
            };

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;
            target.SourceReset += (s, e) =>
            {
                // await Assert.That(e.ParentIndex).IsDefault();
                ++sourceResetRaised;
            };

            data.Reset([data[0]]);

            await Assert.That(target.Count).IsEqualTo(0);
            await Assert.That(target.SelectedIndex).IsEmpty();
            await Assert.That(target.SelectedIndexes).IsEmpty();
            Assert.Null(target.SelectedItem);
            await Assert.That(target.SelectedItems).IsEmpty();
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
            await Assert.That(selectedIndexRaised).IsEqualTo(1);
            await Assert.That(selectedItemRaised).IsEqualTo(1);
        }

        [Test]
        public async Task Doesnt_Crash_On_Removing_Last_Item_After_Resetting_To_Larger_Collection()
        {
            var data = new ResettingCollection<Node>(CreateData(depth: 4));
            var target = CreateTarget(data);

            target.Select(new IndexPath(0, 1, 2));
            data.Reset([.. data, new Node()]);
            data.RemoveAt(data.Count - 1);
        }

        [Test]
        public async Task Handles_Selection_Made_In_CollectionChanged()
        {
            // Tests the following scenario:
            //
            // - Items changes from empty to having 1 item
            // - ViewModel auto-selects item 0 in CollectionChanged
            // - SelectionModel receives CollectionChanged
            // - And so adjusts the selected item from 0 to 1, which is past the end of the items.
            //
            // There's not much we can do about this situation because the order in which
            // CollectionChanged handlers are called can't be known (the problem also exists with
            // WPF). The best we can do is not select an invalid index.
            var data = new AvaloniaList<Node>();
            var target = CreateTarget(data);

            data.CollectionChanged += (s, e) =>
            {
                target.Select(new IndexPath(0));
            };

            data.Add(new Node { Caption = "foo" });

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(0));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(0)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("foo");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["foo"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(0));
        }

        [Test]
        public async Task Clearing_Node_Selection_Unsubscribes_From_CollectionChanged()
        {
            var data = CreateData();
            var target = CreateTarget(data);

            target.Select(new IndexPath(1, 1));

            var debug = (AvaloniaListDebug<Node>)data[1].Children!;
            await Assert.That(debug.GetCollectionChangedSubscribers()).IsSingleElement();

            target.Deselect(new IndexPath(1, 1));

            Assert.Null(debug.GetCollectionChangedSubscribers());
        }

        [Test]
        public async Task Clearing_Children_Updates_State()
        {
            var data = CreateData(depth: 4);
            var target = CreateTarget(data);
            var selectionChangedRaised = 0;
            var sourceResetRaised = 0;
            var indexesChangedRaised = 0;

            target.Select(new IndexPath(0, 1));
            target.Select(new IndexPath(0, 1, 0));
            target.Select(new IndexPath(0, 1, 0, 1));
            target.Select(new IndexPath(0, 2));
            target.Select(new IndexPath(0, 3));
            target.Select(new IndexPath(1, 3));

            target.SelectionChanged += (s, e) => ++selectionChangedRaised;
            target.SourceReset += (s, e) => ++sourceResetRaised;
            target.IndexesChanged += (s, e) => ++indexesChangedRaised;

            data[0].Children!.Clear();

            await Assert.That(target.Count).IsEqualTo(1);
            await Assert.That(target.SelectedIndex).IsEqualTo(new IndexPath(1, 3));
            await Assert.That(target.SelectedIndexes).IsEquivalentTo([new IndexPath(1, 3)]);
            await Assert.That(target.SelectedItem!.Caption).IsEqualTo("Node 1-3");
            await Assert.That(target.SelectedItems.Select(static x => x?.Caption ?? "")).IsEquivalentTo(["Node 1-3"]);
            await Assert.That(target.AnchorIndex).IsEqualTo(new IndexPath(1, 3));
            await Assert.That(indexesChangedRaised).IsEqualTo(0);
            await Assert.That(selectionChangedRaised).IsEqualTo(0);
            await Assert.That(sourceResetRaised).IsEqualTo(1);
        }
    }

    private static AvaloniaListDebug<Node> CreateNodes(IndexPath parentId, int depth = 2)
    {
        var result = new AvaloniaListDebug<Node>();

        for (var i = 0; i < 5; ++i)
        {
            var id = parentId.Append(i);

            var node = new Node
            {
                Id = id,
                Caption = "Node " + string.Join("-", id.ToArray()),
                TargetDepth = depth,
            };

            result.Add(node);
        }

        return result;
    }

    private static AvaloniaListDebug<Node> CreateData(int depth = 2)
    {
        return CreateNodes(default, depth);
    }

    private static TestTreeSelectionModel CreateTarget(IList<Node>? data = null)
    {
        return new TestTreeSelectionModel(data ?? CreateData()) { SingleSelect = false };
    }

    private class Node
    {
        public IndexPath Id { get; set; }
        public string? Caption { get; set; }
        public AvaloniaList<Node>? Children { get; set; }
        public int TargetDepth { get; set; }
    }

    private class TestTreeSelectionModel : TreeSelectionModelBase<Node>
    {
        public TestTreeSelectionModel(IList<Node> data)
            : base(data)
        {
        }

        protected internal override IEnumerable<Node>? GetChildren(Node node)
        {
            if (node.Children is null && node.Id.Count < node.TargetDepth)
                node.Children = CreateNodes(node.Id, node.TargetDepth);
            return node.Children;
        }
    }
}
