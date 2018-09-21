using System.Collections.ObjectModel;

namespace JPC.BindablePicker.Tests
{
    internal class BindingContext
    {
        public ObservableCollection<object> Items { get; set; }

        public object SelectedItem { get; set; }
    }
}