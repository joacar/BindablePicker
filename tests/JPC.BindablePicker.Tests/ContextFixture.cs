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
}