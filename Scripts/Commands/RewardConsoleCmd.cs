using System;
using System.Linq;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;

namespace CakeMod.Commands;

public class CakeRewardConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "cakereward";

    public override string Args => "<int:amount>";

    public override string Description => "Gives extra card rewards to the player in combat room.";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (issuingPlayer == null)
        {
            return new CmdResult(success: false, "This command only works during a run.");
        }

        if (!RunManager.Instance.IsInProgress)
        {
            return new CmdResult(success: false, "A run is currently not in progress!");
        }

        if (!(RunManager.Instance.DebugOnlyGetState()?.CurrentRoom is CombatRoom combatRoom))
        {
            return new CmdResult(success: false, "This command only works in combat rooms.");
        }

        int amount = 1;
        if (args.Length > 0)
        {
            if (!int.TryParse(args[0], out amount) || amount <= 0)
            {
                return new CmdResult(success: false, "Amount must be a positive integer.");
            }
        }

        var perfectedStrikeModel = ModelDb.AllCards.FirstOrDefault(c => c.Id.Entry == "PERFECTED_STRIKE");
        if (perfectedStrikeModel == null)
        {
            return new CmdResult(success: false, "PerfectedStrike card not found.");
        }

        for (int i = 0; i < amount; i++)
        {
            var reward = new CardReward(CardCreationOptions.ForRoom(issuingPlayer, combatRoom.RoomType), 2, issuingPlayer);
            reward.AfterGenerated += () => {
                var perfectedStrike = issuingPlayer.RunState.CreateCard(perfectedStrikeModel, issuingPlayer);
                var cardsField = reward.GetType().GetField("_cards", BindingFlags.NonPublic | BindingFlags.Instance);
                var cardsList = cardsField?.GetValue(reward);
                var resultType = Type.GetType("MegaCrit.Sts2.Core.Entities.Cards.CardCreationResult, Sts2");
                var resultInstance = Activator.CreateInstance(resultType, perfectedStrike);
                cardsList?.GetType().GetMethod("Add")?.Invoke(cardsList, new[] { resultInstance });
            };
            combatRoom.AddExtraReward(issuingPlayer, reward);
        }

        return new CmdResult(success: true, $"Added {amount} card reward(s) to the combat room.");
    }
}
