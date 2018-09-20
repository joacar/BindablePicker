using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Xamarin.Forms;

namespace JPC.BindablePicker
{
    public class BindablePicker : Picker
    {
        public static readonly BindableProperty DisplayMemberPathProperty =
             BindableProperty.Create(
                 nameof(DisplayMemberPath),
                 typeof(string),
                 typeof(BindablePicker),
                 default(string));

        public Func<object, string> DisplayMemberFunc
        {
            get { return (Func<object, string>) GetValue(DisplayMemberFuncProperty); }
            set { SetValue(DisplayMemberFuncProperty, value); }
        }

        public static readonly BindableProperty DisplayMemberFuncProperty =
            BindableProperty.Create(
                nameof(DisplayMemberFunc),
                typeof(Func<object, string>),
                typeof(BindablePicker));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IList),
                typeof(BindablePicker),
                default(IList),
                propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(
                nameof(SelectedItem),
                typeof(object),
                typeof(BindablePicker),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnSelectedItemChanged);

        public BindablePicker()
        {
            SelectedIndexChanged += OnSelectedIndexChanged;
        }

        /// <summary>
        /// Set the name of the property that should be used to display the objects in <see cref="ItemsSource"/>.
        /// If left blank the <see cref="object.ToString"/> will be called
        /// </summary>
        /// <remarks>
        /// Setting a value that does not exists on the object cause exception
        /// </remarks>
        /// <para>
        /// Settings this value only affects objects that are not primitive types.
        /// </para>
        /// <exception cref="ArgumentException">Setting a value that does not exists on the object cause exception</exception>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (BindablePicker)bindable;
            picker.SetSelectedItem(newValue);
        }

        private void SetSelectedItem(object selectedItem)
        {
            var displayMember = GetDisplayMember(selectedItem);
            var index = Items.IndexOf(displayMember);
            SelectedIndex = index;
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (BindablePicker)bindable;
            if (oldValue is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= picker.CollectionChanged;
            }
            observable = newValue as INotifyCollectionChanged;
            picker.BindItems();
            if (observable != null)
            {
                observable.CollectionChanged += picker.CollectionChanged;
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    BindItems();
                    return;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e);
                    break;
                case NotifyCollectionChangedAction.Add:
                    AddItems(e);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddItems(NotifyCollectionChangedEventArgs e)
        {
            var index = e.NewStartingIndex < 0 ? Items.Count : e.NewStartingIndex;
            foreach (var newItem in e.NewItems)
            {
                Items.Insert(index++, GetDisplayMember(newItem));
            }
        }

        private void RemoveItems(NotifyCollectionChangedEventArgs e)
        {
            var index = e.OldStartingIndex < Items.Count ? e.OldStartingIndex : Items.Count;
            // TODO: How do we determine the order of which the items were removed
            foreach (var _ in e.OldItems)
            {
                Items.RemoveAt(index--);
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            // coerceSelectedIndex ensures that SelectedIndex is in range [-1,ItemsSource.Count)
            SelectedItem = SelectedIndex == -1 ? null : ItemsSource?[SelectedIndex];
        }

        private void BindItems()
        {
            Items.Clear();
            if (ItemsSource == null)
            {
                return;
            }
            foreach (var item in ItemsSource)
            {
                Items.Add(GetDisplayMember(item));
            }
        }

        /// <summary>
        /// Check if <paramref name="item"/> is a primitive type.
        /// </summary>
        /// <remarks>
        /// Delegates call to <see cref="object.GetType().GetTypeInfo().IsValueType"/>.
        /// </remarks>
        /// <returns>
        /// <c>true</c>, if <paramref name="item"/> is primitive, <c>false</c> otherwise.
        /// </returns>
        /// <param name="item">The <see cref="object"/>.</param>
        private static bool IsPrimitive(object item)
        {
            return item.GetType().GetTypeInfo().IsValueType;
        }

        /// <summary>
        /// Get the value to display through reflection on <paramref name="item"/> using property <see cref="DisplayMemberPath"/>
        /// </summary>
        /// <param name="item">Item to get value from.</param>
        /// <returns>Value of the property <see cref="DisplayMemberPath"/> if not <c>null</c>; otherwise ToString()</returns>
        /// <exception cref="ArgumentException">If no property with name <see cref="DisplayMemberPath"/> is found</exception>
        protected virtual string GetDisplayMember(object item)
        {
            if (DisplayMemberFunc != null)
            {
                return DisplayMemberFunc(item);
            }
            if (item == null)
            {
                return string.Empty;
            }
            if (IsPrimitive(item) || string.IsNullOrEmpty(DisplayMemberPath))
            {
                return item.ToString();
            }
            // Find the property by walking the display member path to find any nested properties
            var propertyPathParts = DisplayMemberPath.Split('.');
            object propertyValue = item;
            foreach (var propertyPathPart in propertyPathParts)
            {
                PropertyInfo propInfo = propertyValue.GetType().GetTypeInfo().GetDeclaredProperty(propertyPathPart);
                if (propInfo == null)
                {
                    throw new ArgumentException($"No property '{propertyPathPart}' was found on '{propertyValue.GetType().FullName}'");
                }
                propertyValue = propInfo.GetValue(propertyValue);
            }
            if (propertyValue == null)
            {
                throw new ArgumentException($"No property '{DisplayMemberPath}' was found on '{item.GetType().FullName}'");
            }
            return propertyValue as string;
        }
    }
}