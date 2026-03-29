using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace cakemod.Scripts;

[HarmonyPatch(typeof(DrumOfBattle), "ExtraHoverTips", MethodType.Getter)]
public static class DrumOfBattleExtraHoverTipsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<IHoverTip> __result)
    {
        __result = __result.Append(HoverTipFactory.FromCard<Thunder>());
    }
}



[HarmonyPatch(typeof(DrumOfBattle), "OnPlay")]
public static class DrumOfBattleOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(DrumOfBattle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(DrumOfBattle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, __instance.DynamicVars.Cards.BaseValue, __instance.Owner);
        await CreatureCmd.TriggerAnim(__instance.Owner.Creature, "Cast", __instance.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<CakeDrumOfBattlePower>(__instance.Owner.Creature, __instance.DynamicVars["DrumOfBattlePower"].BaseValue, __instance.Owner.Creature, __instance);
    }
}
