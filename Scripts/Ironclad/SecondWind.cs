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

/* [HarmonyPatch(typeof(SecondWind), MethodType.Constructor)]
public class SecondWindConstructorPatch
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


[HarmonyPatch(typeof(SecondWind), "CanonicalVars", MethodType.Getter)]
public static class SecondWindCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new BlockVar(4m, ValueProp.Move)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class SecondWindKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is SecondWind)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(SecondWind), "ExtraHoverTips", MethodType.Getter)]
public static class SecondWindExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


/* [HarmonyPatch(typeof(SecondWind), "OnPlay")]
public static class SecondWindOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SecondWind __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(SecondWind __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
} */


[HarmonyPatch(typeof(SecondWind), "OnUpgrade")]
public static class SecondWindOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(SecondWind __instance)
    {
        __instance.DynamicVars.Block.UpgradeValueBy(2m);
        return false;
    }
}

[HarmonyPatch(typeof(SecondWind), "GetCards")]
public static class SecondWindGetCardsPatch
{
    [HarmonyPrefix]
    public static bool Prefix(SecondWind __instance, ref IEnumerable<CardModel> __result)
    {

        CardPile handPile = PileType.Hand.GetPile(__instance.Owner);
        IEnumerable<CardModel> handCards = handPile.Cards.Where(c => c.Type != CardType.Attack);


        CardPile discardPile = PileType.Discard.GetPile(__instance.Owner);
        CardPile draw = PileType.Draw.GetPile(__instance.Owner);
        IEnumerable<CardModel> DisCards = discardPile.Cards.Where(c => c.Type == CardType.Curse || c.Type == CardType.Status);
        __result = handCards.Concat(DisCards);

        return false;
    }
}