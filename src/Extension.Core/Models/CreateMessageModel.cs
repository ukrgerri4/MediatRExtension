using Extension.Core.Definitions;
using Extension.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extension.Core.Models
{
    public class CreateMessageModel
    {
        public string InputFileName { get; set; }
        public string FolderName { get; set; }
        public FileNameInfo MessageName { get; set; }
        public FileNameInfo MessageHandlerName { get; set; }
        public FileNameInfo ResponseViewModelName { get; set; }
        public FileNameInfo ValidationFileName { get; set; }
        public FileNameInfo AutomapperFileName { get; set; }
        
        public string SuffixValue { get; set; }


        public MessageType MessageType { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public ResponseType ResponseType { get; set; }
        
        
        public bool ShouldCreateFolder { get; set; }
        public bool ShouldCreateValidationFile { get; set; }
        public bool ShouldCreateAutomapperFile { get; set; }
        public bool OneFileStyle { get; set; }
        public bool OneClassStyle { get; set; }

        public string UsingItems { get; set; }
        public string ConstructorItems { get; set; }

        public string ItemInterface
        {
            get
            {
                var interfaceStr = MessageType == MessageType.Notification ? " : INotification" : " : IRequest";
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
                    interfaceStr = MessageType == MessageType.Notification ? " : NotificationHandler" : " : RequestHandler";
                }
                else
                {
                    interfaceStr = MessageType == MessageType.Notification ? " : INotificationHandler" : " : IRequestHandler";
                }
                
                interfaceStr = $"{interfaceStr}<{MessageName.Name}";


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

        public string ValidationInterface => $" : AbstractValidator<{MessageName.Name}>";
        public string AutoMapperInterface => " : Profile";

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
                    if (MessageType == MessageType.Notification)
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

        public string[] DefaultValidationImports => new string[] { "FluentValidation" };
        public string[] DefaultAutoMapperImports => new string[] { "AutoMapper" };

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

        public IEnumerable<TypeNameModel> ConstructorParameters
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
                        return new TypeNameModel(parameters[0], parameters[1]);
                    });
            }
        }
    }
}
