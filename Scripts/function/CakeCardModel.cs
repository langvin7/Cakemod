using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace cakemod.Scripts.function;

public abstract class CakeCardModel : CustomCardModel
{
    public override string PortraitPath => $"res://image/cards/{base.Id.Entry.ToLowerInvariant()}.png";

    public CakeCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) 
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
