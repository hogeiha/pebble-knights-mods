using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using PebbleKnights.ModLoader;
using UnityEngine;

namespace UniversalKingActions
{
    public sealed class UniversalKingActionsBehaviour : MonoBehaviour
    {
        private const float ScanInterval = 1.0f;

        private ManualLogSource _logger;
        private Harmony _harmony;
        private Type _upgradeCampType;
        private float _nextScanTime;

        public void Initialize(IModContext context)
        {
            _logger = context.Logger;
            _harmony = new Harmony("pebbleknights.mod.universal_king_actions");
            _harmony.PatchAll(typeof(UniversalKingActionsBehaviour).Assembly);
            _upgradeCampType = TypeFinder.Find("UpgradeCamp");
            _logger.LogInfo("UniversalKingActions loaded. Player pickup, king-style trait upgrades, bot king selection, and camp trait-upgrade hiding are active.");
        }

        private void Update()
        {
            if (_upgradeCampType == null || Time.unscaledTime < _nextScanTime)
                return;

            _nextScanTime = Time.unscaledTime + ScanInterval;
            HideExistingUpgradeCamps();
        }

        private void HideExistingUpgradeCamps()
        {
            try
            {
                var objects = UnityEngine.Object.FindObjectsByType(_upgradeCampType, FindObjectsSortMode.None);
                for (var i = 0; i < objects.Length; i++)
                    UpgradeCampHider.Hide(objects[i]);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to hide camp trait-upgrade entrance: " + ex.Message);
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null)
                _harmony.UnpatchSelf();
        }
    }
}
