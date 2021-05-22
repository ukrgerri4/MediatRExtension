using Template.Wizard.Definitions;

namespace Template.Wizard.Models
{
    public class UserInput
    {
        public string InputFileName { get; set; }
        public string RequestType { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public bool ShouldCreateFolder { get; set; }
        public string PostfixValue { get; set; }
        public string ReturnValue { get; set; }

        public string UsingItems { get; set; }
        public string ConstructorItems { get; set; }
        
    }
}
