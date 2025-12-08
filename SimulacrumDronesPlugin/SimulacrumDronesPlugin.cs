using BepInEx;
using R2API;

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

        private DroneManager droneManager;

        public void Awake()
        {
            Log.Init(Logger);
            Log.Debug("Simulacrum Drones Plugin Awake called.");
            droneManager = new DroneManager(Config);
            droneManager.RegisterCallbacks();
            droneManager.AddDroneAssets();
        }

        public void OnDestroy()
        {
            Log.Debug("Simulacrum Drones Plugin OnDestroy called.");
            droneManager.UnregisterCallbacks();
        }
    }
}
