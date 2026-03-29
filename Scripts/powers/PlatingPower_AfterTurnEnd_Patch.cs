using HarmonyLib;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

// 注意：请确保引入了 CakeJuggernautPower 所在的命名空间

namespace cakemod.Scripts
{
    [HarmonyPatch(typeof(PlatingPower), nameof(PlatingPower.AfterTurnEnd))]
    public static class PlatingPower_AfterTurnEnd_Patch
    {
        public static bool Prefix(PlatingPower __instance, PlayerChoiceContext choiceContext, CombatSide side, ref Task __result)
        {
            // 原逻辑是：只有在 Enemy 回合结束时才结算
            if (side == CombatSide.Enemy)
            {
                // 判断拥有者是否为玩家 (Side != Enemy)
                if (__instance.Owner.Side != CombatSide.Enemy)
                {
                    // 检查玩家是否拥有 CakeJuggernautPower
                    if (__instance.Owner.HasPower<CakeJuggernautPower>())
                    {
                        // 满足条件：增加 1 层 PlatingPower
                        // PowerCmd.ModifyAmount 返回的是一个 Task，直接赋值给 __result
                        __result = PowerCmd.ModifyAmount(__instance, 1, null, null);
                        
                        // 返回 false 拦截原方法，这样就不会执行原有的 Decrement 逻辑了
                        return false;
                    }
                }
            }

            // 如果是敌人拥有 PlatingPower，或者玩家没有 CakeJuggernautPower，
            // 返回 true，继续执行原版逻辑
            return true;
        }
    }
}
