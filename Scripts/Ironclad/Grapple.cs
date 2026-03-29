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
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(Grapple), MethodType.Constructor)]
public class GrappleConstructorPatch
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


[HarmonyPatch(typeof(Grapple), "CanonicalVars", MethodType.Getter)]
public static class GrappleCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new DamageVar(6m, ValueProp.Move),
        new CardsVar(4)
    ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        
        __result = ModifiedVars;
    }
}


/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class GrappleKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Grapple)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(Grapple), "ExtraHoverTips", MethodType.Getter)]
public static class GrappleExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(Grapple), "OnPlay")]
public static class GrappleOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Grapple __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Grapple __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).FromCard(__instance).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        LocString prompt = Traverse.Create(__instance).Property("SelectionScreenPrompt").GetValue<LocString>();
        var selectedCards = (await CustomCardSelectCmd.FromDraw(
            prefs: new CardSelectorPrefs(prompt, 0, __instance.DynamicVars.Cards.IntValue), 
            context: choiceContext, 
            player: __instance.Owner, 
            filter: null, 
            source: __instance,
            topCount: __instance.DynamicVars.Cards.IntValue
        )).ToList(); // 使用 ToList() 将其转换为列表，方便统计数量

        int nonAttackCount = selectedCards.Count(item => item.Type != CardType.Attack);

        if (nonAttackCount >= 2)
        {
            
        }

        foreach (CardModel item in selectedCards)
        {
            await CardPileCmd.Add(item, PileType.Discard);
        }
    }
}


[HarmonyPatch(typeof(Grapple), "OnUpgrade")]
public static class GrappleOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Grapple __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(2m);
        __instance.DynamicVars.Cards.UpgradeValueBy(2m);
        return false;
    }
}
