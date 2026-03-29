using HarmonyLib;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Combat; 


using System.Runtime.CompilerServices;



namespace cakemod.Scripts
{
    // ---------------------------------------------------------
    // 补丁 1：拦截受伤后逻辑，实现“第一次受伤抽牌”
    // ---------------------------------------------------------
    [HarmonyPatch(typeof(RupturePower), nameof(RupturePower.AfterDamageReceived))]
    public static class RupturePower_AfterDamageReceived_Patch
    {
        // 用于记录每个 RupturePower 实例本回合是否已经触发过抽牌
        public class PowerState 
        { 
            public bool HasDrawnThisTurn = false; 
        }
        
        // ConditionalWeakTable 能确保当 RupturePower 被销毁时，状态也会自动回收，防止内存泄漏
        public static readonly ConditionalWeakTable<RupturePower, PowerState> powerStates = new ConditionalWeakTable<RupturePower, PowerState>();

        public static void Postfix(
            ref Task __result, 
            RupturePower __instance, 
            PlayerChoiceContext choiceContext, 
            Creature target, 
            DamageResult result)
        {
            __result = PostfixAsyncRoutine(__result, __instance, choiceContext, target, result);
        }

        private static async Task PostfixAsyncRoutine(
            Task originalTask, 
            RupturePower __instance, 
            PlayerChoiceContext choiceContext, 
            Creature target, 
            DamageResult result)
        {
            // 1. 等待原版“撕裂”加力量的逻辑执行完毕
            await originalTask;

            // 2. 触发条件：受击者是拥有者、受到未格挡伤害、且在己方回合
            if (target == __instance.Owner && result.UnblockedDamage > 0 && __instance.CombatState.CurrentSide == __instance.Owner.Side)
            {
                // 获取当前能力实例的自定义状态
                var state = powerStates.GetOrCreateValue(__instance);

                // 3. 判断本回合是否还没触发过抽牌
                if (!state.HasDrawnThisTurn)
                {
                    // 标记为已触发
                    state.HasDrawnThisTurn = true;

                    // 4. 利用 PowerModel 源码中提供的 IsPlayer 和 Player 属性，安全获取 Player 对象并抽牌
                    if (__instance.Owner.IsPlayer && __instance.Owner.Player != null)
                    {
                        await CardPileCmd.Draw(choiceContext, __instance.Owner.Player);
                    }
                }
            }
        }
    }

    // ---------------------------------------------------------
    // 补丁 2：在回合结束时重置抽牌状态
    // ---------------------------------------------------------
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterTurnEnd))] 
    public static class RupturePower_TurnReset_Patch
    {
        // 使用 Prefix 是因为重置状态是同步操作，直接在方法调用之初执行即可，不影响原方法的 Task 返回
        public static void Prefix(AbstractModel __instance, CombatSide side)
        {
            // 如果当前触发回合结束的是 RupturePower
            if (__instance is RupturePower rupture)
            {
                // 确保是该能力拥有者所在阵营的回合结束
                if (rupture.Owner != null && side == rupture.Owner.Side)
                {
                    // 将抽牌标记重置为 false
                    if (RupturePower_AfterDamageReceived_Patch.powerStates.TryGetValue(rupture, out var state))
                    {
                        state.HasDrawnThisTurn = false; 
                    }
                }
            }
        }
    }
}

