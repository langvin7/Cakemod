using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Entities.Cards;

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

        for (int i = 0; i < amount; i++)
        {
            combatRoom.AddExtraReward(issuingPlayer, new CardReward(CardCreationOptions.ForRoom(issuingPlayer, combatRoom.RoomType), 3, issuingPlayer));
        }

        return new CmdResult(success: true, $"Added {amount} card reward(s) to the combat room.");
    }
}
