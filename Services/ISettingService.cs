using Common.Helper;
using Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services;

public interface ISettingService
{
    public GeneralSetting? GeneralSetting { get; }

    public PlaySetting? PlaySetting { get; }

    public void SaveSetting();
}

public class SettingService : ISettingService
{
    private GeneralSetting? _generalSetting;
    private PlaySetting? _playSetting;

    public SettingService()
    {
        if (File.Exists(PathHelper.GetLocalDirectory("Settings.xml")))
        {
            SettingsProvider settingsProvider = SerializeHelper.ReadDataFromXmlFile<SettingsProvider>(PathHelper.GetLocalDirectory("Settings.xml"), true);
            _generalSetting = settingsProvider.generalSetting ?? new GeneralSetting();
            _playSetting = settingsProvider.playSetting ?? new PlaySetting();
        }
        else
        {
            _generalSetting = new GeneralSetting();
            _playSetting = new PlaySetting();
            SaveSetting();
        }

        // 설정 기본값 적용
        SetDefaultValue();
    }

    public GeneralSetting? GeneralSetting { get => _generalSetting; }

    public PlaySetting? PlaySetting { get => _playSetting; }

    /// <summary>
    /// 설정 기본값 적용
    /// </summary>
    private void SetDefaultValue()
    {
        // TODO : 설정 카테고리가 늘어나면 여기 코드도 늘어나는데, 일단 이렇게 구현하고 추후 리펙토링 해서 줄여보자!

        // 일반 설정 기본 값 적용
        PropertyInfo[] propertyInfoArr = _generalSetting.GetType().GetProperties();
        foreach (PropertyInfo pi in propertyInfoArr)
        {
            if (pi.GetValue(_generalSetting) == null || (pi.GetValue(_generalSetting) is int && ((int)pi.GetValue(_generalSetting)) == 0))
            {
                SettingAttribute settingAtt = pi.GetCustomAttribute<SettingAttribute>();
                pi.SetValue(_generalSetting, settingAtt.DefaultValue);
            }
        }

        // 프로필 설정 기본 값 적용
        propertyInfoArr = _playSetting.GetType().GetProperties();
        foreach (PropertyInfo pi in propertyInfoArr)
        {
            if (pi.GetValue(_playSetting) == null || (pi.GetValue(_playSetting) is int && ((int)pi.GetValue(_playSetting)) == 0))
            {
                SettingAttribute settingAtt = pi.GetCustomAttribute<SettingAttribute>();
                pi.SetValue(_playSetting, settingAtt.DefaultValue);
            }
        }
    }

    public void SaveSetting()
    {
        SettingsProvider settingsProvider = new SettingsProvider();
        settingsProvider.generalSetting = GeneralSetting;
        settingsProvider.playSetting = PlaySetting;


        SerializeHelper.SaveDataToXml<SettingsProvider>(PathHelper.GetLocalDirectory("Settings.xml"), settingsProvider, true);
    }
}