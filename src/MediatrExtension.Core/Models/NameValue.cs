namespace MediatrExtension.Core.Models
{
    public class NameValue<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
    }
}
