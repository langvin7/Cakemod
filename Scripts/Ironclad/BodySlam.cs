using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Reflection.Emit;



namespace cakemod.Scripts;

 [HarmonyPatch(typeof(BodySlam), MethodType.Constructor)]
    public class BodySlamConstructorPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 使用 CodeMatcher 可以非常安全且精准地定位和修改 IL
            var matcher = new CodeMatcher(instructions);

            // 寻找调用基类 CardModel 构造函数的指令
            // CardModel 的构造函数签名为: (int, CardType, CardRarity, TargetType, bool)
            // 注意: bool shouldShowInCardLibrary = true 是默认参数，编译器会自动在调用方补全这个 true
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Call, AccessTools.DeclaredConstructor(
                    typeof(CardModel), 
                    new[] { typeof(int), typeof(CardType), typeof(CardRarity), typeof(TargetType), typeof(bool) }
                ))
            );

            if (matcher.IsInvalid)
            {
                // 如果因为游戏更新导致没找到，直接返回原指令防止崩溃
                return instructions;
            }

            // 此时 matcher 停在 Call 指令上。
            // 往回倒退 5 步，正好是第一个参数 canonicalEnergyCost 的压栈指令 (ldc.i4.1)
            // 栈的推入顺序是: this -> cost -> type -> rarity -> targetType -> shouldShow
            matcher.Advance(-5);

            // 将费用修改为你想要的值，例如修改为 0 费
            matcher.SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_0));
            
            // 如果你想改成 2 费，使用: new CodeInstruction(OpCodes.Ldc_I4_2)
            // 如果你想改成 3 费，使用: new CodeInstruction(OpCodes.Ldc_I4_3)

            return matcher.InstructionEnumeration();
        }
    }

/* 
[HarmonyPatch(typeof(BodySlam), "CanonicalVars", MethodType.Getter)]
public static class BodySlamCanonicalVarsPatch
{
	private static readonly DynamicVar[] ModifiedVars =
    [

    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<DynamicVar> __result)
	{
		__result = ModifiedVars;
	}
}
 */
/* 
[HarmonyPatch(typeof(CardModel))]
[HarmonyPatch("CanonicalKeywords", MethodType.Getter)]
public static class BodySlamKeywordPatch
{
    private static readonly IEnumerable<CardKeyword> ModifiedKeywords =
    [
        CardKeyword.Exhaust
    ];

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is BodySlam)
        {
            __result = ModifiedKeywords;
        }
    }
}
 */

/*
[HarmonyPatch(typeof(BodySlam), "ExtraHoverTips", MethodType.Getter)]
public static class BodySlamExtraHoverTipsPatch
{
	private static readonly IHoverTip[] ModifiedVars =
    [
    ];

	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<IHoverTip> __result)
	{

		__result = ModifiedVars;
	}
}
*/


/* [HarmonyPatch(typeof(BodySlam), "OnPlay")]
public static class BodySlamOnPlayPatch
{
	[HarmonyPrefix]
	public static bool Prefix(BodySlam __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		__result = PatchedOnPlay(__instance, choiceContext, cardPlay);
		return false;
	}

	private static async Task PatchedOnPlay(BodySlam __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{

	}
}
 */

[HarmonyPatch(typeof(BodySlam), "OnUpgrade")]
public static class BodySlamOnUpgradePatch
{
	[HarmonyPrefix]
	public static bool Prefix(BodySlam __instance)
	{
		__instance.AddKeyword(CardKeyword.Retain);
		return false;
	}
}