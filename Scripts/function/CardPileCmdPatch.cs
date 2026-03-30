using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


namespace cakemod.Patches
{
    [HarmonyPatch]
    public class CardPileCmdPatch
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(CardPileCmd).GetMethods()
                .Where(m => m.Name == "Add" && 
                       m.GetParameters().Length >= 2 &&
                       m.GetParameters()[0].ParameterType == typeof(CardModel) &&
                       m.GetParameters()[1].ParameterType == typeof(PileType));
        }

        [HarmonyPostfix]
        public static async void AddPostfix(Task<CardPileAddResult> __result, CardModel card, PileType newPileType)
        {
            var result = await __result;
            if (result.success && newPileType == PileType.Deck)
            {
                CardModelExtensions.InvokeAfterCardAddedToDeck(card);
                
                if (card is PerfectedStrike perfectedStrike)
                {
                    await PerfectedStrikeChoiceHelper.HandlePerfectedStrikeAddedToDeck(perfectedStrike);
                }
            }
        }
    }

    public static class CardModelExtensions
    {
        public static event System.Action<CardModel> AfterCardAddedToDeck;

        public static void InvokeAfterCardAddedToDeck(CardModel card)
        {
            AfterCardAddedToDeck?.Invoke(card);
        }
    }
}
