using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace cakemod;

public static class AutoPlayHelper
{
    public static async Task TryAutoPlay(Player player, CardModel card, int energy)
    {
        if (player.PlayerCombatState.Energy < energy)
            return;

        NCapstoneContainer.Instance?.Close();

        var choiceContext = new BlockingPlayerChoiceContext();
        await CardCmd.AutoPlay(choiceContext, card, null);

        player.PlayerCombatState.LoseEnergy(energy);

        // CardCmd.AutoPlay 绕过了 ActionExecutor 循环，
        // 需要手动检查胜利条件，否则杀死所有敌人后战斗不会结束
        await CombatManager.Instance.CheckWinCondition();
    }
}
