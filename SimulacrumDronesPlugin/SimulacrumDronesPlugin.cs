using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SimulacrumDronesPlugin
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(DirectorAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class SimulacrumDronesPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "CW";
        public const string PluginName = "SimulacrumDronesPlugin";
        public const string PluginVersion = "0.0.1";

        private static string[] DroneAssets =
        {
            "RoR2/Base/Drones/iscBrokenDrone1.asset",
            "RoR2/Base/Drones/iscBrokenDrone2.asset",
            "RoR2/Base/Drones/iscBrokenEmergencyDrone.asset",
            "RoR2/Base/Drones/iscBrokenEquipmentDrone.asset",
            "RoR2/Base/Drones/iscBrokenFlameDrone.asset",
            "RoR2/Base/Drones/iscBrokenMegaDrone.asset",
            "RoR2/Base/Drones/iscBrokenMissileDrone.asset"
        };

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            // Log.Info("Simulacrum Drones Plugin Awake");
            addDrones();
            // Log.Info("Subscribing to DirectorAPI InteractableActions event.");
            // DirectorAPI.InteractableActions += OnInteractableActions;
            Log.Info("Subscribing to DirectorAPI StageSettingsActions event.");
            DirectorAPI.StageSettingsActions += OnStageSettings;
            Log.Info("Subscribing to HealthComponent TakeDamage event.");
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public void OnDestroy()
        {
            Log.Info("Simulacrum Drones Plugin OnDestroy called.");
            // Log.Info("Unsubscribing from DirectorAPI InteractableActions event.");
            // DirectorAPI.InteractableActions -= OnInteractableActions;
            Log.Info("Unsubscribing from DirectorAPI StageSettingsActions event.");
            DirectorAPI.StageSettingsActions -= OnStageSettings;
            Log.Info("Unsubscribing from HealthComponent TakeDamage event.");
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }

        // private void OnInteractableActions(DccsPool pool, DirectorAPI.StageInfo stageInfo)
        // {
        //     if (Run.instance is InfiniteTowerRun)
        //     {
        //         Log.Info("OnInteractableActions called in Infinite Tower Run.");
        //         addDrones();
        //     }
        // }

        private void OnStageSettings(DirectorAPI.StageSettings stageSettings, DirectorAPI.StageInfo stageInfo)
        {
            if (Run.instance is InfiniteTowerRun)
            {
                Log.Info("OnStageSettings called in Infinite Tower Run.");
                setDroneSpawnRates(stageSettings, 100f);
            }
            else 
            {
                setDroneSpawnRates(stageSettings, 0f);
            }
        }

        private void addDrones()
        {
            Log.Info("Add drones method called!");

            foreach (var droneAsset in DroneAssets)
            {
                Log.Info($"Adding drone: {droneAsset}");
                var droneSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>(droneAsset). WaitForCompletion();
                droneSpawnCard.directorCreditCost = 1;

                var droneDirectorCard = new DirectorCard
                {
                    spawnCard = droneSpawnCard,
                    selectionWeight = 500
                };
                Log.Info($"Setting drone: {droneAsset} to selectionWeight: {droneDirectorCard.selectionWeight}");
                var droneDirectorCardHolder  = new DirectorAPI.DirectorCardHolder
                {
                    Card = droneDirectorCard,
                    InteractableCategory = DirectorAPI.InteractableCategory.Custom,
                    CustomInteractableCategory = "SimulacrumDrones"

                };
                DirectorAPI.Helpers.AddNewInteractable(droneDirectorCardHolder);
                // DirectorAPIhelpers.AddNewInteractable(droneCardHolder);
                Log.Info($"Drone successfully added: {droneAsset}");
            }
        }

        private void setDroneSpawnRates(DirectorAPI.StageSettings stageSettings, float newWeight)
        {
             foreach (var dccsEntry in stageSettings.InteractableCategoryWeightsPerDccs)
            {
                var weights = dccsEntry.Value;   
                if (weights.ContainsKey("SimulacrumDrones"))
                {
                    weights["SimulacrumDrones"] = newWeight;
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, 
        HealthComponent self, DamageInfo damageInfo)
        {
            // Check if this is void fog damage and the target is a drone
            if (Run.instance is InfiniteTowerRun && damageInfo != null && self.body != null)
            {
                // Check if damage is from void fog (VoidFog damage type)
                bool isVoidFogDamage = IsVoidFogDamage(damageInfo);
                
                // Check if the body is a drone (you can customize this check)
                bool isDrone = IsDrone(self.body);
                
                if (isVoidFogDamage && isDrone)
                {
                    
                    // Cancel the damage by returning early
                    return;
                }
            }
            
            orig(self, damageInfo);
        }

        private bool IsDrone(CharacterBody body)
        {
            // Check body name for common drone identifiers
            string bodyName = body.name. ToLower();
            return bodyName.Contains("drone")
                || body.bodyFlags.HasFlag(CharacterBody. BodyFlags.Mechanical);
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
