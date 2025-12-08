using System;

namespace SimulacrumDronesPlugin
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DroneAttribute : Attribute
    {
        public string AssetString { get; set; }
        public int DefaultSpawnWeight { get; set; }
        
        public DroneAttribute(string assetString, int spawnWeight)
        {
            AssetString = assetString;
            DefaultSpawnWeight = spawnWeight;
        }
    }
}