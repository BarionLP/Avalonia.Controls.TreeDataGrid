using System.Globalization;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data;
using TUnit.Assertions.Enums;

namespace Avalonia.Controls.TreeDataGridTests.Models;

public class TextCellTests
{
    [Test]
    public async Task Value_Is_Initially_Read_From_String()
    {
        var binding = new TestSubject<BindingValue<string>>("initial");
        var target = new TextCell<string>(binding, binding, true);

        await Assert.That(target.Text).IsEqualTo("initial");
        await Assert.That(target.Value).IsEqualTo("initial");
    }

    [Test]
    public async Task Modified_Value_Is_Written_To_Binding()
    {
        var binding = new TestSubject<BindingValue<string>>("initial");
        var target = new TextCell<string>(binding, binding, false);
        var result = new List<string>();

        binding.Subscribe(x => result.Add(x.Value));
        target.Value = "new";

        await Assert.That(result).IsEquivalentTo(new[] { "initial", "new" }, CollectionOrdering.Matching);
    }

    [Test]
    public async Task Modified_Text_Is_Written_To_Binding()
    {
        var binding = new TestSubject<BindingValue<string>>("initial");
        var target = new TextCell<string>(binding, binding, false);
        var result = new List<string>();

        binding.Subscribe(x => result.Add(x.Value));
        target.Text = "new";

        await Assert.That(result).IsEquivalentTo(new[] { "initial", "new" }, CollectionOrdering.Matching);
    }

    [Test]
    public async Task Modified_Value_Is_Written_To_Binding_On_EndEdit()
    {
        var binding = new TestSubject<BindingValue<string>>("initial");
        var target = new TextCell<string>(binding, binding, false);
        var result = new List<string>();

        binding.Subscribe(x => result.Add(x.Value));

        target.BeginEdit();
        target.Text = "new";

        await Assert.That(target.Text).IsEqualTo("new");
        await Assert.That(target.Value).IsEqualTo("initial");
        await Assert.That(result).IsEquivalentTo(new[] { "initial" }, CollectionOrdering.Matching);

        target.EndEdit();

        await Assert.That(target.Text).IsEqualTo("new");
        await Assert.That(target.Value).IsEqualTo("new");
        await Assert.That(result).IsEquivalentTo(new[] { "initial", "new" }, CollectionOrdering.Matching);
    }

    [Test]
    public async Task Modified_Value_Is_Not_Written_To_Binding_On_CancelEdit()
    {
        var binding = new TestSubject<BindingValue<string>>("initial");
        var target = new TextCell<string>(binding, binding, false);
        var result = new List<string>();

        binding.Subscribe(x => result.Add(x.Value));

        target.BeginEdit();
        target.Text = "new";

        await Assert.That(target.Text).IsEqualTo("new");
        await Assert.That(target.Value).IsEqualTo("initial");
        await Assert.That(result).IsEquivalentTo(new[] { "initial" }, CollectionOrdering.Matching);

        target.CancelEdit();

        await Assert.That(target.Text).IsEqualTo("initial");
        await Assert.That(target.Value).IsEqualTo("initial");
        await Assert.That(result).IsEquivalentTo(new[] { "initial" }, CollectionOrdering.Matching);
    }

    public class StringFormat
    {
        [Test]
        public async Task Initial_Int_Value_Is_Formatted()
        {
            var binding = new TestSubject<BindingValue<int>>(42);
            var target = new TextCell<int>(binding, binding, true, GetOptions());

            await Assert.That(target.Text).IsEqualTo("42.00");
            await Assert.That(target.Value).IsEqualTo(42);
        }

        [Test]
        public async Task Int_Value_Is_Formatted_After_Editing()
        {
            var binding = new TestSubject<BindingValue<int>>(42);
            var target = new TextCell<int>(binding, binding, false, GetOptions());
            var result = new List<int>();

            binding.Subscribe(x => result.Add(x.Value));

            target.BeginEdit();
            target.Text = "43";

            await Assert.That(target.Text).IsEqualTo("43");
            await Assert.That(target.Value).IsEqualTo(42);
            await Assert.That(result).IsEquivalentTo(new[] { 42 }, CollectionOrdering.Matching);

            target.EndEdit();

            await Assert.That(target.Text).IsEqualTo("43.00");
            await Assert.That(target.Value).IsEqualTo(43);
            await Assert.That(result).IsEquivalentTo(new[] { 42, 43 }, CollectionOrdering.Matching);
        }

        // The culture is pinned so that the expected text doesn't depend on the decimal
        // separator of the machine running the tests.
        private static ITextCellOptions? GetOptions(string format = "{0:n2}")
        {
            return new TextColumnOptions<int>
            {
                StringFormat = format,
                Culture = CultureInfo.InvariantCulture,
            };
        }
    }
}
