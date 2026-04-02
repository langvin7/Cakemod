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
using cakemod.Scripts.function;

namespace cakemod.Scripts;


[HarmonyPatch(typeof(BloodWall), "CanonicalVars", MethodType.Getter)]
public static class BloodWallCanonicalVarsPatch
{
	private static readonly DynamicVar[] ModifiedVars =
    [
		new HpLossVar(2m),
		new BlockVar(14m, ValueProp.Move),
		new PowerVar<PlatingPower>(2m)
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<DynamicVar> __result)
	{
		__result = ModifiedVars;
	}
}

/*
[HarmonyPatch(typeof(CardModel))]
[HarmonyPatch("CanonicalKeywords", MethodType.Getter)]
public static class BloodWallKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords =
    [
        CardKeyword.Exhaust
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is BloodWall)
        {
            __result = ModifiedKeywords;
        }
    }
}
*/

/*
[HarmonyPatch(typeof(BloodWall), "ExtraHoverTips", MethodType.Getter)]
public static class BloodWallExtraHoverTipsPatch
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


[HarmonyPatch(typeof(BloodWall), "OnPlay")]
public static class BloodWallOnPlayPatch
{
	[HarmonyPrefix]
	public static bool Prefix(BloodWall __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		__result = PatchedOnPlay(__instance, choiceContext, cardPlay);
		return false;
	}

	private static async Task PatchedOnPlay(BloodWall __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		VfxCmd.PlayOnCreatureCenter(__instance.Owner.Creature, "vfx/vfx_bloody_impact");
		await CreatureCmd.Damage(choiceContext, __instance.Owner.Creature, __instance.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, __instance);
		await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
		SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_BloodWall");
		VfxCmd.PlayOnCreature(__instance.Owner.Creature, "vfx/vfx_blood_wall");
		await CreatureCmd.GainBlock(__instance.Owner.Creature, __instance.DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<PlatingPower>(__instance.Owner.Creature, __instance.DynamicVars["PlatingPower"].BaseValue, __instance.Owner.Creature, __instance);
	}
}


[HarmonyPatch(typeof(BloodWall), "OnUpgrade")]
public static class BloodWallOnUpgradePatch
{
	[HarmonyPrefix]
	public static bool Prefix(BloodWall __instance)
	{
		__instance.DynamicVars.Block.UpgradeValueBy(3m);
		__instance.DynamicVars["PlatingPower"].UpgradeValueBy(1m);
		return false;
	}
}
