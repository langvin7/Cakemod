using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Logging;
using System;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace cakemod;

[HarmonyPatch(typeof(NCardGrid), "OnHolderPressed")]
public class NCardGridOnHolderPressedPatch
{
    public static event Action<NCardGrid, NCardHolder>? OnCardHolderPressed;

    static bool Prefix(NCardGrid __instance, NCardHolder holder)
    {
        try
        {
            OnCardHolderPressed?.Invoke(__instance, holder);

            if (holder.CardModel is Conflagration && holder.CardModel.Pile?.Type == PileType.Draw)
            {
                _ = AutoPlayHelper.TryAutoPlay(holder.CardModel.Owner, holder.CardModel, 1);
                return false; // 跳过原方法，防止与AutoPlay冲突导致卡死
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[CakeMod] Error in NCardGrid OnHolderPressed hook: {ex}");
        }
        return true;
    }
}

[HarmonyPatch(typeof(NCardGrid), "OnHolderAltPressed")]
public class NCardGridOnHolderAltPressedPatch
{
    public static event Action<NCardGrid, NCardHolder>? OnCardHolderAltPressed;

    static void Prefix(NCardGrid __instance, NCardHolder holder)
    {
        try
        {
            OnCardHolderAltPressed?.Invoke(__instance, holder);
        }
        catch (Exception ex)
        {
            Log.Error($"[CakeMod] Error in NCardGrid OnHolderAltPressed hook: {ex}");
        }
    }
}
