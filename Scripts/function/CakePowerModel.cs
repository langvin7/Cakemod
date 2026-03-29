using BaseLib.Abstracts;

namespace cakemod.Scripts.function;

public abstract class CakePowerModel : CustomPowerModel
{
    public override string? CustomPackedIconPath => $"res://image/powers/{GetType().Name}.png";
    public override string? CustomBigIconPath => $"res://image/powers/{GetType().Name}.png";
}
