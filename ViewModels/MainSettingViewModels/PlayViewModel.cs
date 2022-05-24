using Common.Base;
using Common.Enums;
using Common.Helper;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ViewModels.MainSettingViewModels;

public class PlayViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;

    private EDefaultPlayMode _defaultPlayMode;

    public PlayViewModel(ISettingService settingService)
    {
        _settingService = settingService;

        DefaultPlayMode = _settingService.PlaySetting!.DefaultPlayMode;
    }

    #region Properties
    public EDefaultPlayMode DefaultPlayMode
    {
        get => _defaultPlayMode;
        set
        {
            SetProperty(ref _defaultPlayMode, value);

            _settingService.PlaySetting!.DefaultPlayMode = DefaultPlayMode;
            _settingService.SaveSetting();
        }
    }
    #endregion  // Properties

    #region Commands
    //
    #endregion  // Commands

    #region Commands Execute Methods
    //
    #endregion  // Commands Execute Methods

    #region Methods
    //
    #endregion  // Methods
}
