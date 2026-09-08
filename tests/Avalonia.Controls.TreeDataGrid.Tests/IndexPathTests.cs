namespace Avalonia.Controls.TreeDataGridTests;

public class IndexPathTests
{
    // A path of one index can be stored either inline or in an array, and the two forms compare
    // equal, so they have to hash equal: IndexPath is used as a dictionary key and grouped on.
    [Test]
    [Arguments(0)]
    [Arguments(5)]
    public async Task Single_Index_Forms_Are_Equal_And_Hash_Equally(int index)
    {
        var inline = new IndexPath(index);
        var array = new IndexPath([index]);

        await Assert.That(inline == array).IsTrue();
        await Assert.That(inline.GetHashCode()).IsEqualTo(array.GetHashCode());
    }

    [Test]
    public async Task Empty_Forms_Are_Equal_And_Hash_Equally()
    {
        var empty = default(IndexPath);
        var emptyArray = new IndexPath([]);

        await Assert.That(empty == emptyArray).IsTrue();
        await Assert.That(empty.GetHashCode()).IsEqualTo(emptyArray.GetHashCode());
    }

    [Test]
    public async Task Can_Be_Used_As_A_Dictionary_Key()
    {
        var target = new Dictionary<IndexPath, string>
        {
            [default] = "root",
            [new IndexPath(0)] = "0",
            [new IndexPath(1)] = "1",
            [new IndexPath(0, 1)] = "0.1",
            [new IndexPath(1, 0)] = "1.0",
        };

        await Assert.That(target.Count).IsEqualTo(5);
        await Assert.That(target[new IndexPath([0])]).IsEqualTo("0");
        await Assert.That(target[new IndexPath([0, 1])]).IsEqualTo("0.1");
        await Assert.That(target.ContainsKey(new IndexPath(2))).IsFalse();
    }

    [Test]
    public async Task Grouping_By_Parent_Path_Groups_Siblings_Together()
    {
        IndexPath[] paths = [new(0, 0), new(0, 1), new(1, 0)];

        var groups = paths.GroupBy(x => x[..^1]).ToList();

        await Assert.That(groups.Count).IsEqualTo(2);
        await Assert.That(groups.Single(x => x.Key == new IndexPath(0)).Count()).IsEqualTo(2);
        await Assert.That(groups.Single(x => x.Key == new IndexPath(1)).Count()).IsEqualTo(1);
    }
}
