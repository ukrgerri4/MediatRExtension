using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using Extension.Core.Definitions;
using Extension.Core.Models;
using Extension.View;
using Microsoft;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using TemplatesPackage.Extensions;
using TemplatesPackage.Services;
using Task = System.Threading.Tasks.Task;

namespace TemplatesPackage.Commands
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

            var settingsManager = new ShellSettingsManager(this.package);
            var writableSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            settingsStore = new MediatRVSUserSettingsStore(writableSettingsStore);

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

        private readonly MediatRVSUserSettingsStore settingsStore;

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
                try
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    var dte = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
                    Assumes.Present(dte);

                    if (!(dte.SelectedItems.Count > 0))
                    {
                        MessageBox.Show("Please select project or folder.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var selectedItem = dte.SelectedItems.Item(1);
                    var project = null != selectedItem.Project ? selectedItem.Project : selectedItem.ProjectItem.ContainingProject;

                    var storedSettings = new StoredUserSettings
                    {
                        InputFileName = settingsStore.GetDefaultFileNameByProject(project),
                        Imports = settingsStore.GetImportsByProject(project).ToList(),
                        ConstructorParameters = settingsStore.GetConstructorParametersByProject(project).ToList()
                    };

                    var window = new MediatrItemOptionsWindow(storedSettings);
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
                            SaveUserSettigs(project, model);
                            await CreateMediatrItemsAsync(model);
                        }
                        finally
                        {
                            waitDialog.EndWaitDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private async Task CreateMediatrItemsAsync(CreateItemModel model)
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
            var codeClass = CreateRequest(folderProjectItems, model, classTemplate);

            if (model.OneFileStyle)
            {
                CreateRequestHandlerInExistingFile(codeClass, model);
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

        private CodeClass CreateRequest(ProjectItems projectItems, CreateItemModel model, string classTemplate)
        {
            var requestProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.RequestName.FullName}");
            var codeClass = requestProjectItem.FindCodeClassByName(model.RequestName.Name);
            
            if (null == codeClass) { throw new ArgumentNullException(nameof(codeClass)); } // add message

            // add using to file
            var fileCodeModel = requestProjectItem.FileCodeModel as FileCodeModel2;
            var imports = fileCodeModel.CodeElements.OfType<CodeImport>().Select(x => x.Namespace).ToList();
            foreach (var import in model.DefaultRequestImports)
            {
                if (!imports.Any(x => x == import))
                {
                    fileCodeModel?.AddImport(import, 0);
                }
            }

            // add public access
            codeClass.Access = vsCMAccess.vsCMAccessPublic;

            // add interface
            var editPoint = codeClass.StartPoint.CreateEditPoint();
            editPoint.EndOfLine();
            editPoint.Insert(model.ItemInterface);
            
            return codeClass;
        }

        private void CreateRequestHandlerInNewFile(ProjectItems projectItems, CreateItemModel model, string classTemplate)
        {
            var handlerProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.RequestHandlerName.FullName}");
            var codeClass = handlerProjectItem.FindCodeClassByName(model.RequestHandlerName.Name);
            AdjustRequestHandler(codeClass, model);
        }

        private void CreateRequestHandlerInExistingFile(CodeClass requestCodeClass, CreateItemModel model)
        {
            var codeClass = requestCodeClass.Namespace.AddClass(model.RequestHandlerName.Name, -1, Access: vsCMAccess.vsCMAccessPublic);
            AdjustRequestHandler(codeClass, model);
        }

        private void AdjustRequestHandler(CodeClass codeClass, CreateItemModel model)
        {
            var fileCodeModel = (codeClass.Namespace.Parent as FileCodeModel2);
            model.Imports
                .ToList()
                .ForEach(x => fileCodeModel?.AddImport(x));

            var currentImports = fileCodeModel.CodeElements.OfType<CodeImport>().Select(x => x.Namespace).ToList();
            foreach (var import in model.DefaultHandlerImports)
            {
                if (!currentImports.Any(x => x == import))
                {
                    fileCodeModel?.AddImport(import, -1);
                }
            } 

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

            var constructorParams = model.ConstructorParameters.ToList();
            constructorParams.ForEach(x => x.Type = x.Type.Replace("$self$", model.RequestHandlerName.Name));
            constructorParams.ForEach(x => constructor.AddParameter(x.Name, x.Type, -1));
            constructorParams.ForEach(x => {
                var variable = codeClass.AddVariable(x.Name, x.Type, 0);
                variable.StartPoint.CreateEditPoint().Insert(model.ServicesAcces);
            });

            var constructorEditPoit = constructor.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            constructorParams.ForEach(x => constructorEditPoit.Insert("\t\t\t" + "this." + x.Name + " = " + x.Name + ";" + Environment.NewLine));

            // add handler
            var handler = codeClass.AddFunction(
                Name: "Handle",
                Kind: vsCMFunction.vsCMFunctionFunction,
                Type: model.HandlerHandleReturnValueName,
                Position: -1);

            handler.AddParameter("request", model.RequestName.Name, -1);
            if(model.ProcessingType == ProcessingType.Async)
            {
                handler.AddParameter("cancellationToken", "CancellationToken", -1);
            }

            handler.StartPoint.CreateEditPoint().ReplaceText(0, model.HandlerHandleAcces, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            handler.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint().Insert("\t\t\tthrow new NotImplementedException();");
        }

        private void CreateViewModel(ProjectItems projectItems, CreateItemModel model, string classTemplate)
        {
            if (model.ResponseType == ResponseType.NewItem) {
                var viewModelProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.ResponseViewModelName.FullName}");
                var codeClass = viewModelProjectItem.FindCodeClassByName(model.ResponseViewModelName.Name);
                if (null == codeClass) { return; }
                codeClass.Access = vsCMAccess.vsCMAccessPublic;
            }
        }

        private void SaveUserSettigs(Project project, CreateItemModel model)
        {
            settingsStore.SetImportsByProject(project, model.Imports);
            settingsStore.SetConstructorParametersByProject(project, model.ConstructorParameters);
        }
    }
}
