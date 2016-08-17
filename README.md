# BindablePicker
Picker for Xamarin.Forms to ease programming MVVM paradigm by providing a bindable `ItemsSource` property amongs others

# Usage
Unlike `Xamarin.Forms.Picker` the `BindablePicker` provides a property `ItemsSource` to bind a collection of objects to. If the collection is of type `INotifyCollectionChanged` (ObservableCollection implements this interface) then changes to the collection will be propogated to the `Items` property.

## API
* **ItemsSourceProperty** _(IList)_ Bind a list of objects that synchronize with the visual presentation
* **DisplayMemberPath** _(string)_ The member of the object to display. Can be nested property expression
* **DisplayMemberFunc** _(Func<object, string>)_ Func to create a custom string-representation of the objects
* **SelectedItemProperty** _(object)_ The selected object (TwoWay-binding)

# Issues
* Setting the `SelectedIndex` property before the `ItemsSource` property will set `SelectedIndex` to `-1` since the `Items` property is empty cause `coerceSelectedIndex` returning `-1`
