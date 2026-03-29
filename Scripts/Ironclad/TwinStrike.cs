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




[HarmonyPatch(typeof(TwinStrike), "CanonicalVars", MethodType.Getter)]
public static class TwinStrikeCanonicalVarsPatch
{
	private static readonly DynamicVar[] ModifiedVars =
    [
		new DamageVar(8m, ValueProp.Move)
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<DynamicVar> __result)
	{
		__result = ModifiedVars;
	}
}

[HarmonyPatch(typeof(CardModel))]
[HarmonyPatch("CanonicalKeywords", MethodType.Getter)]
public static class TwinStrikeKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords =
    [
        CardKeyword.Exhaust
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is TwinStrike)
        {
            __result = ModifiedKeywords;
        }
    }
}

/*
[HarmonyPatch(typeof(TwinStrike), "ExtraHoverTips", MethodType.Getter)]
public static class TwinStrikeExtraHoverTipsPatch
{
	private static readonly IHoverTip[] ModifiedVars =
    [
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<IHoverTip> __result)
	{

		__result = ModifiedVars;
	}
}
*/

/*
[HarmonyPatch(typeof(TwinStrike), "OnPlay")]
public static class TwinStrikeOnPlayPatch
{
	[HarmonyPrefix]
	public static bool Prefix(TwinStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		__result = PatchedOnPlay(__instance, choiceContext, cardPlay);
		return false;
	}

	private static async Task PatchedOnPlay(TwinStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(__instance.DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard()
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
	}
}
*/

[HarmonyPatch(typeof(TwinStrike), "OnUpgrade")]
public static class TwinStrikeOnUpgradePatch
{
	[HarmonyPrefix]
	public static bool Prefix(TwinStrike __instance)
	{
		__instance.RemoveKeyword(CardKeyword.Exhaust);
		return false;
	}
}