using Common.Base;
using Common.Converters;
using Common.Enums;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.MainSettingViewModels;

namespace ViewModels;

public class MainSettingPopupViewModel : ViewModelBase
{
    private List<MainSettingPageInfo> _settingPageInfoList = new List<MainSettingPageInfo>(3);

    public MainSettingPopupViewModel()
    {
        this.CreateSettingPage();
    }

    #region Properties
    public List<MainSettingPageInfo> SettingPageInfoList
    {
        get => _settingPageInfoList;
        set => SetProperty(ref _settingPageInfoList, value);
    }
    #endregion  // Properties

    #region Commands
    #endregion  // Commands

    #region Commands Execute Methods
    //
    #endregion  // Commands Execute Methods

    #region Methods
    private void CreateSettingPage()
    {
        // 일반
        MainSettingPageInfo general = new MainSettingPageInfo()
        {
            PageName = "일반",
            PageViewModel = Ioc.Default.GetService<GeneralViewModel>()
        };
        // 재생
        MainSettingPageInfo play = new MainSettingPageInfo()
        {
            PageName = "재생",
            PageViewModel = Ioc.Default.GetService<PlayViewModel>()
        };
        // 정보
        MainSettingPageInfo about = new MainSettingPageInfo()
        {
            PageName = "정보",
            PageViewModel = new AboutViewModel()
        };

        List<MainSettingPageInfo> settingPageInfoList = new List<MainSettingPageInfo>(11);
        settingPageInfoList.Add(general);
        settingPageInfoList.Add(play);
        settingPageInfoList.Add(about);

        SettingPageInfoList = settingPageInfoList;
    }
    #endregion  // Methods
}
