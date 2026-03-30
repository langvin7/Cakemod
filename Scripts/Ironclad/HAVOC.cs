using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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

[HarmonyPatch(typeof(CardModel), "CanonicalVars", MethodType.Getter)]
public static class HavocCanonicalVarsPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<DynamicVar> __result)
    {
        if (__instance is Havoc)
        {
            var list = __result?.ToList() ?? new List<DynamicVar>();
            list.Add(new CardsVar(1));
            __result = list;
        }
    }
}

[HarmonyPatch(typeof(Havoc), "OnPlay")]
public static class HavocOnPlayPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Havoc __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchedOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PatchedOnPlay(Havoc __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        LocString prompt = Traverse.Create(__instance).Property("SelectionScreenPrompt").GetValue<LocString>();
        var selectedCards = (await CustomCardSelectCmd.FromDraw(
            context: choiceContext, 
            player: __instance.Owner, 
            prefs: new CardSelectorPrefs(prompt, 0, __instance.DynamicVars.Cards.IntValue), 
            filter: null, 
            source: __instance,
            topCount: __instance.DynamicVars.Cards.IntValue
        )).ToList();

        foreach (CardModel item in selectedCards)
        {
            await CardPileCmd.Add(item, PileType.Discard);
        }

        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, __instance.Owner, 1, CardPilePosition.Top, forceExhaust: true);
    }
}

[HarmonyPatch(typeof(Havoc), "OnUpgrade")]
public static class HavocOnUpgradePatch
{
	[HarmonyPrefix]
	public static bool Prefix(Havoc __instance)
	{
		__instance.DynamicVars.Cards.UpgradeValueBy(1m);
		return false;
	}
}