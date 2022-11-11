using System;
using System.Windows;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public abstract class ObservableDialogBase : ObservableViewModelBase, IDialogAware
{
    public virtual bool CanCloseDialog() => true;

    public virtual void OnDialogClosed() { }

    public virtual void OnDialogOpened(IDialogParameters parameters) { }

    public string Title { get; protected set; } = string.Empty;

    protected void SetTitleFromResource(string resourceName)
    {
        Title = (string) Application.Current.Resources[resourceName];
    }

    public event Action<IDialogResult>? RequestClose;

    protected void RequestDialogClose(ButtonResult result, IDialogParameters? parameters = null)
    {
        RequestClose?.Invoke(
            parameters == null
                ? new DialogResult(result)
                : new DialogResult(result, parameters)
        );
    }
}