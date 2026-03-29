using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using cakemod.Scripts.function;

namespace cakemod.Scripts;

[Pool(typeof(StatusCardPool))]
public class PerfectGrandFinale : CakeCardModel
{
    private const int energyCost = -1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public PerfectGrandFinale() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
