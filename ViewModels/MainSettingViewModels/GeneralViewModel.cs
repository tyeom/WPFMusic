using Common.Base;
using Common.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.MainSettingViewModels;

public class GeneralViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;

    private bool? _topMost;
    private bool? _saveWindowPosition;

    public GeneralViewModel(ISettingService settingService)
    {
        _settingService = settingService;

        TopMost = _settingService.GeneralSetting!.TopMost;
        SaveWindowPosition = _settingService.GeneralSetting!.SaveWindowPosition;
    }

    #region Properties
    public bool? TopMost
    {
        get => _topMost;
        set
        {
            SetProperty(ref _topMost, value);

            _settingService.GeneralSetting!.TopMost = TopMost;
            _settingService.SaveSetting();
        }
    }

    public bool? SaveWindowPosition
    {
        get => _saveWindowPosition;
        set
        {
            SetProperty(ref _saveWindowPosition, value);

            _settingService.GeneralSetting!.SaveWindowPosition = SaveWindowPosition;
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
