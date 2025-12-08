using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;

namespace SimulacrumDronesPlugin
{
    public class DroneManager
    {
        private const string configSection = "DroneSettings";
        private const string defaultDroneCategoryName = "SimulacrumDrones";
        private const float defaultDroneCategorySpawnRate = 14f;
        private const int defaultAdditonalCreditPerStage = 20;


        private ConfigEntry<string> droneCategoryName;
        private ConfigEntry<float> droneCategorySpawnRate;
        private ConfigEntry<int> additionalCreditsPerStage;

        private Dictionary<DroneAssetEnum, ConfigEntry<int>> droneSpawnWeights = 
            new Dictionary<DroneAssetEnum, ConfigEntry<int>>();

        public DroneManager(ConfigFile config)
        {
            droneCategoryName = config.Bind(configSection,
                     "CategoryName", defaultDroneCategoryName, 
                     "Name of the drone category for spawning.");
            droneCategorySpawnRate = config.Bind(configSection,
                     "CategorySpawnRate", defaultDroneCategorySpawnRate, 
                     "Spawn rate for the drone category.");
            additionalCreditsPerStage = config.Bind(configSection,
                     "AdditionalCreditsPerStage", defaultAdditonalCreditPerStage, 
                     "Additional credits added to dirctor iteractable spawns per stage.");

            foreach (DroneAssetEnum drone in Enum.GetValues(typeof(DroneAssetEnum)))
            {
                ConfigEntry<int> spawnWeight = config.Bind(configSection,
                    $"{drone}_SpawnWeight",
                    drone.GetSpawnWeight(),
                    $"Spawn weight for {drone} drone.");
                droneSpawnWeights[drone] = spawnWeight;
            }
        }

        public void RegisterCallbacks()
        {
            Log.Info("DroneManager - Subscribing to callbacks.");
            DirectorAPI.StageSettingsActions += OnStageSettings;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            Log.Info("DroneManager - Successfully subscribed to callbacks.");
        }

        public void UnregisterCallbacks()
        {
            Log.Info("DroneManager - Unsubscribing from callbacks.");
            DirectorAPI.StageSettingsActions -= OnStageSettings;
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
            Log.Info("DroneManager - Successfully unsubscribed from callbacks.");
        }

        public void AddDroneAssets()
        {
            Log.Info("DroneManager - Add drone assets method called!");
            foreach (DroneAssetEnum droneAsset in Enum.GetValues(typeof(DroneAssetEnum)))
            {
                if (!droneSpawnWeights.ContainsKey(droneAsset))
                {
                    Log.Warning($"Spawn weight config not found for asset: {droneAsset}, skipping.");
                    continue;
                }
                Log.Info($"DroneManager - Adding asset: {droneAsset}");
                var droneSpawnCard = 
                    Addressables.LoadAssetAsync<InteractableSpawnCard>(droneAsset.GetAssetString())
                        .WaitForCompletion();

                var droneDirectorCard = new DirectorCard
                {
                    spawnCard = droneSpawnCard,
                    selectionWeight = droneSpawnWeights[droneAsset].Value
                };
                Log.Info($"DroneManager - Setting asset: {droneAsset} to selectionWeight: {droneDirectorCard.selectionWeight}");
                var droneDirectorCardHolder  = new DirectorAPI.DirectorCardHolder
                {
                    Card = droneDirectorCard,
                    InteractableCategory = DirectorAPI.InteractableCategory.Custom,
                    CustomInteractableCategory = droneCategoryName.Value

                };
                DirectorAPI.Helpers.AddNewInteractable(droneDirectorCardHolder);
                Log.Info($"DroneManager - Successfully added asset: {droneAsset}");
            }
            Log.Info("DroneManager - Finished adding drone assets.");
        }

        private void OnStageSettings(DirectorAPI.StageSettings stageSettings, DirectorAPI.StageInfo stageInfo)
        {
            if (Run.instance is InfiniteTowerRun)
            {
                stageSettings.SceneDirectorInteractableCredits += additionalCreditsPerStage.Value;
                SetCategorySpawnRates(stageSettings, droneCategorySpawnRate.Value);
            }
            else 
            {
                // Disable simulacrum drone spawns in other modes
                SetCategorySpawnRates(stageSettings, 0f);
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, 
            HealthComponent self, DamageInfo damageInfo)
        {
            // Verify we are in Simulacrum mode as drones should only be immune there.
            if (Run.instance is InfiniteTowerRun && damageInfo != null && self.body != null)
            {
                bool isVoidFogDamage = IsVoidFogDamage(damageInfo);
                bool isDrone = IsDrone(self.body);
                if (isVoidFogDamage && isDrone)
                {
                    return;
                }
            }
            orig(self, damageInfo);
        }

        private void SetCategorySpawnRates(DirectorAPI.StageSettings stageSettings, float newWeight)
        {
            foreach (var dccsEntry in stageSettings.InteractableCategoryWeightsPerDccs)
            {
                var weights = dccsEntry.Value;   
                if (weights.ContainsKey(droneCategoryName.Value))
                {
                    weights[droneCategoryName.Value] = newWeight;
                }
            }
        }

        private bool IsDrone(CharacterBody body)
        {
            string bodyName = body.name.ToLower();
            return bodyName.Contains("drone")
                && body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical);
        }

        private bool IsVoidFogDamage(DamageInfo damageInfo)
        {
            var damageType = damageInfo.damageType;
            DamageType requiredFlags = DamageType.BypassArmor | DamageType. BypassBlock;
            return damageInfo.inflictor == null 
                && damageInfo.attacker == null 
                && damageInfo.damageColorIndex == DamageColorIndex.Void
                && damageType.damageTypeExtended == DamageTypeExtended.DamageField
                && damageType.damageSource == DamageSource.Hazard
                && (damageType.damageType & requiredFlags) == requiredFlags;
        }
    }
}