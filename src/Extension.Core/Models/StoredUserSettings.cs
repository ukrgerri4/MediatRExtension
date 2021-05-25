using System.Collections.Generic;

namespace Extension.Core.Models
{
    public class StoredUserSettings
    {
        public string InputFileName { get; set; } = string.Empty;
        public List<string> Imports { get; set; } = new List<string>();
        public List<TypeNameModel> ConstructorParameters { get; set; } = new List<TypeNameModel>();
    }
}
