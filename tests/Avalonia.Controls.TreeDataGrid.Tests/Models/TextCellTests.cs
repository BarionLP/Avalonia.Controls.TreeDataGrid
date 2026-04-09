// using Avalonia.Controls.Models.TreeDataGrid;
// using Avalonia.Data;

// namespace Avalonia.Controls.TreeDataGridTests.Models;

// public class TextCellTests
// {
//     [Test]
//     public async Task Value_Is_Initially_Read_From_String()
//     {
//         var binding = new BehaviorSubject<BindingValue<string>>("initial");
//         var target = new TextCell<string>(binding, true);

//         Assert.Equal("initial", target.Text);
//         Assert.Equal("initial", target.Value);
//     }

//     [Test]
//     public async Task Modified_Value_Is_Written_To_Binding()
//     {
//         var binding = new BehaviorSubject<BindingValue<string>>("initial");
//         var target = new TextCell<string>(binding, false);
//         var result = new List<string>();

//         binding.Subscribe(x => result.Add(x.Value));
//         target.Value = "new";

//         Assert.Equal(new[] { "initial", "new" }, result);
//     }

//     [Test]
//     public async Task Modified_Text_Is_Written_To_Binding()
//     {
//         var binding = new BehaviorSubject<BindingValue<string>>("initial");
//         var target = new TextCell<string>(binding, false);
//         var result = new List<string>();

//         binding.Subscribe(x => result.Add(x.Value));
//         target.Text = "new";

//         Assert.Equal(new[] { "initial", "new" }, result);
//     }

//     [Test]
//     public async Task Modified_Value_Is_Written_To_Binding_On_EndEdit()
//     {
//         var binding = new BehaviorSubject<BindingValue<string>>("initial");
//         var target = new TextCell<string>(binding, false);
//         var result = new List<string>();

//         binding.Subscribe(x => result.Add(x.Value));

//         target.BeginEdit();
//         target.Text = "new";

//         Assert.Equal("new", target.Text);
//         Assert.Equal("initial", target.Value);
//         Assert.Equal(new[] { "initial"}, result);

//         target.EndEdit();

//         Assert.Equal("new", target.Text);
//         Assert.Equal("new", target.Value);
//         Assert.Equal(new[] { "initial", "new" }, result);
//     }

//     [Test]
//     public async Task Modified_Value_Is_Not_Written_To_Binding_On_CancelEdit()
//     {
//         var binding = new BehaviorSubject<BindingValue<string>>("initial");
//         var target = new TextCell<string>(binding, false);
//         var result = new List<string>();

//         binding.Subscribe(x => result.Add(x.Value));

//         target.BeginEdit();
//         target.Text = "new";

//         Assert.Equal("new", target.Text);
//         Assert.Equal("initial", target.Value);
//         Assert.Equal(new[] { "initial" }, result);

//         target.CancelEdit();

//         Assert.Equal("initial", target.Text);
//         Assert.Equal("initial", target.Value);
//         Assert.Equal(new[] { "initial" }, result);
//     }

//     public class StringFormat
//     {
//         [Test]
//         public async Task Initial_Int_Value_Is_Formatted()
//         {
//             var binding = new BehaviorSubject<BindingValue<int>>(42);
//             var target = new TextCell<int>(binding, true, GetOptions());

//             Assert.Equal("42.00", target.Text);
//             Assert.Equal(42, target.Value);
//         }

//         [Test]
//         public async Task Int_Value_Is_Formatted_After_Editing()
//         {
//             var binding = new BehaviorSubject<BindingValue<int>>(42);
//             var target = new TextCell<int>(binding, false, GetOptions());
//             var result = new List<int>();

//             binding.Subscribe(x => result.Add(x.Value));

//             target.BeginEdit();
//             target.Text = "43";

//             Assert.Equal("43", target.Text);
//             Assert.Equal(42, target.Value);
//             Assert.Equal(new[] { 42 }, result);

//             target.EndEdit();

//             Assert.Equal("43.00", target.Text);
//             Assert.Equal(43, target.Value);
//             Assert.Equal(new[] { 42, 43 }, result);
//         }

//         private ITextCellOptions? GetOptions(string format = "{0:n2}")
//         {
//             return new TextColumnOptions<int> { StringFormat = format };
//         }
//     }
// }
