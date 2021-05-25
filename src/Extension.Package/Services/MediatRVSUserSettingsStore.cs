using EnvDTE;
using Extension.Core.Models;
using Microsoft.VisualStudio.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TemplatesPackage.Services
{
    public class MediatRVSUserSettingsStore
    {
        private const string SEPARATOR = ";";
        private const string COLLECTION_KEY = "MediatR";

        private const string FILE_NAME_KEY = "file_name";
        private const string IMPORTS_KEY = "imports";
        private const string CONSTRUCTOR_PARAMETERS_KEY = "c_params";

        private const string DEFAULT_FILE_NAME = "Class";

        private readonly WritableSettingsStore store;

        public MediatRVSUserSettingsStore(WritableSettingsStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string GetDefaultFileNameByProject(Project project)
        {
            var collectionPath = EnsureCollectionCreated(project);
            return store.GetString(collectionPath, FILE_NAME_KEY, DEFAULT_FILE_NAME);
        }

        public IEnumerable<string> GetImportsByProject(Project project)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var imports = store.GetString(collectionPath, IMPORTS_KEY, string.Empty);
            
            return imports.Split(new [] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<TypeNameModel> GetConstructorParametersByProject(Project project)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var constructorParameters = store.GetString(collectionPath, CONSTRUCTOR_PARAMETERS_KEY, string.Empty);
            
            return constructorParameters.Split(new[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        var parameters = x.Split(' ');
                        return new TypeNameModel(parameters[0], parameters[1]);
                    });
        }

        public void SetImportsByProject(Project project, IEnumerable<string> imports)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var storedValue = string.Join(SEPARATOR, imports);
            store.SetString(collectionPath, IMPORTS_KEY, storedValue);
        }

        public void SetConstructorParametersByProject(Project project, IEnumerable<TypeNameModel> constructorParameters)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var storedValue = string.Join(SEPARATOR, constructorParameters.Select(x => $"{x.Type} {x.Name}"));
            store.SetString(collectionPath, CONSTRUCTOR_PARAMETERS_KEY, storedValue);
        }

        private string GetFullProjectPath(string safeProjectName)
        {
            return $"{COLLECTION_KEY}\\{safeProjectName}";
        }

        private string EnsureCollectionCreated(Project project)
        {
            if (null == project) { throw new ArgumentNullException(nameof(project)); }

            var safeProjUniqueName = project.UniqueName.Replace('\\', '_');
            var collectionPath = GetFullProjectPath(safeProjUniqueName);

            store.CreateCollection(collectionPath);

            return collectionPath;
        }
    }
}
