using Prism.Services.Dialogs;
using System.Windows;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Services;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class PasswordGenerationDialogViewModel : ObservableObject, IDialogAware
{
        private readonly IPasswordGenerator _passwordGenerator;

        public string Title => (string) Application.Current.FindResource("GeneratePasswordDialogHeader");

        public event Action<IDialogResult>? RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }

        public PasswordGenerationDialogViewModel(IPasswordGenerator passwordGenerator)
        {
            _passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
        }

        private bool CanGenerate()
        {
            if (!(UseLetters || UseDigits || UsePunctuation)) return false;
            if (PasswordLength <= 0) return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanGenerate))]
        private void Generate()
        {
            Password = _passwordGenerator.Generate(
                PasswordLength,
                UseLetters,
                UseDigits,
                UsePunctuation
            );
        }

        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private bool CanCopy() => !string.IsNullOrWhiteSpace(Password);

        [RelayCommand(CanExecute = nameof(CanCopy))]
        private void Copy()
        {
            Clipboard.SetText(Password);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private bool _useLetters = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private bool _useDigits = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private bool _usePunctuation = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private uint _passwordLength = 16;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyCommand))]
        private string _password = string.Empty;
}