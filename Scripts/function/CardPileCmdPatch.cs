using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;


namespace cakemod.Patches
{
    [HarmonyPatch(typeof(CardPileCmd))]
    public class CardPileCmdPatch
    {
        [HarmonyPatch(nameof(CardPileCmd.Add))]
        [HarmonyPatch(new[] { typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool) })]
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
