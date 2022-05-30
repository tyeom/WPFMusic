using Common.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Services;
using ViewModels.Messaging;

namespace ViewModels;

public class ShellViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;
    private readonly IBassService _bassService;
    private readonly IDialogService _dialogService;

    public bool _windowTopMost = false;
    public double _windowLeft = 500;
    public double _windowTop = 500;
    public double _windowWidth = 310;
    public double _windowHeight = 650;

    private bool _isBassServiceInit = false;

    private IntPtr _mainHandle = IntPtr.Zero;

    private object? _currentDataContext;

    public ShellViewModel(ISettingService settingService, IBassService bassService)
    {
        _settingService = settingService;
        _bassService = bassService;

        WindowTopMost = _settingService.GeneralSetting!.TopMost ?? false;
        WindowLeft = _settingService.WindowSetting!.XPos ?? 500;
        WindowTop = _settingService.WindowSetting!.YPos ?? 500;
        WindowWidth = _settingService.WindowSetting!.Width ?? 340;
        WindowHeight = _settingService.WindowSetting!.Height ?? 650;

        WeakReferenceMessenger.Default.Register<MainHandleRequestMessage>(this, this.ResponseMainHandle);
    }

    #region Properties
    public bool WindowTopMost
    {
        get => _windowTopMost;
        set => SetProperty(ref _windowTopMost, value);
    }

    public double WindowLeft
    {
        get => _windowLeft;
        set => SetProperty(ref _windowLeft, value);
    }

    public double WindowTop
    {
        get => _windowTop;
        set => SetProperty(ref _windowTop, value);
    }

    public double WindowWidth
    {
        get => _windowWidth;
        set => SetProperty(ref _windowWidth, value);
    }

    public double WindowHeight
    {
        get => _windowHeight;
        set => SetProperty(ref _windowHeight, value);
    }

    public object? CurrentDataContext
    {
        get => _currentDataContext;
        set => SetProperty(ref _currentDataContext, value);
    }
    #endregion  // Properties

    #region Commands
    private RelayCommand? _mainSettingCommand;
    public RelayCommand? MainSettingCommand
    {
        get
        {
            return _mainSettingCommand ??
                (_mainSettingCommand = new RelayCommand(this.MainSettingExecute));
        }
    }

    private RelayCommand<object>? _activatedCommand;
    public RelayCommand<object>? ActivatedCommand
    {
        get
        {
            return _activatedCommand ??
                (_activatedCommand = new RelayCommand<object>(this.ActivatedExecute));
        }
    }

    private RelayCommand<object>? _closingCommand;
    public RelayCommand<object>? ClosingCommand
    {
        get
        {
            return _closingCommand ??
                (_closingCommand = new RelayCommand<object>(this.ClosingExecute));
        }
    }
    #endregion  // Commands

    #region Commands Execute Methods
    private void MainSettingExecute()
    {
        var dialogService = Ioc.Default.GetService<IDialogService>();
        if (dialogService!.CheckActivate("설정") is true)
        {
            // CheckActivate에서 해당 팝업 창 활성화
        }
        else
        {
            dialogService.SetSize(500, 650);
            dialogService.SetVM(new MainSettingPopupViewModel(), "설정");
            dialogService.Dialog.Show();
        }
    }

    private void ActivatedExecute(object? mainHandle)
    {
        if (_isBassServiceInit is false)
        {
            _mainHandle = (IntPtr)mainHandle!;
            _isBassServiceInit = _bassService.Initialize(_mainHandle);
        }
    }

    private void ClosingExecute(object? param)
    {
        _bassService.ChannelPostionTaskEnd();

        if(_settingService.GeneralSetting!.SaveWindowPosition is not null &&
            _settingService.GeneralSetting!.SaveWindowPosition is true)
        {
            // 0 - Window left
            // 1 - Window top
            // 2 - Window width
            // 3 - Window height
            object[] windowInfo = (object[])param!;

            _settingService.WindowSetting!.XPos = (double)windowInfo[0];
            _settingService.WindowSetting!.YPos = (double)windowInfo[1];
            _settingService.WindowSetting!.Width = (double)windowInfo[2];
            _settingService.WindowSetting!.Height = (double)windowInfo[3];
            _settingService.SaveSetting();
        }
    }
    #endregion  // Commands Execute Methods

    #region Methods
    private void ChangeDataContext(ViewModelBase obj)
    {
        CurrentDataContext = obj;
    }

    private void ResponseMainHandle(object recipient, MainHandleRequestMessage mainHandleRequestMessage)
    {
        mainHandleRequestMessage.Reply(_mainHandle);
    }

    public override void Cleanup()
    {
        base.Cleanup();

        CurrentDataContext = null;
    }
    #endregion  // Methods
}