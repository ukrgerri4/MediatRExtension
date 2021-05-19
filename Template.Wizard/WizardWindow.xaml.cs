using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

namespace Template.Wizard
{
    public partial class WizardWindow : DialogWindow
    {
        public WizardWindow()
        {
            InitializeComponent();
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
    }
}
