using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using Template.Wizard.Models;

namespace Template.Wizard
{
    public class Migration
    {
        public Migration(CodeType type)
        {
            CodeType = type;
            DisplayName = type.Name;
        }

        public CodeType CodeType { get; set; }
        public string DisplayName { get; set; }
    }

    public class GenericWizard : IWizard
    {
        private WizardWindow wizardPage;

        public GenericWizard()
        {
        }

        #region IWizard Methods

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            replacementsDictionary.TryGetValue("$safeitemname$", out string inputFileName);
            wizardPage = new WizardWindow(inputFileName);
            var dialogCompleted = wizardPage.ShowModal();

            if (dialogCompleted == true)
            {
                var userInput = wizardPage.GetUserInputModel();
                var dte = (_DTE)automationObject as DTE2;

                SelectedItem selectedItem = null;
                Project proj = null;
                if (dte.SelectedItems.Count > 0)
                {
                    selectedItem = dte.SelectedItems.Item(1);
                    proj = null != selectedItem.Project ? selectedItem.Project : selectedItem.ProjectItem.ContainingProject;
                }

                if (null == proj) return; // add message
                inputFileName = userInput.InputFileName;
                var selectedType = userInput.RequestType;

                var fileExtension = ".cs";
                var className = $"{inputFileName}{selectedType}";
                var viewModelName = $"{inputFileName}ViewModel";
                var handlername = $"{inputFileName}{selectedType}Handler";

                var projItems = selectedItem.ProjectItem?.ProjectItems ?? proj?.ProjectItems;
                var folderItem = projItems.AddFolder(inputFileName);
                Solution3 solution = dte.Solution as Solution3;
                var classTemplate = solution.GetProjectItemTemplate("Class", "CSharp");
                var requestProjectItem = folderItem.ProjectItems.AddFromTemplate(classTemplate, $"{className}{fileExtension}");
                var viewModelProjectItem = folderItem.ProjectItems.AddFromTemplate(classTemplate, $"{viewModelName}{fileExtension}");
                var handlerProjectItem = folderItem.ProjectItems.AddFromTemplate(classTemplate, $"{handlername}{fileExtension}");
                CodeClass classItem = null;
                foreach (var codeElement in requestProjectItem.FileCodeModel.CodeElements)
                {
                    if (codeElement is CodeNamespace codeNamespace)
                    {
                        foreach (var child in codeNamespace.Children)
                        {
                            if (child is CodeClass codeClass && codeClass.Name == className)
                            {
                                classItem = codeClass;
                                break;
                            }
                        }
                    }
                }
                if (null == classItem) { return; } // add message

                // add using
                var folderEditPoint = requestProjectItem.FileCodeModel.CodeElements.Item(1).GetStartPoint().CreateEditPoint();
                folderEditPoint.Insert($"using MediatR;{Environment.NewLine}");
                if (!string.IsNullOrWhiteSpace(userInput.UsingItems))
                {
                    var usings = string.Join(
                        Environment.NewLine,
                        userInput.UsingItems
                            .Replace($"{Environment.NewLine}", "")
                            .Split(';')
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => $"using {x.Trim(' ')};")
                    );
                    folderEditPoint.Insert(usings);
                    folderEditPoint.Insert(Environment.NewLine);
                }

                // make public
                classItem.Access = vsCMAccess.vsCMAccessPublic;

                // add interface
                var editPoint = classItem.StartPoint.CreateEditPoint();
                editPoint.EndOfLine();
                editPoint.Insert(selectedType == "Notification" ? " : INotification" : " : IRequest");
                editPoint.Insert($"<{viewModelName}>");

                requestProjectItem.Save();
                proj.Save();
            }

            // return type?

            //var imports = classProjectItem.FileCodeModel.CodeElements.OfType<CodeImport>().ToList();
            //if (imports.Any())
            //{
            //    imports.Last().GetEndPoint().CreateEditPoint().Insert("\nusing MediatR;;\nusing blblb2;");
            //}

            //{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}
            //EnvDTE.Constants.vsProjectItemKindPhysicalFolder

            /* To get all classes in project */
            //var projectItems = selectedItem.Project.ProjectItems.GetAllProjectItems();
            //var classes = GetAllClasses(projectItems);
        }

        // Always return true; this IWizard implementation throws a WizardCancelledException
        // that is handled by Visual Studio if the user cancels the wizard.
        public bool ShouldAddProjectItem(string filePath)
        {
            return false;
        }

        // The following IWizard methods are not implemented in this example.
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        #endregion

        private void PopulateReplacementDictionary(Dictionary<string, string> replacementsDictionary) {
            //// Fill in the replacement values from the UI selections on the wizard page. These values are automatically inserted
            //// into the Elements.xml file for the custom action.
            //string locationValue = (bool)wizardPage.standardMenuRadioButton.IsChecked ?
            //    CustomActionLocations.StandardMenu : CustomActionLocations.ListEdit;
            //replacementsDictionary.Add("$LocationValue$", locationValue);
            //replacementsDictionary.Add("$GroupIdValue$", (string)wizardPage.idComboBox.SelectedItem);
            //replacementsDictionary.Add("$IdValue$", Guid.NewGuid().ToString());

            //string titleText = DefaultTextBoxStrings.TitleText;
            //if (!String.IsNullOrEmpty(wizardPage.titleTextBox.Text))
            //{
            //    titleText = wizardPage.titleTextBox.Text;
            //}

            //string descriptionText = DefaultTextBoxStrings.DescriptionText;
            //if (!String.IsNullOrEmpty(wizardPage.descriptionTextBox.Text))
            //{
            //    descriptionText = wizardPage.descriptionTextBox.Text;
            //}

            //string urlText = DefaultTextBoxStrings.UrlText;
            //if (!String.IsNullOrEmpty(wizardPage.urlTextBox.Text))
            //{
            //    urlText = wizardPage.urlTextBox.Text;
            //}

            //replacementsDictionary.Add("$TitleValue$", titleText);
            //replacementsDictionary.Add("$DescriptionValue$", descriptionText);
            //replacementsDictionary.Add("$UrlValue$", urlText);
        }

        private List<Migration> GetAllClasses(IEnumerable<ProjectItem> projectItems)
        {
            var migrations = new List<Migration>();
            foreach (var projectItem in projectItems)
            {
                var fileCodeModel = projectItem.FileCodeModel;
                if (fileCodeModel == null)
                    continue;
                foreach (var codeElement in fileCodeModel.CodeElements)
                {
                    if (codeElement is CodeNamespace)
                    {
                        var ns = codeElement as CodeNamespace;
                        // run through classes
                        foreach (var member in ns.Members)
                        {
                            var codeType = member as CodeType;
                            if (codeType == null)
                                continue;

                            foreach (var baseClass in codeType.Bases)
                            {
                                var dClass = baseClass as CodeClass;
                                if (dClass == null)
                                    continue;

                                migrations.Add(new Migration(codeType));
                            }
                        }
                    }
                }
            }
            return migrations;
        }

        private void GetAllClassesFromSolution()
        {
            //var solutionService = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            //var dynamicTypeService = Package.GetGlobalService(typeof(DynamicTypeService)) as DynamicTypeService;
            //IVsHierarchy vsHierarhy;
            //var solution = solutionService.GetProjectOfUniqueName(selectedItem.Project.FullName, out vsHierarhy);
            //var disc = dynamicTypeService.GetTypeDiscoveryService(vsHierarhy);
            //var types = disc.GetTypes(typeof(object), true /*excludeGlobalTypes*/);
            //var result = new List<Type>();
            //foreach (Type type in types)
            //{
            //    if (type.IsPublic)
            //    {
            //        if (!result.Contains(type))
            //            result.Add(type);
            //    }
            //}
        }
    }
}
