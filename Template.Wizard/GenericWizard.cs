using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE80;

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

        //private DTE dte;

        public GenericWizard()
        {
            //dte = Package.GetGlobalService(typeof(DTE)) as DTE;
        }

        #region IWizard Methods

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            var dte = (_DTE)automationObject as DTE;
            var solutionService = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            var dynamicTypeService = Package.GetGlobalService(typeof(DynamicTypeService)) as DynamicTypeService;


            wizardPage = new WizardWindow(inputFileName);
            var dialogCompleted = wizardPage.ShowModal();

            var selectedItem = dte.SelectedItems.Item(1);
            var folder = selectedItem.Name;
            var t1 = wizardPage.SelectedItemType;
            var t2 = wizardPage.ProcessingType;

            var t3 = wizardPage.UsingItems;
            var t4 = wizardPage.ConstructorItems;


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

            var i = GetProjectItemsClasses(selectedItem.Project.ProjectItems);

            //var items = GetProjectItems(selectedItem.Project.ProjectItems);
            //var migr = new List<Migration>();
            //foreach (var c in items)
            //{
            //    var eles = c.FileCodeModel;
            //    if (eles == null)
            //        continue;
            //    foreach (var ele in eles.CodeElements)
            //    {
            //        var x = ele is CodeElement;
            //        var x1 = ele is CodeClass;

            //        if (ele is EnvDTE.CodeNamespace)
            //        {
            //            var ns = ele as EnvDTE.CodeNamespace;
            //            // run through classes
            //            foreach (var property in ns.Members)
            //            {
            //                var member = property as CodeType;
            //                if (member == null)
            //                    continue;
            //                if (member.IsCodeType && member.Kind == vsCMElement.vsCMElementClass)
            //                    migr.Add(new Migration(member));

            //                //foreach (var d in member.Bases)
            //                //{
            //                //    var dClass = d as CodeClass;
            //                //    if (dClass == null)
            //                //        continue;

            //                //    var name = member.Name;
            //                //    migr.Add(new Migration(member));
            //                //}
            //            }
            //        }
            //    }
            //}
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

        public IEnumerable<ProjectItem> GetProjectItems(EnvDTE.ProjectItems projectItems)
        {
            foreach (EnvDTE.ProjectItem item in projectItems)
            {
                yield return item;

                if (item.SubProject != null)
                {
                    foreach (EnvDTE.ProjectItem childItem in GetProjectItems(item.SubProject.ProjectItems))
                        yield return childItem;
                }
                else
                {
                    foreach (EnvDTE.ProjectItem childItem in GetProjectItems(item.ProjectItems))
                        yield return childItem;
                }
            }

        }

        //public ProjectItem GetProjectsClasses(Projects projects)
        //{
        //    foreach (Project proj in projects)
        //    {
        //        foreach(ProjectItem item in proj.ProjectItems)
        //        {
        //            if (item.Name.Contains(".cs") && null != item.FileCodeModel)
        //            {

        //            }
        //            else
        //            {

        //            }
        //        }
        //    }

        //}

        public List<string> GetProjectItemsClasses(ProjectItems projectItems)
        {
            var result = new List<string>();
            foreach (ProjectItem item in projectItems)
            {
                if (item.Name.Contains(".cs") && null != item.FileCodeModel && item.FileCodeModel.Language == CodeModelLanguageConstants.vsCMLanguageCSharp)
                {
                    foreach (var codeElement in item.FileCodeModel.CodeElements)
                    {
                        if (codeElement is CodeNamespace codeNamespace)
                        {
                            foreach (var property in codeNamespace.Members)
                            {
                                if (property is CodeType member && member.IsCodeType && member.Kind == vsCMElement.vsCMElementClass)
                                {
                                    result.Add(member.Name);
                                }
                            }
                        }
                    }
                }
                else if (item.SubProject != null)
                {
                    result = GetProjectItemsClasses(item.SubProject.ProjectItems);
                }
                else
                {
                    result = GetProjectItemsClasses(item.ProjectItems);
                }
            }
            return result;
        }
    }
}
