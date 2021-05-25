namespace Extension.Core.Models
{
    public class TypeNameModel
    {
        public TypeNameModel(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; set; }
        public string Name { get; set; }
    }
}
