using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using Template.Wizard.Definitions;

namespace Template.Wizard
{
    public partial class WizardWindow : DialogWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string InputFileName { get; set; }

        public string[] ItemTypes { get; set; }

        public string _selectedItemType;
        public string SelectedItemType
        { 
            get
            {
                return _selectedItemType;
            }

            set
            {
                _selectedItemType = value ?? string.Empty;

                if (PostfixType == PostfixType.With)
                {
                    PostfixValue = _selectedItemType;
                    OnPropertyChanged(nameof(PostfixValue));
                }
            }
        }

        public ProcessingType ProcessingType { get; set; }
        public FolderFormatType FolderFormatType { get; set; }

        private PostfixType _postfixType;
        public PostfixType PostfixType {
            get
            {
                return _postfixType;
            }
            set
            {
                _postfixType = value;
                switch (_postfixType)
                {
                    case PostfixType.With:
                        PostfixValue = SelectedItemType;
                        break;
                    case PostfixType.Without:
                        PostfixValue = "";
                        break;
                    case PostfixType.Custom:
                        break;
                }
                OnPropertyChanged(nameof(IsCustomPostfix));
                OnPropertyChanged(nameof(PostfixValue));
            }
        }
        public bool IsCustomPostfix => PostfixType == PostfixType.Custom;
        public string PostfixValue { get; set; }

        public string UsingItems { get; set; }
        public string ConstructorItems { get; set; }

        public WizardWindow(string inputFileName = null)
        {
            InitializeComponent();
            
            InputFileName = string.IsNullOrWhiteSpace(inputFileName) ? "" : inputFileName;
            ItemTypes = new string[] {"Command", "Query", "Notification"};
            SelectedItemType = ItemTypes[0];

            ProcessingType = ProcessingType.Async;
            FolderFormatType = FolderFormatType.Create;
            PostfixValue = "";
            PostfixType = PostfixType.Custom;

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void standardMenuRadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void listEditRadioButton_Checked(object sender, RoutedEventArgs e)
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
    }
}
