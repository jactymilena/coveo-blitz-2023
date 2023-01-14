using System.Runtime.Serialization;

namespace Application.Models.Towers;

public enum TowerType
{
    [EnumMember(Value = "SPIKE_SHOOTER")]
    SpikeShooter,

    [EnumMember(Value = "SPEAR_SHOOTER")]
    SpearShooter,

    [EnumMember(Value = "BOMB_SHOOTER")]
    BombShooter
}
