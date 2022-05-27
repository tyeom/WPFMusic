using Common.Enums;

namespace Services.Settings;

public class PlaySetting
{
    /// <summary>
    /// 기본 재생 옵션
    /// </summary>
    [Setting(EDefaultPlayMode.Once_All)]
    public EDefaultPlayMode DefaultPlayMode
    {
        get;
        set;
    }

    /// <summary>
    /// 볼륨 설정
    /// </summary>
    [Setting(0.5f)]
    public float Volume
    {
        get;
        set;
    }
}
