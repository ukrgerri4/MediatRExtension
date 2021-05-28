using Extension.Core.Definitions;
using Extension.Core.Extensions;
using Extension.Core.Models;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Extension.View
{
    public partial class MediatrItemOptionsWindow : DialogWindow, INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string VIEW_MODEL = "ViewModel";
        public string DefaultViewModelName => GetViewModelName(InputFileName);

        #region InputFileName Properties

        private string _inputFileName;
        public string InputFileName 
        { 
            get => _inputFileName;
            set
            {
                var oldValue = _inputFileName;

                _inputFileName = value;
                OnPropertyChanged(nameof(InputFileName));
                OnPropertyChanged(nameof(FolderName));
                OnPropertyChanged(nameof(MessageName));
                OnPropertyChanged(nameof(MessageHandlerName));
                OnPropertyChanged(nameof(ValidatorFileName));
                OnPropertyChanged(nameof(IsFormValid));


                // if input return value was not changed manualy
                if (InputReturnValue?.Trim() == GetViewModelName(oldValue))
                {
                    InputReturnValue = DefaultViewModelName;
                }
            }
        }

        #endregion

        #region MessageType Properties

        public NameValue<MessageType>[] MessageTypes { get; set; }
        public NameValue<MessageType> _selectedMessageType;
        public NameValue<MessageType> SelectedMessageType
        {
            get =>  _selectedMessageType;
            set
            {
                _selectedMessageType = value;

                switch(_selectedMessageType.Value)
                {
                    case MessageType.Query:
                        if (SelectedResponseType?.Value != ResponseType.ExistingItem)
                        {
                            SelectedResponseType = ResponseTypes.First(x => x.Value == ResponseType.NewItem);
                            OnPropertyChanged(nameof(SelectedResponseType));
                        }
                        break;
                    case MessageType.Command:
                        break;
                    case MessageType.Notification:
                        SelectedResponseType = ResponseTypes.First(x => x.Value == ResponseType.None);
                        OnPropertyChanged(nameof(SelectedResponseType));
                        break;
                }

                if (SelectedSuffixType?.Value == SuffixType.Default)
                {
                    InputSuffixValue = _selectedMessageType.Name;
                }

                OnPropertyChanged(nameof(IsResponseTypeComboBoxEnabled));
            }
        }

        #endregion

        #region ProsessingType Properties

        public NameValue<ProcessingType>[] ProcessingTypes { get; set; }
        private NameValue<ProcessingType> _selectedProcessingType;
        public NameValue<ProcessingType> SelectedProcessingType
        {
            get => _selectedProcessingType;
            set => _selectedProcessingType = value;
        }

        #endregion

        #region SuffixType Properties

        public NameValue<SuffixType>[] SuffixTypes { get; set; }
        private NameValue<SuffixType> _selectedSuffixType;

        public NameValue<SuffixType> SelectedSuffixType
        {
            get => _selectedSuffixType;
            set
            {
                _selectedSuffixType = value;
                switch (_selectedSuffixType.Value)
                {
                    case SuffixType.Default:
                        InputSuffixValue = SelectedMessageType?.Name ?? string.Empty;
                        break;
                    case SuffixType.None:
                        InputSuffixValue = string.Empty;
                        break;
                    case SuffixType.Custom:
                        break;
                }
                OnPropertyChanged(nameof(IsCustomSuffix));
            }
        }

        public bool IsCustomSuffix => SelectedSuffixType?.Value == SuffixType.Custom;

        private string _inputSuffixValue;
        public string InputSuffixValue
        {
            get => _inputSuffixValue;
            set
            {
                _inputSuffixValue = value;
                OnPropertyChanged(nameof(InputSuffixValue));
                OnPropertyChanged(nameof(FolderName));
                OnPropertyChanged(nameof(MessageName));
                OnPropertyChanged(nameof(MessageHandlerName));
                OnPropertyChanged(nameof(ValidatorFileName));
                OnPropertyChanged(nameof(InputReturnValue));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        #endregion

        #region ResponseType Properties
        public NameValue<ResponseType>[] ResponseTypes { get; set; }
        private NameValue<ResponseType> _selectedResponseType;
        public NameValue<ResponseType> SelectedResponseType
        {
            get => _selectedResponseType;
            set
            {
                _selectedResponseType = value;
                switch (_selectedResponseType.Value)
                {
                    case ResponseType.None:
                        InputReturnValue = string.Empty;
                        break;
                    case ResponseType.NewItem:
                        InputReturnValue = DefaultViewModelName;
                        break;
                    case ResponseType.ExistingItem:
                        OnPropertyChanged(nameof(IsFormValid));
                        break;
                }
                OnPropertyChanged(nameof(ResponseViewModelNameVisibility));
                OnPropertyChanged(nameof(IsCustomReturnValueEnabled));
            }
        }

        public bool IsResponseTypeComboBoxEnabled => SelectedMessageType?.Value != MessageType.Notification;
        public bool IsCustomReturnValueEnabled => IsResponseTypeComboBoxEnabled && SelectedResponseType?.Value != ResponseType.None;

        private string _inputReturnValue;
        public string InputReturnValue
        {
            get => _inputReturnValue;
            set
            {
                _inputReturnValue = value;
                OnPropertyChanged(nameof(InputReturnValue));
                OnPropertyChanged(nameof(ResponseViewModelName));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }
        #endregion

        public string UsingItems { get; set; }
        public string ConstructorItems { get; set; }

        #region Additional options
        private bool _shouldCreateFolder;
        public bool ShouldCreateFolder
        {
            get => _shouldCreateFolder;
            set
            {
                _shouldCreateFolder = value;
                OnPropertyChanged(nameof(FolderVisibility));
            }
        }

        private bool _shouldCreateValidationFile;
        public bool ShouldCreateValidationFile
        {
            get => _shouldCreateValidationFile;
            set
            {
                _shouldCreateValidationFile = value;
                OnPropertyChanged(nameof(ShouldCreateValidationFile));
                OnPropertyChanged(nameof(ValidatorFileVisibility));
            }
        }

        private bool _oneFileStyle;
        public bool OneFileStyle
        {
            get => _oneFileStyle;
            set
            {
                _oneFileStyle = value;
                OnPropertyChanged(nameof(OneFileStyle));
                OnPropertyChanged(nameof(MessageHandlerNameVisibility));
            }
        }

        public bool OneClassStyle { get; set; }
        #endregion

        #region Preview Properties
        public string FolderName => InputFileName;
        public string FolderVisibility => ShouldCreateFolder ? Visibility.Visible.ToString() : Visibility.Collapsed.ToString();
        
        public string MessageName => $"{InputFileName}{InputSuffixValue}.cs";
        
        public string MessageHandlerName => $"{InputFileName}{InputSuffixValue}Handler.cs";
        public string MessageHandlerNameVisibility => OneFileStyle ? Visibility.Collapsed.ToString() : Visibility.Visible.ToString();

        public string ResponseViewModelName => $"{InputReturnValue}.cs";
        public string ResponseViewModelNameVisibility => 
            SelectedResponseType?.Value == ResponseType.NewItem
                ? Visibility.Visible.ToString()
                : Visibility.Collapsed.ToString();

        public string ValidatorFileName => $"{InputFileName}{InputSuffixValue}Validator.cs";
        public string ValidatorFileVisibility => ShouldCreateValidationFile ? Visibility.Visible.ToString() : Visibility.Collapsed.ToString();

        #endregion

        #region Validation
        public string Error => "Test";
        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case "InputFileName":
                        if (!InputFileNameValid)
                        {
                            error = "File name type can't be empty.";
                        }
                        break;
                    case "InputReturnValue":
                        if (!InputReturnValueValid)
                        {
                            error = "Response type can't be empty.";
                        }
                        break;
                }
                return error;
            }
        }

        private bool InputReturnValueValid => SelectedResponseType.Value == ResponseType.None || !string.IsNullOrWhiteSpace(InputReturnValue);
        private bool InputFileNameValid => !string.IsNullOrWhiteSpace(InputFileName);

        public bool IsFormValid => InputReturnValueValid && InputFileNameValid && !MessageName.Equals(ResponseViewModelName, StringComparison.InvariantCultureIgnoreCase);

        #endregion

        public MediatrItemOptionsWindow(StoredUserSettings settings)
        {
            InitializeComponent();

            InputReturnValue = string.Empty;
            InputFileName = string.IsNullOrWhiteSpace(settings.InputFileName) ? string.Empty : settings.InputFileName;

            MessageTypes = Enums.ToNameValues<MessageType>().ToArray();
            SelectedMessageType = MessageTypes.First(x => x.Value == MessageType.Command);

            InputSuffixValue = string.Empty;
            SuffixTypes = Enums.ToNameValues<SuffixType>().ToArray();
            SelectedSuffixType = SuffixTypes.First(x => x.Value == SuffixType.Default);

            ProcessingTypes = Enums.ToNameValues<ProcessingType>().ToArray();
            SelectedProcessingType = ProcessingTypes.First(x => x.Value == ProcessingType.Async);

            ResponseTypes = Enums.ToNameValues<ResponseType>().ToArray();
            SelectedResponseType = ResponseTypes.First(x => x.Value == ResponseType.None);

            ShouldCreateFolder = settings.ShouldCreateFolder;
            ShouldCreateValidationFile = settings.ShouldCreateValidationFile;
            OneFileStyle = settings.OneFileStyle;
            OneClassStyle = settings.OneClassStyle;

            UsingItems = string.Join(Environment.NewLine, settings.Imports);
            ConstructorItems = string.Join(Environment.NewLine, settings.ConstructorParameters.Select(x => $"{x.Type} {x.Name}"));

            DataContext = this;
        }

        private string GetViewModelName(string input)
        {
            return $"{input}{VIEW_MODEL}";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public CreateMessageModel GetUserInputModel()
        {
            return new CreateMessageModel
            {
                InputFileName = InputFileName,
                FolderName = FolderName,
                MessageName = new FileNameInfo(MessageName),
                MessageHandlerName = new FileNameInfo(MessageHandlerName),
                ResponseViewModelName = new FileNameInfo(ResponseViewModelName),
                ValidationFileName = new FileNameInfo(ValidatorFileName),
                SuffixValue = InputSuffixValue,
                
                MessageType = SelectedMessageType.Value,
                ProcessingType = SelectedProcessingType.Value,
                ResponseType = SelectedResponseType.Value,

                UsingItems = UsingItems,
                ConstructorItems = ConstructorItems,

                ShouldCreateFolder = ShouldCreateFolder,
                ShouldCreateValidationFile = ShouldCreateValidationFile,
                OneClassStyle = OneClassStyle,
                OneFileStyle = OneFileStyle
            };
        }
    }
}
