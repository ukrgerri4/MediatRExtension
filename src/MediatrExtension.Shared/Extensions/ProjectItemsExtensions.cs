using EnvDTE;
using System.Collections.Generic;

namespace MediatrExtension.Shared.Extensions
{
    public static class ProjectItemsExtensions
    {
        public static CodeClass FindCodeClassByName(this ProjectItem projectItem, string className)
        {
            foreach (var codeElement in projectItem.FileCodeModel.CodeElements)
            {
                if (codeElement is CodeNamespace codeNamespace)
                {
                    foreach (var child in codeNamespace.Children)
                    {
                        if (child is CodeClass codeClass && codeClass.Name == className)
                        {
                            return codeClass;
                        }
                    }
                }
            }
            return null;
        }

        public static IEnumerable<ProjectItem> GetAllProjectItems(this ProjectItems projectItems)
        {
            foreach (ProjectItem item in projectItems)
            {
                yield return item;

                if (item.SubProject != null)
                {
                    foreach (ProjectItem childItem in GetAllProjectItems(item.SubProject.ProjectItems))
                        yield return childItem;
                }
                else
                {
                    foreach (ProjectItem childItem in GetAllProjectItems(item.ProjectItems))
                        yield return childItem;
                }
            }

        }
    }
}
