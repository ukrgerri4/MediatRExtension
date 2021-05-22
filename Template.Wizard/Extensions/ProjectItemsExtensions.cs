using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Wizard.Extensions
{
    public static class ProjectItemsExtensions
    {
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
