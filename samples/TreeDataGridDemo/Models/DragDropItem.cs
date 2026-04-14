using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Models;

namespace TreeDataGridDemo.Models;

public class DragDropItem(string name) : NotifyingBase
{
    private static readonly Random _random = new(0);
    private ObservableCollection<DragDropItem>? _children;
    private bool _allowDrag = true;
    private bool _allowDrop = true;

    public string Name { get; } = name;

    public bool AllowDrag
    {
        get => _allowDrag;
        set => RaiseAndSetIfChanged(ref _allowDrag, value);
    }

    public bool AllowDrop
    {
        get => _allowDrop;
        set => RaiseAndSetIfChanged(ref _allowDrop, value);
    }

    public ObservableCollection<DragDropItem> Children => _children ??= CreateRandomItems();

    public static ObservableCollection<DragDropItem> CreateRandomItems()
    {
        var names = new Bogus.DataSets.Name();
        var count = _random.Next(10);
        return new ObservableCollection<DragDropItem>(Enumerable.Range(0, count)
            .Select(x => new DragDropItem(names.FullName())));
    }
}
