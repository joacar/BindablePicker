using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xunit;

namespace JPC.BindablePicker.Tests
{
	internal class ContextFixture
	{
		public class NestedClass
		{
			public string Nested { get; set; }
		}

		public NestedClass Nested { get; set; }

		public string DisplayName { get; set; }

		public string ComplexName { get; set; }

		public ContextFixture(string displayName, string complexName)
		{
			DisplayName = displayName;
			ComplexName = complexName;
		}

		public ContextFixture()
		{
		}
	}

	internal class BindingContext
	{
		public ObservableCollection<object> Items { get; set; }

		public object SelectedItem { get; set; }
	}

	public class BindablePickerFixture
	{
		[Fact]
		public void ItemsSource()
		{
			var items = new ObservableCollection<object>
			{
				new {Name = "Monkey"},
				"Banana",
				"Lemon",
				0,
				new DateTime(1970, 1, 1),
			};
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				ItemsSource = items
			};
			Assert.Equal(5, picker.Items.Count);
			Assert.Equal("Monkey", picker.Items[0]);
			Assert.Equal("0", picker.Items[3]);
		}

		[Fact]
		public void ItemsSource_CollectionChanged_Append()
		{
			var items = new ObservableCollection<object>
			{
				new {Name = "Monkey"},
				"Banana",
				"Lemon"
			};
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("Monkey", picker.Items[0]);
			items.Add(new { Name = "Pineapple" });
			Assert.Equal(4, picker.Items.Count);
			Assert.Equal("Pineapple", picker.Items.Last());
		}

		[Fact]
		public void ItemsSource_CollectionChanged_Clear()
		{
			var items = new ObservableCollection<object>
			{
				new {Name = "Monkey"},
				"Banana",
				"Lemon"
			};
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			items.Clear();
			Assert.Equal(0, picker.Items.Count);
		}

		[Fact]
		public void ItemsSource_CollectionChanged_Insert()
		{
			var items = new ObservableCollection<object>
			{
				new {Name = "Monkey"},
				"Banana",
				"Lemon"
			};
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("Monkey", picker.Items[0]);
			items.Insert(1, new { Name = "Pineapple" });
			Assert.Equal(4, picker.Items.Count);
			Assert.Equal("Pineapple", picker.Items[1]);
		}

		[Fact]
		public void ItemsSource_CollectionChanged_ReAssign()
		{
			var items = new ObservableCollection<object>
			{
				new {Name = "Monkey"},
				"Banana",
				"Lemon"
			};
			var bindingContext = new { Items = items };
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				BindingContext = bindingContext
			};
			picker.SetBinding(BindablePicker.ItemsSourceProperty, "Items");
			Assert.Equal(3, picker.Items.Count);
			items = new ObservableCollection<object>
			{
				"Peach",
				"Orange"
			};
			picker.BindingContext = new { Items = items };
			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("Peach", picker.Items[0]);
		}

		[Fact]
		public void ItemsSource_CollectionChanged_Remove()
		{
			var items = new ObservableCollection<object>
			{
				new {Name = "Monkey"},
				"Banana",
				"Lemon"
			};
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("Monkey", picker.Items[0]);
			items.RemoveAt(1);
			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("Lemon", picker.Items[1]);
		}

		[Fact]
		public void ItemsSource_Strings()
		{
			var items = new ObservableCollection<string>
			{
				"Monkey",
				"Banana",
				"Lemon"
			};
			var picker = new BindablePicker
			{
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("Monkey", picker.Items[0]);
		}

		[Fact]
		public void SelectedItem_InitialNull()
		{
			var bindingContext = new BindingContext
			{
				Items = new ObservableCollection<object>
				{
					new ContextFixture("Monkey", "Monkey")
				}
			};
			var picker = new BindablePicker
			{
				BindingContext = bindingContext
			};
			picker.SetBinding(BindablePicker.ItemsSourceProperty, "Items");
			picker.SetBinding(BindablePicker.SelectedItemProperty, "SelectedItem");
			Assert.Equal(1, picker.Items.Count);
			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Equal(bindingContext.SelectedItem, picker.SelectedItem);
		}

		[Fact]
		public void SelectedItem_InitialValue()
		{
			var obj = new ContextFixture("Monkey", "Monkey");
			var bindingContext = new BindingContext
			{
				Items = new ObservableCollection<object>
				{
					obj
				},
				SelectedItem = obj
			};
			var picker = new BindablePicker
			{
				BindingContext = bindingContext,
				DisplayMemberPath = "DisplayName"
			};
			picker.SetBinding(BindablePicker.ItemsSourceProperty, "Items");
			picker.SetBinding(BindablePicker.SelectedItemProperty, "SelectedItem");
			Assert.Equal(1, picker.Items.Count);
			Assert.Equal(0, picker.SelectedIndex);
			Assert.Equal(obj, picker.SelectedItem);
		}

		[Fact]
		public void DisplayMemberFunc()
		{
			Func<object, string> customDisplayFunc = o =>
			{
				var f = (ContextFixture)o;
				return $"{f.DisplayName} ({f.ComplexName})";
			};
			var obj = new ContextFixture("Monkey", "Complex Monkey");
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Name",
				DisplayMemberFunc = customDisplayFunc,
				ItemsSource = new ObservableCollection<object>
				{
					obj
				},
				SelectedIndex = 1
			};
			Assert.Equal("Monkey (Complex Monkey)", picker.Items[0]);
		}

		[Fact]
		public void SelectedIndex_OutOfRange()
		{
			var picker = new BindablePicker
			{
				ItemsSource = new List<string>
				{
					"Monkey",
					"Banana",
					"Lemon"
				},
				SelectedIndex = 0
			};
			Assert.Equal("Monkey", picker.SelectedItem);
			picker.SelectedIndex = 42;
			Assert.Equal("Lemon", picker.SelectedItem);
			picker.SelectedIndex = -42;
			Assert.Null(picker.SelectedItem);
		}

		[Fact]
		public void DisplayMemberPath_ShouldThrowArgumentException_InvalidPath()
		{
			var obj = new ContextFixture("Monkey", "Complex Monkey");
			Func<Picker> picker = () => new BindablePicker
			{
				DisplayMemberPath = "Name",
				ItemsSource = new ObservableCollection<object>
				{
					obj
				},
				SelectedIndex = 1
			};
			Assert.Throws<ArgumentException>(() => picker());
		}

		[Fact]
		public void SelectedItem_SelectedIndex_Changed()
		{
			var obj = new ContextFixture("Monkey", "Monkey");
			var bindingContext = new BindingContext
			{
				Items = new ObservableCollection<object>
				{
					obj
				},
			};
			var picker = new BindablePicker
			{
				BindingContext = bindingContext,
				DisplayMemberPath = "DisplayName"
			};
			picker.SetBinding(BindablePicker.ItemsSourceProperty, "Items");
			picker.SetBinding(BindablePicker.SelectedItemProperty, "SelectedItem");
			Assert.Equal(1, picker.Items.Count);
			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Equal(null, picker.SelectedItem);
			picker.SelectedItem = obj;
			Assert.Equal(0, picker.SelectedIndex);
			Assert.Equal(obj, picker.SelectedItem);
			picker.SelectedIndex = -1;
			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Equal(null, picker.SelectedItem);
		}

		[Fact]
		public void DisplayMemberPath_NestedPropertyExpression()
		{
			var obj = new ContextFixture
			{
				Nested = new ContextFixture.NestedClass
				{
					Nested = "NestedProperty"
				}
			};
			var picker = new BindablePicker
			{
				DisplayMemberPath = "Nested.Nested",
				ItemsSource = new ObservableCollection<object>
				{
					obj
				},
				SelectedIndex = 0
			};
			Assert.Equal("NestedProperty", picker.Items[0]);
		}
	}
}