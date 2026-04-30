using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(SettlementPlanner.SettlementPlannerMod), "Settlement Planner", "1.0.0", "sagedragoon79")]
[assembly: MelonGame("Crate Entertainment", "Farthest Frontier")]

namespace SettlementPlanner
{
    /// <summary>
    /// Standalone mod that adds a single icon button to the Farthest Frontier
    /// top bar which opens the Farthest Frontier Planner web tool in the
    /// player's default browser. No other features.
    ///
    /// If "Keep Clarity" is also installed (it ships an equivalent button as
    /// part of its broader feature set), this mod detects that at startup and
    /// suppresses its own button so the player only sees one.
    /// </summary>
    public class SettlementPlannerMod : MelonMod
    {
        public const string PlannerUrl = "https://sagedragoon79.github.io/FarthestFrontierPlanner/";
        public const string TooltipText = "Click to open the Farthest Frontier Planner in your browser.";

        internal static MelonLogger.Instance? Log;
        internal static bool DisabledByKeepClarity;
        internal static GameObject? PlannerEntry;

        public override void OnInitializeMelon()
        {
            Log = LoggerInstance;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var n = asm.GetName().Name;
                if (n == "KeepClarity" || n == "FFUIOverhaul")
                {
                    DisabledByKeepClarity = true;
                    break;
                }
            }

            if (DisabledByKeepClarity)
                Log.Msg("Keep Clarity detected — Settlement Planner will not inject a button.");
            else
                Log.Msg("Settlement Planner v1.0.0 initialized");
        }
    }

    internal static class Reflect
    {
        public static object? GetPrivateField(Type type, string name, object instance)
        {
            var f = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return f?.GetValue(instance);
        }
    }

    /// <summary>
    /// Inject the planner button into the top bar when UITopBar.Start completes.
    /// We clone the brick-resource entry (known-good icon path) and reposition
    /// it in the LEFT-side container immediately after the Food entry.
    /// </summary>
    [HarmonyPatch(typeof(UITopBar), "Start")]
    static class PatchUITopBarStart
    {
        public static void Postfix(UITopBar __instance)
        {
            if (SettlementPlannerMod.DisabledByKeepClarity) return;
            if (SettlementPlannerMod.PlannerEntry != null) return;

            try
            {
                var brickText = Reflect.GetPrivateField(typeof(UITopBar), "brickValueText", __instance) as TextMeshProUGUI;
                if (brickText == null) return;
                var brickEntry = brickText.transform.parent.gameObject;

                var foodText = Reflect.GetPrivateField(typeof(UITopBar), "foodValueText", __instance) as TextMeshProUGUI;
                if (foodText == null) return;
                var foodEntry = foodText.transform.parent.gameObject;
                var leftParent = foodEntry.transform.parent;

                var entry = UnityEngine.Object.Instantiate(brickEntry, leftParent);
                entry.name = "SettlementPlanner_Button";
                entry.transform.SetSiblingIndex(foodEntry.transform.GetSiblingIndex() + 1);

                // Use the blueprint sprite — most thematically planner-y. Falls
                // through to paper/book if blueprint isn't in the asset set.
                Sprite? sprite = null;
                foreach (var probe in new[] { "ItemBlueprint", "ItemPaper", "ItemBook" })
                {
                    sprite = GlobalAssets.itemSetupData?.GetItemEntry(probe)?.icon;
                    if (sprite != null) break;
                }
                var iconImage = entry.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                if (iconImage != null && sprite != null) iconImage.sprite = sprite;

                // Icon-only — hide the value text the brick clone came with.
                var valueText = entry.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (valueText != null)
                {
                    valueText.text = "";
                    valueText.gameObject.SetActive(false);
                }

                // Click handler on the entry root (not the icon) so the
                // GenericTooltipDataProvider on the same root still receives
                // hover events. Clicks on the icon bubble up.
                if (iconImage != null) iconImage.raycastTarget = true;
                var btn = entry.GetComponent<Button>() ?? entry.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                if (iconImage != null) btn.targetGraphic = iconImage;
                btn.onClick.AddListener(OnClick);

                SettlementPlannerMod.PlannerEntry = entry;
                RefreshTooltip(entry);
                SettlementPlannerMod.Log?.Msg("Planner button injected into top bar");
            }
            catch (Exception e)
            {
                SettlementPlannerMod.Log?.Warning($"Failed to inject planner button: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Set / re-set the tooltip rows. Called from the Start postfix above
        /// AND from UpdateResourceValues postfix below — the game's per-frame
        /// UI refresh otherwise resets toolTipRowKeyNames to whatever
        /// tooltipRowKeyLocalizationTags resolves to (empty in our case).
        /// </summary>
        public static void RefreshTooltip(GameObject entry)
        {
            var tooltip = entry.GetComponent<GenericTooltipDataProvider>();
            if (tooltip == null) return;

            // No-op if the rows already match — avoids rewriting every frame.
            if (tooltip.toolTipRowKeyNames.Count == 1
                && tooltip.toolTipRowKeyNames[0] == SettlementPlannerMod.TooltipText)
                return;

            var rowKeys = Reflect.GetPrivateField(typeof(GenericTooltipDataProvider), "tooltipRowKeyLocalizationTags", tooltip) as List<string>;
            rowKeys?.Clear();
            tooltip.toolTipRowKeyNames.Clear();
            tooltip.toolTipRowValues.Clear();
            tooltip.AddKeyValue(SettlementPlannerMod.TooltipText, "");
        }

        private static void OnClick()
        {
            SettlementPlannerMod.Log?.Msg($"Opening URL: {SettlementPlannerMod.PlannerUrl}");
            Application.OpenURL(SettlementPlannerMod.PlannerUrl);
        }
    }

    /// <summary>
    /// Re-applies the planner button's tooltip rows after each top-bar UI
    /// refresh (otherwise the game blanks them).
    /// </summary>
    [HarmonyPatch(typeof(UITopBar), "UpdateResourceValues")]
    static class PatchUITopBarUpdate
    {
        public static void Postfix()
        {
            var entry = SettlementPlannerMod.PlannerEntry;
            if (entry == null) return;
            PatchUITopBarStart.RefreshTooltip(entry);
        }
    }
}
