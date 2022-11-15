using System.ComponentModel.DataAnnotations;
using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Unity;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class ObservableViewModelBase : ObservableValidator, INavigationAware
{
    [Dependency] public IDialogService DialogService { get; set; } = null!;

    [Dependency] public IRegionManager RegionManager { get; set; } = null!;

    private IEventAggregator? _eventAggregator;

    [Dependency]
    public IEventAggregator EventAggregator
    {
        get => _eventAggregator!;
        set
        {
            _eventAggregator = value;
            if (_eventAggregator != null)
            {
                SubscribeToEvents();
            }
        }
    }

    protected virtual void SubscribeToEvents() { }

    public virtual void OnNavigatedTo(NavigationContext navigationContext) { }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }
}

public sealed class IsNotNullOrEmptyAttribute : ValidationAttribute
{
    public IsNotNullOrEmptyAttribute(string? message = null)
    {
        Message = message;
    }

    public string? Message { get; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string strValue)
            throw new InvalidOperationException(
                $"{nameof(IsNotNullOrEmptyAttribute)} can only be applied to string properties");

        if (!string.IsNullOrEmpty(strValue))
        {
            return ValidationResult.Success!;
        }

        return new(Message ?? $"{validationContext.DisplayName} {Application.Current.Resources["PropertyCantBeEmptyValidationMessage"]}");
    }
}

public sealed class IsNotNullOrWhiteSpaceAttribute : ValidationAttribute
{
    public IsNotNullOrWhiteSpaceAttribute(string? message = null)
    {
        Message = message;
    }

    public string? Message { get; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string strValue)
            throw new InvalidOperationException(
                $"{nameof(IsNotNullOrWhiteSpaceAttribute)} can only be applied to string properties");

        if (!string.IsNullOrWhiteSpace(strValue))
        {
            return ValidationResult.Success!;
        }

        return new(Message ?? $"{validationContext.DisplayName} {Application.Current.Resources["PropertyCantBeEmptyValidationMessage"]}");
    }
}