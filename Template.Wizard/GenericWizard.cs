using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Template.Wizard
{
    public class GenericWizard : IWizard
    {
        private WizardWindow wizardPage;

        private DTE dte;

        public GenericWizard()
        {
            dte = Package.GetGlobalService(typeof(DTE)) as DTE;
        }

        #region IWizard Methods

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            replacementsDictionary.TryGetValue("$safeitemname$", out string inputFileName);
            wizardPage = new WizardWindow(inputFileName);
            var dialogCompleted = wizardPage.ShowModal();

            var selectedItem = dte.SelectedItems.Item(1);
            var folder = selectedItem.Name;
            var t1 = wizardPage.SelectedItemType;
            var t2 = wizardPage.ProcessingType;

            var t3 = wizardPage.UsingItems;
            var t4 = wizardPage.ConstructorItems;


            if (dialogCompleted == true)
            {
                PopulateReplacementDictionary(replacementsDictionary);
            }
            else
            {
                //throw new WizardCancelledException();
            }
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
    }
}
