using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                string interfaceStr = "";
                if (ProcessingType == ProcessingType.Sync)
                {
                    interfaceStr = RequestType == RequestType.Notification ? " : NotificationHandler" : " : RequestHandler";
                }
                else
                {
                    interfaceStr = RequestType == RequestType.Notification ? " : INotificationHandler" : " : IRequestHandler";
                }
                
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

        public string HandlerHandleAcces
        {
            get
            {
                return ProcessingType == ProcessingType.Sync ? "protected override " : "public async ";
            }
        }

        public string HandlerHandleReturnValueName
        {
            get
            {
                if (ProcessingType == ProcessingType.Sync)
                {
                    return ResponseType == ResponseType.None ? "void" : ResponseViewModelName.Name;
                }
                else
                {
                    if (RequestType == RequestType.Notification)
                    {
                        return "Task";
                    }

                    var rType = !string.IsNullOrWhiteSpace(ResponseViewModelName.Name) ? ResponseViewModelName.Name : "Unit";
                    return "Task" + $"<{rType}>";
                }
            }
        }

        public string ServicesAcces => "private readonly ";

        public string[] DefaultRequestImports => new string[] { "MediatR" };

        public string[] DefaultHandlerImports
        {
            get
            {
                return ProcessingType == ProcessingType.Sync
                    ? new string[] { "MediatR" }
                    : new string[]
                    {

                        "MediatR",
                        "System.Threading",
                        "System.Threading.Tasks"
                    };

            }
        }

        public IEnumerable<string> Imports
        {
            get
            {
                return UsingItems?
                    .Split(new string[] { $"{Environment.NewLine}" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().TrimPrefix("using ").TrimSuffix(";"));
            }
        }

        public IEnumerable<(string Type, string Name)> ConstructorParameters
        {
            get
            {
                return ConstructorItems?
                    .Split(new string[] { $"{Environment.NewLine}" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().TrimEnd(','))
                    .Select(x =>
                    {
                        var parameters = Regex.Replace(x, @"\s+", " ").Trim().Split(' ');
                        (string Type, string Name) parameter = (parameters[0], parameters[1]);
                        return parameter;
                    });
            }
        }
    }
}
