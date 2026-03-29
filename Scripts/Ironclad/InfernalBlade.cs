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

[HarmonyPatch(typeof(InfernalBlade), MethodType.Constructor)]
public class InfernalBladeConstructorPatch
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

        matcher.Advance(-4);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)CardType.Attack)); 

        matcher.Advance(2);
        matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4, (int)TargetType.AnyEnemy)); 
        
        return matcher.InstructionEnumeration();
    }
}



[HarmonyPatch(typeof(InfernalBlade), "CanonicalKeywords", MethodType.Getter)]
public static class InfernalBladeCanonicalKeywordsPatch
{
	private static readonly IEnumerable<CardKeyword> ModifiedVars =
    [
        CardKeyword.Exhaust,
        CardKeyword.Innate
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<CardKeyword> __result)
	{
		__result = ModifiedVars;
	}
}




[HarmonyPatch(typeof(CardModel), "CanonicalVars", MethodType.Getter)]
public static class InfernalBladeKeywordPatch
{
    private static readonly DynamicVar[] ModifiedVars = [
        new PowerVar<VulnerablePower>(2m),
		new DamageVar(8m, ValueProp.Move)
     ] ;
        

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<DynamicVar> __result)
    {
        if (__instance is InfernalBlade)
        {
            __result = ModifiedVars;
        }
    }
}


/*
[HarmonyPatch(typeof(InfernalBlade), "ExtraHoverTips", MethodType.Getter)]
public static class InfernalBladeExtraHoverTipsPatch
{
    private static readonly IHoverTip[] ModifiedVars = [ ];

    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = ModifiedVars;
    }
}
*/


[HarmonyPatch(typeof(InfernalBlade), "OnPlay")]
public static class InfernalBladeOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(InfernalBlade __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(InfernalBlade __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
     	ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).FromCard(__instance).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target,__instance.DynamicVars.Vulnerable.BaseValue, __instance.Owner.Creature, __instance);
		IEnumerable<CardModel> enumerable = PileType.Draw.GetPile(__instance.Owner).Cards.ToList();
		foreach (CardModel item in enumerable)
		{
			await CardPileCmd.Add(item, PileType.Discard);
		}   
    }
}


[HarmonyPatch(typeof(InfernalBlade), "OnUpgrade")]
public static class InfernalBladeOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(InfernalBlade __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(4m);
        return false;
    }
}