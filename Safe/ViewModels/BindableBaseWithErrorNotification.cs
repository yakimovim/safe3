using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Prism.Mvvm;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class BindableBaseWithErrorNotification 
    : BindableBase, INotifyDataErrorInfo
{
    protected IDictionary<string, string> ValidationErrors = new Dictionary<string, string>();

    public IEnumerable GetErrors(string? propertyName)
    {
        if(string.IsNullOrWhiteSpace(propertyName)) return Array.Empty<string>();

        if (!ValidationErrors.ContainsKey(propertyName)) return Array.Empty<string>();

        return new [] { ValidationErrors[propertyName] };
    }

    public bool HasErrors => ValidationErrors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected void RaiseErrorsChanged(string propertyName)
    {
        if(string.IsNullOrWhiteSpace(propertyName)) return;

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    protected void CheckNullOrWhiteSpace(string propertyValue, string propertyName, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(propertyValue))
        {
            ValidationErrors[propertyName] = message ?? $"{propertyName} can't be empty";
        }
        else
        {
            ValidationErrors.Remove(propertyName);
        }

        RaiseErrorsChanged(propertyName);
    }

    protected void CheckNullOrEmpty(string propertyValue, string propertyName, string? message = null)
    {
        if (string.IsNullOrEmpty(propertyValue))
        {
            ValidationErrors[propertyName] = message ?? $"{propertyName} can't be empty";
        }
        else
        {
            ValidationErrors.Remove(propertyName);
        }

        RaiseErrorsChanged(propertyName);
    }
}