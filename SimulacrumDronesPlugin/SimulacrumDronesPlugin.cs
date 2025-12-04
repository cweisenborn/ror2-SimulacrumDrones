using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SimulacrumDronesPlugin
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(DirectorAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class SimulacrumDronesPlugin : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
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
            // addDrones();
            Log.Info("Subscribing to DirectorAPI InteractableActions event.");
            DirectorAPI.InteractableActions += OnInteractableActions;
            Log.Info("Subscribing to DirectorAPI StageSettingsActions event.");
            DirectorAPI.StageSettingsActions += OnStageSettings;
            Log.Info("Subscribing to HealthComponent TakeDamage event.");
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        public void OnDestroy()
        {
            Log.Info("Simulacrum Drones Plugin OnDestroy called.");
            Log.Info("Unsubscribing from DirectorAPI InteractableActions event.");
            DirectorAPI.InteractableActions -= OnInteractableActions;
            Log.Info("Unsubscribing from DirectorAPI StageSettingsActions event.");
            DirectorAPI.StageSettingsActions -= OnStageSettings;
            Log.Info("Unsubscribing from HealthComponent TakeDamage event.");
            On.RoR2.HealthComponent.TakeDamage -= HealthComponent_TakeDamage;
        }

        private void OnInteractableActions(DccsPool pool, DirectorAPI.StageInfo stageInfo)
        {
            if (Run.instance is InfiniteTowerRun)
            {
                Log.Info("OnInteractableActions called in Infinite Tower Run.");
                addDrones();
            }
        }

        private void OnStageSettings(DirectorAPI.StageSettings stageSettings, DirectorAPI.StageInfo stageInfo)
        {
            if (Run.instance is InfiniteTowerRun)
            {
                Log.Info("OnStageSettings called in Infinite Tower Run.");
                // addDrones();

                // Access the weights for each DirectorCardCategorySelection
                foreach (var dccsEntry in stageSettings.InteractableCategoryWeightsPerDccs)
                {
                    var dccs = dccsEntry.Key;
                    var weights = dccsEntry.Value;
                    
                    // Modify the weight of a specific category
                    if (weights.ContainsKey("Drones"))
                    {
                        Log.Info("Settings Drones to a higher spawn rate.");
                        weights["Drones"] = 100f;  // Make chests 5x more likely to be selected
                    }
                }
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
                    InteractableCategory = DirectorAPI.InteractableCategory.Custom
                    
                };
                DirectorAPI.Helpers.AddNewInteractable(droneDirectorCardHolder);
                // DirectorAPIhelpers.AddNewInteractable(droneCardHolder);
                Log.Info($"Drone successfully added: {droneAsset}");
            }

            // DirectorAPI.StageSettingsActions += (stageSettings, stageInfo) =>
            // {
            //     // Access the weights for each DirectorCardCategorySelection
            //     foreach (var dccsEntry in stageSettings.InteractableCategoryWeightsPerDccs)
            //     {
            //         var dccs = dccsEntry.Key;
            //         var weights = dccsEntry.Value;
                    
            //         // Modify the weight of a specific category
            //         if (weights.ContainsKey("Drones"))
            //         {
            //             Log.Info("Settings Drones to a higher spawn rate.");
            //             weights["Drones"] = 100f;  // Make chests 5x more likely to be selected
            //         }
            //     }
            // };
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
