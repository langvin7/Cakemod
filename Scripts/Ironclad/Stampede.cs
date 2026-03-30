using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace cakemod.Scripts;

 
[HarmonyPatch(typeof(CardModel))]
[HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
public static class StampedeExtraHoverTipsPatch
{
    private static readonly IEnumerable<IHoverTip> ModifiedExtraHoverTipss =
    [
        HoverTipFactory.FromCard<Thunder>()
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is Stampede)
        {
            __result = ModifiedExtraHoverTipss;
        }
    }
}

[HarmonyPatch(typeof(Stampede), "OnPlay")]
public static class StampedeOnPlayPatch
{
	[HarmonyPrefix]
	public static bool Prefix(Stampede __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		__result = PatchedOnPlay(__instance, choiceContext, cardPlay);
		return false;
	}

	private static async Task PatchedOnPlay(Stampede __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
		
		var existingPower = __instance.Owner.Creature.GetPower<CakeStampedePower>();
		if (existingPower != null)
		{
			if (__instance.IsUpgraded && !existingPower.isUpgraded)
			{
				await PowerCmd.Remove(existingPower);
				var newPower = await PowerCmd.Apply<CakeStampedePower>(__instance.Owner.Creature, 1m, __instance.Owner.Creature, __instance);
				newPower.isUpgraded = true;
				((IntVar)newPower.DynamicVars["ThunderCount"]).BaseValue = 3;
			}
			return;
		}
		
		var power = await PowerCmd.Apply<CakeStampedePower>(__instance.Owner.Creature, 1m, __instance.Owner.Creature, __instance);
		power.isUpgraded = __instance.IsUpgraded;
		((IntVar)power.DynamicVars["ThunderCount"]).BaseValue = __instance.IsUpgraded ? 3 : 4;
	}
}

[HarmonyPatch(typeof(Stampede), "OnUpgrade")]
public static class StampedeOnUpgradePatch
{
    [HarmonyPrefix]
    public static bool Postfix(Stampede __instance)
    {
        return false;
    }
}
