using Common.Base;
using Common.Helper;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;
    private readonly IBassService _bassService;

    public MainViewModel(ISettingService settingService, IBassService bassService)
    {
        Logger.Log.Write("MainViewModel Constructor");

        _settingService = settingService;
        _bassService = bassService;

        Logger.Log.Write("MainViewModel Constructor End");
    }

    #region Properties
    public AlbumArtInfoViewModel? AlbumArtInfo
    {
        get => Ioc.Default.GetService<AlbumArtInfoViewModel>();
    }

    public ControlPanelViewModel? ControlPanel
    {
        get => Ioc.Default.GetService<ControlPanelViewModel>();
    }

    public PlayListViewModel? PlayList
    {
        get => Ioc.Default.GetService<PlayListViewModel>();
    }
    #endregion  // Properties

    #region Commands
    private RelayCommand _unloadedCommand;
    public RelayCommand UnloadedCommand
    {
        get
        {
            return _unloadedCommand ??
                (_unloadedCommand = new RelayCommand(this.UnloadedExecute));
        }
    }
    #endregion  // Commands

    #region Commands Execute Methods
    // NOTE : Program 종료시 이벤트 발생 안함.
    private void UnloadedExecute()
    {
        AlbumArtInfo?.Cleanup();
        ControlPanel?.Cleanup();
        PlayList?.Cleanup();

        this.Cleanup();
    }
    #endregion  // Commands Execute Methods

    #region Methods
    public override void Cleanup()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
    #endregion  // Methods
}
