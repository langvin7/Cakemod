using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Reflection.Emit;


namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(SwordBoomerang), MethodType.Constructor)]
public class SwordBoomerangConstructorPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Call, AccessTools.DeclaredConstructor(
                typeof(CardModel), 
                new[] { typeof(int), typeof(CardType), typeof(CardRarity), typeof(TargetType), typeof(bool) }
            ))
        );

        if (matcher.IsInvalid) return instructions;

        matcher.Advance(-5);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0));
        
        return matcher.InstructionEnumeration();
    }
} */


[HarmonyPatch(typeof(SwordBoomerang), "CanonicalVars", MethodType.Getter)]
public static class SwordBoomerangCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(2)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class SwordBoomerangKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is SwordBoomerang)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(SwordBoomerang), "ExtraHoverTips", MethodType.Getter)]
public static class SwordBoomerangExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/

/* 
[HarmonyPatch(typeof(SwordBoomerang), "OnPlay")]
public static class SwordBoomerangOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SwordBoomerang __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(SwordBoomerang __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}
 */




[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterCardChangedPilesLate))]
public static class SwordBoomerang_PileChange_Patch
{

    private static bool _isTriggering = false;

    public static void Postfix(AbstractModel __instance, ref Task __result, CardModel card, PileType oldPileType, AbstractModel source)
    {
        if (__instance == card && card is SwordBoomerang boomerang)
        {
            __result = ExecuteBoomerangLogicAsync(__result, boomerang, oldPileType);
        }
    }

    // 自定义的异步追加逻辑
    private static async Task ExecuteBoomerangLogicAsync(Task originalTask, SwordBoomerang boomerang, PileType oldPileType)
    {
        // 确保游戏原有的逻辑先执行完毕
        if (originalTask != null)
        {
            await originalTask;
        }

        if (_isTriggering) return;
        PileType currentPile = boomerang.Pile?.Type ?? PileType.None;

        bool isDiscardToDraw = (oldPileType == PileType.Discard && currentPile == PileType.Draw);
        bool isDrawToDiscard = (oldPileType == PileType.Draw && currentPile == PileType.Discard);

        if (isDiscardToDraw || isDrawToDiscard)
        {
            try
            {

                _isTriggering = true;
                CardCmd.Preview(boomerang);
                await DamageCmd.Attack(boomerang.DynamicVars.Damage.BaseValue)
                    .WithHitCount(boomerang.IsUpgraded?2:1)
                    .FromCard(boomerang)
                    .TargetingRandomOpponents(boomerang.CombatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(null); 
            }
            finally
            {
                // 无论是否发生异常，最后必须解锁
                _isTriggering = false;
            }
        }
    }
}



/* [HarmonyPatch(typeof(SwordBoomerang), "OnUpgrade")]
public static class SwordBoomerangOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(SwordBoomerang __instance)
    {
        return false;
    }
} */