using Common.Enums;

namespace Services.Settings;

public class GeneralSetting
{
    /// <summary>
    /// 윈도우 항상 위에 표시
    /// </summary>
    [Setting(false)]
    public bool? TopMost
    {
        get;
        set;
    }

    /// <summary>
    /// 종료시 창 위치 기억하기
    /// </summary>
    [Setting(true)]
    public bool? SaveWindowPosition
    {
        get;
        set;
    }
}
