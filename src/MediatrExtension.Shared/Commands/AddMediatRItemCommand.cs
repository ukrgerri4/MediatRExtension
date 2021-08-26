using EnvDTE;
using EnvDTE80;
using MediatrExtension.Core.Definitions;
using MediatrExtension.Core.Models;
using MediatrExtension.Shared.Decorators;
using MediatrExtension.Shared.Extensions;
using MediatrExtension.Shared.Services;
using MediatrExtension.Shared.View;
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
using Task = System.Threading.Tasks.Task;

namespace MediatrExtension.Shared.Commands
{
    internal sealed class AddMediatRItemCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("49e145c8-7463-4521-92c4-60184f2e30c9");

        private const string NO_FOLDER_OR_PROJECT_SELECTED_ERROR_MESSAGE = "Please select project or folder.";


        private readonly AsyncPackage package;
        private readonly MediatRSettingsStoreManager mediatrSettingsStoreManager;
        private readonly IVsThreadedWaitDialog2 waitDialog;

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
            this.mediatrSettingsStoreManager = new MediatRSettingsStoreManager(writableSettingsStore);
            Assumes.Present(this.mediatrSettingsStoreManager);

            this.waitDialog = this.package.GetService<SVsThreadedWaitDialog, IVsThreadedWaitDialog2>();
            Assumes.Present(this.waitDialog);

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static AddMediatRItemCommand Instance { get; private set; }
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;
        

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
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async() => {
                    try
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        var dte = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
                        Assumes.Present(dte);
                        var dteWrapper = new DteWrapper(dte);

                        if (dteWrapper.HasNoSelectedItems)
                        {
                            ShowWarningMessage(NO_FOLDER_OR_PROJECT_SELECTED_ERROR_MESSAGE);
                            return;
                        }

                        CreateMessageBasedOnUserSettings(dteWrapper);
                    }
                    catch (Exception ex)
                    {
                        EnsureWaitDialogClosed();
                        ShowErrorMessage(ex.Message);
                    }
                }
            );
        }

        private void CreateMessageBasedOnUserSettings(DteWrapper dteWrapper)
        {
            var storedSettings = mediatrSettingsStoreManager.GetAllSettingsByProject(dteWrapper.SelectedProject);
            var messageSettingsWindow = new MediatrMessageSettingsWindow(storedSettings);
            var windowResult = WindowHelper.ShowModal(messageSettingsWindow);

            if (WindowResultSucceeded(windowResult))
            {
                waitDialog.StartWaitDialog(
                    "Creating MeditR message...",
                    "Please wait...",
                    null,
                    0,
                    "MeditR message creating...",
                    0,
                    false,
                    true
                );

                var mediatRMessageCreateModel = messageSettingsWindow.GetCreateMessageModel();
                CreateMediatrMessage(dteWrapper, mediatRMessageCreateModel);
                mediatrSettingsStoreManager.SaveUserSettigs(dteWrapper.SelectedProject, mediatRMessageCreateModel);

                waitDialog.EndWaitDialog();
            }
        }

        private void CreateMediatrMessage(DteWrapper dteWrapper, CreateMessage model)
        {
            var folderProjectItems = GetOrCreateFolderProjectItems(dteWrapper, model);

            var codeClass = CreateMessageRequest(folderProjectItems, model, dteWrapper.DefaultClassTemplate);

            if (model.OneFileStyle)
            {
                if (model.OneClassStyle)
                {
                    CreateMessageRequestHandlerInRequestClass(codeClass, model);
                }
                else
                {
                    CreateMessageRequestHandlerInRequestFile(codeClass, model);
                }
            }
            else
            {
                CreateMessageRequestHandlerInNewFile(folderProjectItems, model, dteWrapper.DefaultClassTemplate);
            }

            if (model.ShouldCreateViewModel)
            {
                CreateViewModel(folderProjectItems, model, dteWrapper.DefaultClassTemplate);
            }

            if (model.ShouldCreateValidationFile)
            {
                CreateRequestValidator(folderProjectItems, model, dteWrapper.DefaultClassTemplate);
            }

            if (model.ShouldCreateAutomapperFile)
            {
                CreateAutoMapperProfile(folderProjectItems, model, dteWrapper.DefaultClassTemplate);
            }

            dteWrapper.SelectedProject.Save();
        }

        private ProjectItems GetOrCreateFolderProjectItems(DteWrapper dteWrapper, CreateMessage model)
        {
            if (!model.ShouldCreateFolder)
            {
                return dteWrapper.SelectedItemProjectItems;
            }

            var folderItem = dteWrapper.SelectedItemProjectItems.AddFolder(model.InputFileName);
            return folderItem.ProjectItems;
        }

        private CodeClass CreateMessageRequest(ProjectItems projectItems, CreateMessage model, string classTemplate)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var requestProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.MessageName.FullName}");
            (requestProjectItem.FileCodeModel as FileCodeModel2).AddNotExistingImports(model.DefaultRequestImports);
            
            var codeClass = requestProjectItem.FindCodeClassByName(model.MessageName.Name);
            codeClass.Access = vsCMAccess.vsCMAccessPublic;
            codeClass.InsertInterfaceToTheEnd(model.RequestInterface);
            return codeClass;
        }

        private void CreateMessageRequestHandlerInNewFile(ProjectItems projectItems, CreateMessage model, string classTemplate)
        {
            var handlerProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.MessageHandlerName.FullName}");
            var handlerCodeClass = handlerProjectItem.FindCodeClassByName(model.MessageHandlerName.Name);
            handlerCodeClass.Access = vsCMAccess.vsCMAccessPublic;
            AdjustRequestHandler(handlerCodeClass, model);
        }

        private void CreateMessageRequestHandlerInRequestFile(CodeClass requestCodeClass, CreateMessage model)
        {
            var handlerCodeClass = requestCodeClass.Namespace.AddClass(
                Name: model.MessageHandlerName.Name, 
                Position: -1, 
                Access: vsCMAccess.vsCMAccessPublic);
            AdjustRequestHandler(handlerCodeClass, model);
        }

        private void CreateMessageRequestHandlerInRequestClass(CodeClass requestCodeClass, CreateMessage model)
        {
            var handlerCodeClass = requestCodeClass.AddClass(
                Name: model.MessageHandlerName.Name,
                Position: -1,
                Access: vsCMAccess.vsCMAccessPublic
            );
            AdjustRequestHandler(handlerCodeClass, model);
        }

        private void AdjustRequestHandler(CodeClass codeClass, CreateMessage model)
        {
            var fileCodeModel = codeClass.Namespace.Parent as FileCodeModel2;
            fileCodeModel.AddNotExistingImports(model.HandlerImports);
            codeClass.InsertInterfaceToTheEnd(model.ItemHandlerInterface);
            AddConstructorToRequestHandler(codeClass, model);
            AddHandlerFunctionToRequestHandler(codeClass, model);
        }

        private void AddHandlerFunctionToRequestHandler(CodeClass codeClass, CreateMessage model)
        {
            var handler = codeClass.AddFunction(
                Name: "Handle",
                Kind: vsCMFunction.vsCMFunctionFunction,
                Type: model.HandlerHandleReturnValueName,
                Position: -1);

            handler.AddParameter("request", model.MessageName.Name, -1);
            if (model.ProcessingType == ProcessingType.Async)
            {
                handler.AddParameter("cancellationToken", "CancellationToken", -1);
            }

            handler.StartPoint.CreateEditPoint().ReplaceText(0, model.HandlerHandleAcces, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            handler.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint().Insert(model.RequestHandlerIndent + "throw new NotImplementedException();");
        }

        private void AddConstructorToRequestHandler(CodeClass codeClass, CreateMessage model)
        {
            var constructor = codeClass.AddFunction(
                Name: codeClass.Name,
                Kind: vsCMFunction.vsCMFunctionConstructor,
                Type: vsCMTypeRef.vsCMTypeRefVoid,
                Position: 0,
                Access: vsCMAccess.vsCMAccessPublic);

            var constructorParameters = model.ConstructorParameters
                .Select(x => {
                    x.Type = x.Type.Replace("$self$", model.MessageHandlerName.Name);
                    return x;
                })
                .ToList();
            constructorParameters.ForEach(x => constructor.AddParameter(x.Name, x.Type, -1));
            constructorParameters.ForEach(x => {
                var variable = codeClass.AddVariable(x.Name, x.Type, 0);
                variable.StartPoint.CreateEditPoint().Insert(model.ServicesAcces);
            });

            var constructorEditPoit = constructor.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            constructorParameters.ForEach(x => constructorEditPoit.Insert(model.RequestHandlerIndent + "this." + x.Name + " = " + x.Name + ";" + Environment.NewLine));
        }

        private void CreateViewModel(ProjectItems projectItems, CreateMessage model, string classTemplate)
        {
            var viewModelProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.ResponseViewModelName.FullName}");
            var codeClass = viewModelProjectItem.FindCodeClassByName(model.ResponseViewModelName.Name);
            codeClass.Access = vsCMAccess.vsCMAccessPublic;
        }

        private void CreateRequestValidator(ProjectItems projectItems, CreateMessage model, string classTemplate)
        {
            var validationProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.ValidationFileName.FullName}");
            (validationProjectItem.FileCodeModel as FileCodeModel2).AddNotExistingImports(model.DefaultValidationImports);

            var codeClass = validationProjectItem.FindCodeClassByName(model.ValidationFileName.Name);
            codeClass.Access = vsCMAccess.vsCMAccessPublic;
            codeClass.InsertInterfaceToTheEnd(model.ValidationInterface);
            codeClass.AddFunction(
                Name: codeClass.Name,
                Kind: vsCMFunction.vsCMFunctionConstructor,
                Type: vsCMTypeRef.vsCMTypeRefVoid,
                Position: 0,
                Access: vsCMAccess.vsCMAccessPublic);
        }

        private void CreateAutoMapperProfile(ProjectItems projectItems, CreateMessage model, string classTemplate)
        {
            var automapperProfileProjectItem = projectItems.AddFromTemplate(classTemplate, $"{model.AutomapperFileName.FullName}");
            (automapperProfileProjectItem.FileCodeModel as FileCodeModel2).AddNotExistingImports(model.DefaultAutoMapperImports);

            var codeClass = automapperProfileProjectItem.FindCodeClassByName(model.AutomapperFileName.Name);
            codeClass.Access = vsCMAccess.vsCMAccessPublic;
            codeClass.InsertInterfaceToTheEnd(model.AutoMapperInterface);
            codeClass.AddFunction(
                Name: codeClass.Name,
                Kind: vsCMFunction.vsCMFunctionConstructor,
                Type: vsCMTypeRef.vsCMTypeRefVoid,
                Position: 0,
                Access: vsCMAccess.vsCMAccessPublic);
        }

        private void EnsureWaitDialogClosed()
        {
            waitDialog.HasCanceled(out bool dialogCanceled);
            if (!dialogCanceled)
            {
                waitDialog.EndWaitDialog();
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool WindowResultSucceeded(int showWindowResult)
        {
            // 1 - Ok clicked, 0 - Cancel clicked
            return showWindowResult == 1;
        }
    }
}
