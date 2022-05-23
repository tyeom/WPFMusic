using Common.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Services;

namespace ViewModels;

public class ShellViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private object? _currentDataContext;

    public ShellViewModel()
    {
        //
    }

    #region Properties
    public object? CurrentDataContext
    {
        get => _currentDataContext;
        set => SetProperty(ref _currentDataContext, value);
    }
    #endregion  // Properties

    #region Commands
    //
    #endregion  // Commands

    #region Commands Execute Methods
    //
    #endregion  // Commands Execute Methods

    #region Methods
    private void ChangeDataContext(ViewModelBase obj)
    {
        CurrentDataContext = obj;
    }

    public override void Cleanup()
    {
        base.Cleanup();

        CurrentDataContext = null;
    }
    #endregion  // Methods
}