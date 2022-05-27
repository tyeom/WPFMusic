using Common.Enums;

namespace Services.Settings;

public class WindowSetting
{
    /// <summary>
    /// 창 표시 X 좌표
    /// </summary>
    [Setting(100)]
    public double? XPos
    {
        get;
        set;
    }

    /// <summary>
    /// 창 표시 Y 좌표
    /// </summary>
    [Setting(100)]
    public double? YPos
    {
        get;
        set;
    }

    /// <summary>
    /// 창 크기 width
    /// </summary>
    [Setting(340d)]
    public double? Width
    {
        get;
        set;
    }

    /// <summary>
    /// 창 크기 height
    /// </summary>
    [Setting(650d)]
    public double? Height
    {
        get;
        set;
    }
}
