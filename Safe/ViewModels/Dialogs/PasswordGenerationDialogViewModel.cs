using Prism.Commands;
using Prism.Services.Dialogs;
using System.Windows;
using System;
using EdlinSoftware.Safe.Services;
using Prism.Mvvm;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public class PasswordGenerationDialogViewModel : BindableBase, IDialogAware
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

            CopyCommand = new DelegateCommand(CopyPassword, CanCopy)
                .ObservesProperty(() => Password);

            CloseCommand = new DelegateCommand(Close);

            GenerateCommand = new DelegateCommand(GeneratePassword, CanGeneratePassword)
                .ObservesProperty(() => UseLetters)
                .ObservesProperty(() => UseDigits)
                .ObservesProperty(() => UsePunctuation)
                .ObservesProperty(() => PasswordLength);
        }

        private bool CanCopy() => !string.IsNullOrWhiteSpace(Password);

        private void GeneratePassword()
        {
            Password = _passwordGenerator.Generate(
                PasswordLength,
                UseLetters,
                UseDigits,
                UsePunctuation
            );
        }

        private bool CanGeneratePassword()
        {
            if (!(UseLetters || UseDigits || UsePunctuation)) return false;
            if (PasswordLength <= 0) return false;
            return true;
        }

        private void Close()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void CopyPassword()
        {
            Clipboard.SetText(Password);
        }

        private bool _useLetters = true;
        public bool UseLetters
        {
            get => _useLetters;
            set => SetProperty(ref _useLetters, value);
        }

        private bool _useDigits = true;
        public bool UseDigits
        {
            get => _useDigits;
            set => SetProperty(ref _useDigits, value);
        }

        private bool _usePunctuation = true;
        public bool UsePunctuation
        {
            get => _usePunctuation;
            set => SetProperty(ref _usePunctuation, value);
        }

        private uint _passwordLength = 16;
        public uint PasswordLength
        {
            get => _passwordLength;
            set => SetProperty(ref _passwordLength, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public DelegateCommand CopyCommand { get; }

        public DelegateCommand GenerateCommand { get; }

        public DelegateCommand CloseCommand { get; }
}