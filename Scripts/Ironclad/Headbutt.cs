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
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Extensions;


namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(Headbutt), MethodType.Constructor)]
public class HeadbuttConstructorPatch
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

/* 
[HarmonyPatch(typeof(Headbutt), "CanonicalVars", MethodType.Getter)]
public static class HeadbuttCanonicalVarsPatch
{
    private static readonly DynamicVar[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<DynamicVar> __result)
    {
        __result = ModifiedVars;
    }
}
 */

/* 
[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class HeadbuttKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords = [ CardKeyword.Exhaust ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is Headbutt)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(Headbutt), "ExtraHoverTips", MethodType.Getter)]
public static class HeadbuttExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(Headbutt), "OnPlay")]
public static class HeadbuttOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Headbutt __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Headbutt __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).FromCard(__instance).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
            
        // 【修改这里】使用 Traverse 获取受保护的成员
        var prompt = Traverse.Create(__instance).Property("SelectionScreenPrompt").GetValue<MegaCrit.Sts2.Core.Localization.LocString>();
        CardSelectorPrefs prefs = new CardSelectorPrefs(prompt, 1);
        
        CardPile pile = PileType.Discard.GetPile(__instance.Owner);
        CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, __instance.Owner, prefs)).FirstOrDefault();
        if (cardModel != null)
        {
            if(cardModel.IsUpgradable)
            {
                CardCmd.Upgrade(cardModel);
            }
            await CardPileCmd.Add(cardModel, PileType.Draw, CardPilePosition.Top);
       }

           }
}



/* [HarmonyPatch(typeof(Headbutt), "OnUpgrade")]
public static class HeadbuttOnUpgradePatch
{
    [HarmonyPostfix]
    public static bool Postfix(Headbutt __instance)
    {
        return false;
    }
} */