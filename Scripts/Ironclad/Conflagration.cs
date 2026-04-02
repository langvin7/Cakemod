using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Helpers;


namespace cakemod.Scripts;


[HarmonyPatch(typeof(CardModel), "ExtraHoverTips", MethodType.Getter)]
public static class ConflagrationExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedHoverTips =
    [
        HoverTipFactory.FromKeyword(CakeKeywords.PlayFromDrawPile)
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is Conflagration)
        {
            __result = ModifiedHoverTips;
        }
    }
}


[HarmonyPatch(typeof(Conflagration), "CanonicalVars", MethodType.Getter)]
public static class ConflagrationCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new CalculationBaseVar(3m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => CombatManager.Instance.History.CardPlaysFinished.Count(delegate(CardPlayFinishedEntry e)
        {
            if (!e.HappenedThisTurn(card.CombatState))
            {
                return false;
            }
            if (e.CardPlay.Card.Type != CardType.Attack)
            {
                return false;
            }
            return (e.CardPlay.Card.Owner == card.Owner) ? true : false;
        }))
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class ConflagrationKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Conflagration)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(Conflagration), "ExtraHoverTips", MethodType.Getter)]
public static class ConflagrationExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(Conflagration), "OnPlay")]
public static class ConflagrationOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Conflagration __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Conflagration __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
                IReadOnlyList<Creature> hittableEnemies = __instance.CombatState.HittableEnemies;
        foreach (Creature item in hittableEnemies)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(item));
        }

        await DamageCmd.Attack(__instance.DynamicVars.CalculatedDamage).FromCard(__instance).TargetingAllOpponents(__instance.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .WithHitCount(2)
            .Execute(choiceContext);
    }
}


[HarmonyPatch(typeof(Conflagration), "OnUpgrade")]
public static class ConflagrationOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Conflagration __instance)
    {
        __instance.DynamicVars.CalculationBase.UpgradeValueBy(2m);
        return false;
    }
}