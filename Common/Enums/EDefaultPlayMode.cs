using Common.Converters;
using System.ComponentModel;

namespace Common.Enums;

[TypeConverter(typeof(EnumDescriptionConverter<EDefaultPlayMode>))]
public enum EDefaultPlayMode
{
    [Description("한번재생")]
    Once_All,
    [Description("반복재생")]
    Repeat_All,
}
