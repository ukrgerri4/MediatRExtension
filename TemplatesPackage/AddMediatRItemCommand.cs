using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using Microsoft;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Template.Wizard;
using Templates.View.Definitions;
using Templates.View.Models;
using TemplatesPackage.Extensions;
using Task = System.Threading.Tasks.Task;

namespace TemplatesPackage
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddMediatRItemCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("49e145c8-7463-4521-92c4-60184f2e30c9");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMediatRItemCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AddMediatRItemCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddMediatRItemCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AddMediatRItem's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AddMediatRItemCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async() => {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var window = new WizardWindow("Class");
                var result = WindowHelper.ShowModal(window);

                if (result == 1)
                {
                    var waitDialog = await ServiceProvider.GetServiceAsync(typeof(SVsThreadedWaitDialog)) as IVsThreadedWaitDialog2;
                    Assumes.Present(waitDialog);

                    try
                    {
                        waitDialog.StartWaitDialog(
                            "Creating MeditR item...", 
                            "Please wait...", 
                            null, 
                            0,
                            "MeditR item creating...",
                            0, 
                            false, 
                            true
                        );

                        var model = window.GetUserInputModel();
                        await CreateMediatrItemsAsync(model);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "MeditR item creation error.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    finally
                    {
                        waitDialog.EndWaitDialog();
                    }
                }
            });
        }

        private async Task CreateMediatrItemsAsync(CreateMediatrItemModel model)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(dte);

            SelectedItem selectedItem = null;
            Project proj = null;
            if (dte.SelectedItems.Count > 0)
            {
                selectedItem = dte.SelectedItems.Item(1);
                proj = null != selectedItem.Project ? selectedItem.Project : selectedItem.ProjectItem.ContainingProject;
            }

            if (null == proj) return;
            var classTemplate = (dte.Solution as Solution3).GetProjectItemTemplate("Class", "CSharp");
            var folderProjectItems = GetOrCreateFolderProjectItems(selectedItem, proj, model.InputFileName, model.ShouldCreateFolder);
            var codeNamespace = CreateRequest(folderProjectItems, model, classTemplate);

            if (model.OneFileStyle)
            {
                CreateRequestHandlerInExistingFile(codeNamespace, model);
            }
            else
            {
                CreateRequestHandlerInNewFile(folderProjectItems, model, classTemplate);
            }

            CreateViewModel(folderProjectItems, model, classTemplate);

            proj.Save();
        }

        private ProjectItems GetOrCreateFolderProjectItems(SelectedItem selectedItem, Project proj, string folderName, bool shouldCreateFolder)
        {
            if (!shouldCreateFolder)
            {
                return selectedItem.ProjectItem?.ProjectItems ?? proj.ProjectItems;
            }

            var projItems = selectedItem.ProjectItem?.ProjectItems ?? proj?.ProjectItems;
            var folderItem = projItems.AddFolder(folderName);
            return folderItem.ProjectItems;
        }

        private CodeClass CreateRequest(ProjectItems projectItems, CreateMediatrItemModel model, string classTemplate)
        {
            var requestProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.RequestName.FullName}");
            var codeClass = requestProjectItem.FindCodeClassByName(model.RequestName.Name);
            
            if (null == codeClass) { throw new ArgumentNullException(nameof(codeClass)); } // add message

            // add using to file
            var fileCodeModel = requestProjectItem.FileCodeModel as FileCodeModel2;
            fileCodeModel?.AddImport("MediatR");
            
            // add public access
            codeClass.Access = vsCMAccess.vsCMAccessPublic;

            // add interface
            var editPoint = codeClass.StartPoint.CreateEditPoint();
            editPoint.EndOfLine();
            editPoint.Insert(model.ItemInterface);
            
            return codeClass;
        }

        private void CreateRequestHandlerInNewFile(ProjectItems projectItems, CreateMediatrItemModel model, string classTemplate)
        {
            var handlerProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.RequestHandlerName.FullName}");
            var codeClass = handlerProjectItem.FindCodeClassByName(model.RequestHandlerName.Name);
        }

        private void CreateRequestHandlerInExistingFile(CodeClass requestCodeClass, CreateMediatrItemModel model)
        {
            var codeClass = requestCodeClass.Namespace.AddClass(model.RequestHandlerName.Name, -1, Access: vsCMAccess.vsCMAccessPublic);
            AdjustRequestHandler(codeClass, model);
        }

        private void AdjustRequestHandler(CodeClass codeClass, CreateMediatrItemModel model)
        {
            var fileCodeModel = (codeClass.Namespace.Parent as FileCodeModel2);
            model.UsingItems?
                .Split(new string[] { $"{Environment.NewLine}" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().TrimPrefix("using ").TrimSuffix(";"))
                .ToList()
                .ForEach(x => fileCodeModel?.AddImport(x));

            fileCodeModel?.AddImport("System.Threading", -1);


            // add public access 
            codeClass.Access = vsCMAccess.vsCMAccessPublic;

            // add interface
            var editPoint = codeClass.StartPoint.CreateEditPoint();
            editPoint.EndOfLine();
            editPoint.Insert(model.ItemHandlerInterface);

            // add constructor with params
            var constructor = codeClass.AddFunction(
                Name: codeClass.Name,
                Kind: vsCMFunction.vsCMFunctionConstructor,
                Type: vsCMTypeRef.vsCMTypeRefVoid,
                Position: 0,
                Access: vsCMAccess.vsCMAccessPublic);

            var constructorParams = model.ConstructorItems?
                .Split(new string[] { $"{Environment.NewLine}" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                {
                    var parameters = Regex.Replace(x, @"\s+", " ").Trim().Split(' ');
                    return new KeyValuePair<string, string>(parameters[0], parameters[1]);
                })
                .ToList();

            constructorParams.ForEach(x => constructor.AddParameter(x.Value, x.Key, -1));
            constructorParams.ForEach(x => {
                var variable = codeClass.AddVariable(x.Value, x.Key, 0);
                variable.StartPoint.CreateEditPoint().Insert("protected readonly ");
            });

            var constructorEditPoit = constructor.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            constructorParams.ForEach(x => constructorEditPoit.Insert("\t\t\t" + "this." + x.Value + " = " + x.Value + ";" + Environment.NewLine));

            // add handler
            // TODO: sync\async handlers
            var handler = codeClass.AddFunction(
                Name: "Handle",
                Kind: vsCMFunction.vsCMFunctionFunction,
                Type: "Task<Unit>",
                Position: -1);

            handler.AddParameter("request", model.RequestName.Name, -1);
            handler.AddParameter("cancellationToken", "CancellationToken", -1);

            handler.StartPoint.CreateEditPoint().ReplaceText(0, "public async ", (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            handler.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint().Insert("\t\t\tthrow new NotImplementedException();");
        }

        private void CreateViewModel(ProjectItems projectItems, CreateMediatrItemModel model, string classTemplate)
        {
            if (model.ResponseType == ResponseType.NewItem) {
                var viewModelProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.ResponseViewModelName.FullName}");
                viewModelProjectItem.Save();
            }
        }
    }
}
