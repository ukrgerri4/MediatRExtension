using Templates.View.Definitions;

namespace Templates.View.Models
{
    public class CreateMediatrItemModel
    {
        public string InputFileName { get; set; }
        public string FolderName { get; set; }
        public FileNameInfo RequestName { get; set; }
        public FileNameInfo RequestHandlerName { get; set; }
        public FileNameInfo ResponseViewModelName { get; set; }
        public string PostfixValue { get; set; }


        public RequestType RequestType { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public ResponseType ResponseType { get; set; }
        
        
        public bool ShouldCreateFolder { get; set; }
        public bool OneFileStyle { get; set; }
        public bool OneClassStyle { get; set; }

        public string UsingItems { get; set; }
        public string ConstructorItems { get; set; }

        public string ItemInterface
        {
            get
            {
                var interfaceStr = RequestType == RequestType.Notification ? " : INotification" : " : IRequest";
                if (ResponseType != ResponseType.None)
                {
                    interfaceStr = $"{interfaceStr}<{ResponseViewModelName.Name}>";
                }
                return interfaceStr;
            }
        } 
        public string ItemHandlerInterface
        {
            get
            {
                var interfaceStr = RequestType == RequestType.Notification ? " : INotificationHandler" : " : IRequestHandler";
                interfaceStr = $"{interfaceStr}<{RequestName.Name}";


                if (ResponseType != ResponseType.None && !string.IsNullOrWhiteSpace(ResponseViewModelName.Name))
                {
                    interfaceStr = $"{interfaceStr}, {ResponseViewModelName.Name}>";
                }
                else
                {
                    interfaceStr = $"{interfaceStr}>";
                }

                return interfaceStr;
            }
        }
        
    }
}
