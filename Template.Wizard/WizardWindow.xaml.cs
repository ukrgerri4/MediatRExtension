using Microsoft.VisualStudio.PlatformUI;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Template.Wizard.Definitions;
using Template.Wizard.Extensions;
using Template.Wizard.Models;

namespace Template.Wizard
{
    public partial class WizardWindow : DialogWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string VIEW_MODEL = "ViewModel";

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
                OnPropertyChanged(nameof(RequestName));
                OnPropertyChanged(nameof(RequestHandlerName));

                if (InputReturnValue?.Trim() == GetViewModelName(oldValue))
                {
                    InputReturnValue = DefaultViewModelName;
                }
            }
        }
        #endregion

        #region RequestType Properties

        public NameValue<RequestType>[] RequestTypes { get; set; }
        public NameValue<RequestType> _selectedRequestType;
        public NameValue<RequestType> SelectedRequestType
        {
            get
            {
                return _selectedRequestType;
            }

            set
            {
                _selectedRequestType = value;

                switch(_selectedRequestType.Value)
                {
                    case RequestType.Query:
                        // TODO: check id SelectedResponseType is not ResponseType.ExistingType
                        SelectedResponseType = ResponseTypes.First(x => x.Value == ResponseType.NewItem);
                        OnPropertyChanged(nameof(SelectedResponseType));
                        break;
                    case RequestType.Command:
                        break;
                    case RequestType.Notification:
                        SelectedResponseType = ResponseTypes.First(x => x.Value == ResponseType.None);
                        OnPropertyChanged(nameof(SelectedResponseType));
                        break;
                }

                if (SelectedPostfixType?.Value == PostfixType.Default)
                {
                    InputPostfixValue = _selectedRequestType.Name;
                    OnPropertyChanged(nameof(InputPostfixValue));
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

        #region PostfixType Properties

        public NameValue<PostfixType>[] PostfixTypes { get; set; }
        private NameValue<PostfixType> _selectedPostfixType;

        public NameValue<PostfixType> SelectedPostfixType
        {
            get => _selectedPostfixType;
            set
            {
                _selectedPostfixType = value;
                switch (_selectedPostfixType.Value)
                {
                    case PostfixType.Default:
                        InputPostfixValue = SelectedRequestType?.Name ?? string.Empty;
                        break;
                    case PostfixType.None:
                        InputPostfixValue = string.Empty;
                        break;
                    case PostfixType.Custom:
                        break;
                }
                OnPropertyChanged(nameof(IsCustomPostfix));
                OnPropertyChanged(nameof(InputPostfixValue));
            }
        }

        public bool IsCustomPostfix => SelectedPostfixType?.Value == PostfixType.Custom;

        private string _inputPostfixValue;
        public string InputPostfixValue
        {
            get => _inputPostfixValue;
            set
            {
                _inputPostfixValue = value;
                OnPropertyChanged(nameof(InputPostfixValue));
                OnPropertyChanged(nameof(FolderName));
                OnPropertyChanged(nameof(RequestName));
                OnPropertyChanged(nameof(RequestHandlerName));
                OnPropertyChanged(nameof(InputReturnValue));
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
                        break;
                }
                OnPropertyChanged(nameof(ViewModelNameVisibility));
                OnPropertyChanged(nameof(IsCustomReturnValue));
                OnPropertyChanged(nameof(InputReturnValue));
            }
        }

        public bool IsResponseTypeComboBoxEnabled => SelectedRequestType?.Value != RequestType.Notification;

        public bool IsCustomReturnValue => SelectedResponseType?.Value != ResponseType.None;

        private string _inputReturnValue;
        public string InputReturnValue
        {
            get => _inputReturnValue;
            set
            {
                _inputReturnValue = value;
                OnPropertyChanged(nameof(InputReturnValue));
            }
        }
        #endregion

        public string UsingItems { get; set; }
        public string ConstructorItems { get; set; }

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

        private bool _oneFileStyle;
        public bool OneFileStyle
        {
            get => _oneFileStyle;
            set
            {
                _oneFileStyle = value;
                OnPropertyChanged(nameof(OneFileStyle));
                OnPropertyChanged(nameof(RequestHandlerNameVisibility));
            }
        }
        public bool OneClassStyle { get; set; }

        #region Preview Properties
        public string FolderName => InputFileName;
        public string FolderVisibility => ShouldCreateFolder ? Visibility.Visible.ToString() : Visibility.Collapsed.ToString();
        public string RequestName => $"{InputFileName}{InputPostfixValue}.cs";
        public string RequestHandlerName => $"{InputFileName}{InputPostfixValue}Handler.cs";
        public string RequestHandlerNameVisibility => OneFileStyle ? Visibility.Collapsed.ToString() : Visibility.Visible.ToString();
        public string DefaultViewModelName => GetViewModelName(InputFileName);

        public string ViewModelNameVisibility => 
            SelectedResponseType?.Value == ResponseType.NewItem
                ? Visibility.Visible.ToString()
                : Visibility.Collapsed.ToString();

        #endregion

        public WizardWindow(string inputFileName = null)
        {
            InitializeComponent();

            InputReturnValue = string.Empty;
            InputFileName = string.IsNullOrWhiteSpace(inputFileName) ? string.Empty : inputFileName;

            RequestTypes = Enums.ToNameValues<RequestType>().ToArray();
            SelectedRequestType = RequestTypes.First(x => x.Value == RequestType.Command); // TODO: get from options

            InputPostfixValue = string.Empty;
            PostfixTypes = Enums.ToNameValues<PostfixType>().ToArray();
            SelectedPostfixType = PostfixTypes.First(x => x.Value == PostfixType.Default); // TODO: get from options

            ProcessingTypes = Enums.ToNameValues<ProcessingType>().ToArray();
            SelectedProcessingType = ProcessingTypes.First(x => x.Value == ProcessingType.Async); // TODO: get from options

            ResponseTypes = Enums.ToNameValues<ResponseType>().ToArray();
            SelectedResponseType = ResponseTypes.First(x => x.Value == ResponseType.None);

            ShouldCreateFolder = true;
            OneFileStyle = false;
            OneClassStyle = false;

            UsingItems = string.Empty;
            ConstructorItems = string.Empty;

            DataContext = this;
        }

        private string GetViewModelName(string input)
        {
            return $"{input}{VIEW_MODEL}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public UserInput GetUserInputModel()
        {
            return new UserInput
            {
                InputFileName = InputFileName,
                RequestType = SelectedRequestType.Name,
                ProcessingType = SelectedProcessingType.Value,
                PostfixValue = InputPostfixValue,
                ReturnValue = "",
                UsingItems = UsingItems,
                ConstructorItems = ConstructorItems,
                ShouldCreateFolder = ShouldCreateFolder
            };
        }
    }
}
