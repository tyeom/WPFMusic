using Common.Enums;

namespace Services.Settings;

public class PlaySetting
{
    /// <summary>
    /// 기본 재생 옵션
    /// </summary>
    public EDefaultPlayMode DefaultPlayMode
    {
        get;
        set;
    }
}
