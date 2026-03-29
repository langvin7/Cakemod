using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
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
using System.ComponentModel;


namespace cakemod.Scripts;

[HarmonyPatch(typeof(ExpectAFight), MethodType.Constructor)]
public class ExpectAFightConstructorPatch
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
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_2));
        
        return matcher.InstructionEnumeration();
    }
}


[HarmonyPatch(typeof(ExpectAFight), "CanonicalVars", MethodType.Getter)]
public static class ExpectAFightCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new CardsVar(2),
        new PowerVar<StrengthPower>(2m)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {

        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class ExpectAFightKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is ExpectAFight)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(ExpectAFight), "ExtraHoverTips", MethodType.Getter)]
public static class ExpectAFightExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(ExpectAFight), "OnPlay")]
public static class ExpectAFightOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ExpectAFight __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(ExpectAFight __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile pile = PileType.Draw.GetPile(__instance.Owner);
        IEnumerable<CardModel> source = pile.Cards.Where((CardModel c) => c.Type == CardType.Attack);
        IEnumerable<CardModel> enumerable = source.ToList().UnstableShuffle(__instance.Owner.RunState.Rng.CombatCardSelection).Take(__instance.DynamicVars.Cards.IntValue);
        foreach (CardModel card in enumerable)
        {
            await CardPileCmd.Add(card, PileType.Hand);
            if (card.EnergyCost.GetWithModifiers(CostModifiers.All) >= 1 && !card.EnergyCost.CostsX)
            {
                int newBaseCost = card.EnergyCost.GetWithModifiers(CostModifiers.All) - 1;
                card.EnergyCost.SetThisTurnOrUntilPlayed(newBaseCost);
            }
        }
         await PowerCmd.Apply<SetupStrikePower>(__instance.Owner.Creature, __instance.DynamicVars.Strength.BaseValue, __instance.Owner.Creature, __instance);
    }
}


[HarmonyPatch(typeof(ExpectAFight), "OnUpgrade")]
public static class ExpectAFightOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(ExpectAFight __instance)
    {
        __instance.DynamicVars.Cards.UpgradeValueBy(1m);
        return false;
    }
}
