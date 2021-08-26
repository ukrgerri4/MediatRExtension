using EnvDTE;
using EnvDTE90;
using System;

namespace MediatrExtension.Shared.Decorators
{
    internal class DteWrapper
    {
        private DTE _dte;
        public DteWrapper(DTE dte)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
        }

        public bool HasNoSelectedItems => _dte.SelectedItems?.Count == 0;

        private SelectedItem _firstSelectedItem;
        public SelectedItem FirstSelectedItem
        {
            get
            {
                if (null == _firstSelectedItem)
                {
                    _firstSelectedItem = HasNoSelectedItems ? null : _dte.SelectedItems?.Item(1);
                }

                return _firstSelectedItem;
            }
        }


        private Project _selectedProject;
        public Project SelectedProject
        {
            get
            {
                if (null == _selectedProject)
                {
                    _selectedProject = null != FirstSelectedItem?.Project ? FirstSelectedItem.Project : FirstSelectedItem?.ProjectItem?.ContainingProject;
                }

                return _selectedProject;
            }
        }

        public Solution3 Solution => _dte.Solution as Solution3;

        private string _defaultClassTemplate;
        public string DefaultClassTemplate
        {
            get
            {
                if (null == _defaultClassTemplate)
                {
                    _defaultClassTemplate = Solution.GetProjectItemTemplate("Class", "CSharp");
                }

                return _defaultClassTemplate;
            }
        }


        private ProjectItems _selectedItemProjectItems;
        public ProjectItems SelectedItemProjectItems
        {
            get
            {
                if (null == _selectedItemProjectItems)
                {
                    _selectedItemProjectItems = FirstSelectedItem?.ProjectItem?.ProjectItems ?? SelectedProject?.ProjectItems;
                }

                return _selectedItemProjectItems;
            }
        }
    }
}
