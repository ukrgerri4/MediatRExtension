using MediatrExtension.Core.Definitions;
using System.Collections.Generic;

namespace MediatrExtension.Core.Models
{
    public class StoredUserSettings
    {
        public string InputFileName { get; set; } = string.Empty;
        public List<string> Imports { get; set; } = new List<string>();
        public List<TypeNameModel> ConstructorParameters { get; set; } = new List<TypeNameModel>();

        // adjustments
        public bool ShouldCreateFolder { get; set; }
        public bool ShouldCreateValidationFile { get; set; }
        public bool ShouldCreateAutomapperFile { get; set; }
        public bool OneFileStyle { get; set; }
        public bool OneClassStyle { get; set; }

        // suffixes
        //public string QuerySuffix { get; set; }
        //public string CommandSuffix { get; set; }
        //public string NotificationSuffix { get; set; }

    }
}
