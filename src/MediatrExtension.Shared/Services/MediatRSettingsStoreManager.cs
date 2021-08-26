using EnvDTE;
using MediatrExtension.Core.Models;
using Microsoft.VisualStudio.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatrExtension.Shared.Services
{
    public static class MediatRSettingsKey
    {
        public const string FileName = "file_name";
        public const string Imports = "imports";
        public const string ConstructorParemeters = "c_params";
        public const string ShouldCreateFolder = "create_folder";
        public const string ShouldCreateValidationFile = "create_validator";
        public const string ShouldCreateAutomapperFile = "create_automapper_profile";
        public const string OneFileStyle = "one_file_style";
        public const string OneClassStyle = "one_class_style";
    }

    public class MediatRSettingsStoreManager
    {
        private const string SEPARATOR = ";";
        private const string COLLECTION_KEY = "MediatR";
        private const string DEFAULT_FILE_NAME = "Class";

        private readonly WritableSettingsStore store;

        public MediatRSettingsStoreManager(WritableSettingsStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string GetDefaultFileNameByProject(Project project)
        {
            var collectionPath = GetOrCreateCollectionByProject(project);
            return store.GetString(collectionPath, MediatRSettingsKey.FileName, DEFAULT_FILE_NAME);
        }

        public IEnumerable<string> GetImportsByProject(Project project)
        {
            var collectionPath = GetOrCreateCollectionByProject(project);
            var imports = store.GetString(collectionPath, MediatRSettingsKey.Imports, string.Empty);
            
            return imports.Split(new [] { SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<TypeNameModel> GetConstructorParametersByProject(Project project)
        {
            var collectionPath = GetOrCreateCollectionByProject(project);
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
            var collectionPath = GetOrCreateCollectionByProject(project);
            return store.GetBoolean(collectionPath, key, defaultValue);
        }

        public void SetImportsByProject(Project project, IEnumerable<string> imports)
        {
            var collectionPath = GetOrCreateCollectionByProject(project);
            var storedValue = string.Join(SEPARATOR, imports);
            store.SetString(collectionPath, MediatRSettingsKey.Imports, storedValue);
        }

        public void SetConstructorParametersByProject(Project project, IEnumerable<TypeNameModel> constructorParameters)
        {
            var storedValue = string.Join(SEPARATOR, constructorParameters.Select(x => $"{x.Type} {x.Name}"));
            var collectionPath = GetOrCreateCollectionByProject(project);
            store.SetString(collectionPath, MediatRSettingsKey.ConstructorParemeters, storedValue);
        }

        public void SetBooleanByProject(Project project, string key, bool value)
        {
            var collectionPath = GetOrCreateCollectionByProject(project);
            store.SetBoolean(collectionPath, key, value);
        }

        private string GetFullProjectPath(string safeProjectName)
        {
            return $"{COLLECTION_KEY}\\{safeProjectName}";
        }

        private string GetOrCreateCollectionByProject(Project project)
        {
            if (null == project) { throw new ArgumentNullException(nameof(project)); }

            var safeProjUniqueName = project.UniqueName.Replace('\\', '_');
            var collectionPath = GetFullProjectPath(safeProjUniqueName);

            store.CreateCollection(collectionPath);

            return collectionPath;
        }

        public StoredUserSettings GetAllSettingsByProject(Project project)
        {
            return new StoredUserSettings
            {
                InputFileName = GetDefaultFileNameByProject(project),
                Imports = GetImportsByProject(project).ToList(),
                ConstructorParameters = GetConstructorParametersByProject(project).ToList(),
                ShouldCreateFolder = GetBooleanByProject(project, MediatRSettingsKey.ShouldCreateFolder, true),
                ShouldCreateValidationFile = GetBooleanByProject(project, MediatRSettingsKey.ShouldCreateValidationFile),
                ShouldCreateAutomapperFile = GetBooleanByProject(project, MediatRSettingsKey.ShouldCreateAutomapperFile),
                OneFileStyle = GetBooleanByProject(project, MediatRSettingsKey.OneFileStyle),
                OneClassStyle = GetBooleanByProject(project, MediatRSettingsKey.OneClassStyle)
            };
        }

        public void SaveUserSettigs(Project project, CreateMessage model)
        {
            SetImportsByProject(project, model.Imports);
            SetConstructorParametersByProject(project, model.ConstructorParameters);

            SetBooleanByProject(project, MediatRSettingsKey.ShouldCreateFolder, model.ShouldCreateFolder);
            SetBooleanByProject(project, MediatRSettingsKey.ShouldCreateValidationFile, model.ShouldCreateValidationFile);
            SetBooleanByProject(project, MediatRSettingsKey.ShouldCreateAutomapperFile, model.ShouldCreateAutomapperFile);
            SetBooleanByProject(project, MediatRSettingsKey.OneFileStyle, model.OneFileStyle);
            SetBooleanByProject(project, MediatRSettingsKey.OneClassStyle, model.OneClassStyle);
        }
    }
}
