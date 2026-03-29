using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace cakemod.Scripts.function;

public static class CardSelectHelper
{
	public static async Task<CardModel> SelectFromChoices(PlayerChoiceContext choiceContext, IReadOnlyList<CardModel> choices, Player player)
	{
		return await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, player);
	}
}
