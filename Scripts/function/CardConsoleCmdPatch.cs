using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Combat;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace cakemod;

[HarmonyPatch(typeof(CardConsoleCmd), "Process")]
public class CardConsoleCmdPatch
{
    static void Postfix(Player? issuingPlayer, string[] args, ref object __result)
    {
        if (args.Length < 3)
        {
            return;
        }
        
        if (!int.TryParse(args[2], out int count) || count <= 1)
        {
            return;
        }
        
        if (!RunManager.Instance.IsInProgress)
        {
            return;
        }
        
        PileType pileType = PileType.Hand;
        if (args.Length >= 2)
        {
            var tryParseEnum = typeof(AbstractConsoleCmd).GetMethod("TryParseEnum", BindingFlags.Static | BindingFlags.NonPublic);
            if (tryParseEnum == null) return;
            var genericMethod = tryParseEnum.MakeGenericMethod(typeof(PileType));
            object[] parameters = new object[] { args[1], null };
            bool success = (bool)genericMethod.Invoke(null, parameters);
            if (!success)
            {
                return;
            }
            pileType = (PileType)parameters[1];
        }
        
        string cardName = args[0].ToUpperInvariant();
        CardModel cardModel = ModelDb.AllCards.FirstOrDefault((CardModel c) => c.Id.Entry == cardName);
        if (cardModel == null)
        {
            return;
        }
        
        ICardScope cardScope = pileType.IsCombatPile() 
            ? CombatManager.Instance.DebugOnlyGetState() 
            : RunManager.Instance.DebugOnlyGetState();
        
        for (int i = 1; i < count; i++)
        {
            CardModel card = cardScope.CreateCard(cardModel, issuingPlayer);
            CardPileCmd.Add(card, pileType);
        }
    }
}
