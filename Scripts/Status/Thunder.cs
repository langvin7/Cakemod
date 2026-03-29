using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Platform;
using cakemod.Scripts.function;

namespace cakemod.Scripts;


[Pool(typeof(StatusCardPool))]
public class Thunder : CakeCardModel
{
    // 基础耗能 (-1表示不可打出等特殊状态，这里不可打出使用关键字Unplayable，并将cost设为-1)
    private const int energyCost = -1;
    // 卡牌类型
    private const CardType type = CardType.Status;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Status;
    // 目标类型
    private const TargetType targetType = TargetType.None;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];


    public override bool HasTurnEndInHandEffect => false;

    public Thunder() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterCardChangedPilesLate))]
public static class Thunder_PileChange_Patch
{
    private static bool _isTriggering = false;

    public static void Postfix(AbstractModel __instance, ref Task __result, CardModel card, PileType oldPileType, AbstractModel source)
    {
        if (__instance == card && card is Thunder thunderCard)
        {
            __result = ExecuteThunderLogicAsync(__result, thunderCard);
        }
    }

    private static async Task ExecuteThunderLogicAsync(Task originalTask, Thunder thunderCard)
    {
        if (originalTask != null)
        {
            await originalTask;
        }

        if (_isTriggering) return;

        PileType currentPile = thunderCard.Pile?.Type ?? PileType.None;

        if (currentPile == PileType.Discard)
        {
            try
            {
                _isTriggering = true;
                var localPlayerId = PlatformUtil.GetLocalPlayerId(PlatformUtil.PrimaryPlatform);
                var context = new HookPlayerChoiceContext(thunderCard, localPlayerId, thunderCard.CombatState, GameActionType.Combat);
                await CardCmd.Exhaust(context, thunderCard);
            }
            finally
            {
                _isTriggering = false;
            }
        }
    }
}
