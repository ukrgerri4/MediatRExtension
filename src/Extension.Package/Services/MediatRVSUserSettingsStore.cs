using EnvDTE;
using Extension.Core.Models;
using Microsoft.VisualStudio.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TemplatesPackage.Services
{
    public static class MediatRSettingsKey
    {
        public const string FileName = "file_name";
        public const string Imports = "imports";
        public const string ConstructorParemeters = "c_params";
        public const string ShouldCreateFolder = "create_folder";
        public const string ShouldCreateValidationFile = "create_validator";
        public const string OneFileStyle = "one_file_style";
        public const string OneClassStyle = "one_class_style";
    }

    public class MediatRVSUserSettingsStore
    {
        private const string SEPARATOR = ";";
        private const string COLLECTION_KEY = "MediatR";
        private const string DEFAULT_FILE_NAME = "Class";

        private readonly WritableSettingsStore store;

        public MediatRVSUserSettingsStore(WritableSettingsStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string GetDefaultFileNameByProject(Project project)
        {
            var collectionPath = EnsureCollectionCreated(project);
            return store.GetString(collectionPath, MediatRSettingsKey.FileName, DEFAULT_FILE_NAME);
        }

        public IEnumerable<string> GetImportsByProject(Project project)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var imports = store.GetString(collectionPath, MediatRSettingsKey.Imports, string.Empty);
            
            return imports.Split(new [] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<TypeNameModel> GetConstructorParametersByProject(Project project)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var constructorParameters = store.GetString(collectionPath, MediatRSettingsKey.ConstructorParemeters, string.Empty);
            
            return constructorParameters.Split(new[] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        var parameters = x.Split(' ');
                        return new TypeNameModel(parameters[0], parameters[1]);
                    });
        }

        public bool GetBooleanByProject(Project project, string key, bool defaultValue = false)
        {
            var collectionPath = EnsureCollectionCreated(project);
            return store.GetBoolean(collectionPath, key, defaultValue);
        }

        public void SetImportsByProject(Project project, IEnumerable<string> imports)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var storedValue = string.Join(SEPARATOR, imports);
            store.SetString(collectionPath, MediatRSettingsKey.Imports, storedValue);
        }

        public void SetConstructorParametersByProject(Project project, IEnumerable<TypeNameModel> constructorParameters)
        {
            var collectionPath = EnsureCollectionCreated(project);
            var storedValue = string.Join(SEPARATOR, constructorParameters.Select(x => $"{x.Type} {x.Name}"));
            store.SetString(collectionPath, MediatRSettingsKey.ConstructorParemeters, storedValue);
        }

        public void SetBooleanByProject(Project project, string key, bool value)
        {
            var collectionPath = EnsureCollectionCreated(project);
            store.SetBoolean(collectionPath, key, value);
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
