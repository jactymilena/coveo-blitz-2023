using System.Runtime.Serialization;

namespace Application.Models.Actions;

public enum ActionType
{
    [EnumMember(Value = "BUILD")]
    Build,

    [EnumMember(Value = "SELL")]
    Sell,

    [EnumMember(Value = "SEND_REINFORCEMENTS")]
    SendReinforcements
}
