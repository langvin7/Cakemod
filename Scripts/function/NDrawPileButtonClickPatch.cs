using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using System;

namespace cakemod;

/// <summary>
/// Harmony patch for NCombatCardPile (includes NDrawPileButton, NDiscardPileButton, etc.)
/// Hooks into the OnRelease event to allow custom handling when pile buttons are clicked
/// </summary>
[HarmonyPatch(typeof(NCombatCardPile), "OnRelease")]
public class NCombatCardPileClickPatch
{
    /// <summary>
    /// Hook that fires when a combat card pile button is clicked
    /// Provides the pile button instance and the pile type being clicked
    /// </summary>
    public static event Action<NCombatCardPile, PileType>? OnPileButtonClicked;

    static void Prefix(NCombatCardPile __instance)
    {
        try
        {
            // Get the pile type using reflection since Pile property is protected
            var pileProperty = typeof(NCombatCardPile).GetProperty("Pile", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
            
            if (pileProperty != null)
            {
                PileType pileType = (PileType)pileProperty.GetValue(__instance);
                
                // Invoke the hook to allow mod code to handle the click
                OnPileButtonClicked?.Invoke(__instance, pileType);
                
                // Log for debugging purposes
                // Log.Debug($"[CakeMod] Pile button clicked: {pileType}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[CakeMod] Error in NCombatCardPile OnRelease hook: {ex}");
        }
    }
}

/// <summary>
/// Specific patch for NDrawPileButton to provide a more specific hook
/// </summary>
[HarmonyPatch(typeof(NDrawPileButton), "_Ready")]
public class NDrawPileButtonReadyPatch
{
    /// <summary>
    /// Hook that fires when the draw pile button is ready
    /// </summary>
    public static event Action<NDrawPileButton>? OnDrawPileButtonReady;

    static void Postfix(NDrawPileButton __instance)
    {
        try
        {
            // Invoke the hook
            OnDrawPileButtonReady?.Invoke(__instance);
            
            // Log for debugging purposes
            // Log.Debug($"[CakeMod] Draw pile button is ready");
        }
        catch (Exception ex)
        {
            Log.Error($"[CakeMod] Error in NDrawPileButton _Ready hook: {ex}");
        }
    }
}
