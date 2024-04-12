using System.ComponentModel;

namespace BLL.Item_related;

public enum BidDuration
{
    
    [Description("One day")]
    OneDay,

    [Description("Three days")]
    ThreeDays,

    [Description("Five days")]
    FiveDays,

    [Description("Seven days")]
    SevenDays,

    [Description("Fourteen days")]
    FourteenDays,

    [Description("Twenty one days")]
    TwentyOneDays,

    [Description("Thirty days")]
    ThirtyDays
}