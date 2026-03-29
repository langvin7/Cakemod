using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using cakemod.Scripts;
using cakemod.Scripts.function;
using HarmonyLib;


namespace cakemod.Patches;

public static class PerfectedStrikeChoiceHelper
{
    public static async Task HandlePerfectedStrikeAddedToDeck(PerfectedStrike perfectedStrike)
    {
        var localPlayerId = PlatformUtil.GetLocalPlayerId(PlatformUtil.PrimaryPlatform);
        
        var combatState = perfectedStrike.CombatState;
        var runState = perfectedStrike.Owner.RunState;
        
        PlayerChoiceContext context;
        if (combatState != null)
        {
            context = new HookPlayerChoiceContext(perfectedStrike, localPlayerId, combatState, GameActionType.Combat);
        }
        else
        {
            context = new BlockingPlayerChoiceContext();
        }

        var choice1 = runState.CreateCard<PerfectBash>(perfectedStrike.Owner);
        var choice2 = runState.CreateCard<PerfectPerfected>(perfectedStrike.Owner);

        var choices = new List<CardModel> { choice1, choice2 };

        var selected = await CardSelectHelper.SelectFromChoices(context, choices, perfectedStrike.Owner);

        if (selected == choice1)
        {
            PerfectedStrikePatch.IncreaseDamage(perfectedStrike, 2);
            PerfectedStrikePatch.MarkSelected(perfectedStrike);
        }
        else if (selected == choice2)
        {
            PerfectedStrikePatch.IncreaseBlock(perfectedStrike, 5);
            PerfectedStrikePatch.MarkSelected(perfectedStrike);
        }
    }
}
