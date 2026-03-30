using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using cakemod.Scripts.function;

namespace cakemod.Scripts;

public sealed class CakeDrumOfBattlePower : CakePowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[] 
	{ 
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromCard<Thunder>()
	};

	public override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player.Creature != base.Owner)
		{
			return;
		}
		Flash();
		CardModel thunder = combatState.CreateCard<Thunder>(base.Owner.Player);
		CardCmd.Preview(thunder);
		await CardPileCmd.AddGeneratedCardToCombat(thunder, PileType.Draw, addedByPlayer: true,CardPilePosition.Random);
		CardPile drawPile = PileType.Draw.GetPile(base.Owner.Player);
		for (int i = 0; i < base.Amount; i++)
		{
			if (drawPile.Cards.Any())
			{
				LocString prompt = Traverse.Create(this).Property("SelectionScreenPrompt").GetValue<LocString>();
				var selectedCards = (await CustomCardSelectCmd.FromDraw(
					context: choiceContext,
					player: base.Owner.Player,
					prefs: new CardSelectorPrefs(prompt, 0, 1),
					filter: null,
					source: this,
					topCount: 1
				)).ToList();

				if (selectedCards.Any())
				{
					CardModel thunder2 = combatState.CreateCard<Thunder>(base.Owner.Player);
					CardCmd.Preview(thunder2);
					await CardPileCmd.AddGeneratedCardToCombat(thunder2, PileType.Draw, addedByPlayer: true,CardPilePosition.Random);
				}
				else
				{
					CardModel cardModel = drawPile.Cards.First();
					await CardCmd.Exhaust(choiceContext, cardModel);
				}
			}
		}
	}
}
