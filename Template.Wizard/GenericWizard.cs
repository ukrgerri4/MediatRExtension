using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;

namespace GenericTemplateWizard
{
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
            wizardPage = new WizardWindow();
            Nullable<bool> dialogCompleted = wizardPage.ShowModal();

            if (dialogCompleted == true)
            {
                PopulateReplacementDictionary(replacementsDictionary);
            }
            else
            {
                throw new WizardCancelledException();
            }
        }

        // Always return true; this IWizard implementation throws a WizardCancelledException
        // that is handled by Visual Studio if the user cancels the wizard.
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
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

        }
    }
}
