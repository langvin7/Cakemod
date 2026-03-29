using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace cakemod.Scripts;

/* [HarmonyPatch(typeof(Stampede), "ExtraHoverTips", MethodType.Getter)]
public static class StampedeExtraHoverTipsPatch
{
	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<IHoverTip> __result)
	{
		__result = __result.Append(HoverTipFactory.FromCard<Thunder>());
	}
}
 */
 
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
		await PowerCmd.Apply<CakeStampedePower>(__instance.Owner.Creature, 1m, __instance.Owner.Creature, __instance);
	}
}
