using System;
using System.Reflection;

namespace SimulacrumDronesPlugin
{
    public static class DroneAssetExtensions
    {
        public static string GetAssetString(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DroneAttribute>();
            return attribute?.AssetString ?? string.Empty;
        }
        
        public static int GetSpawnWeight(this Enum value)
        {
            var field = value. GetType().GetField(value. ToString());
            var attribute = field?.GetCustomAttribute<DroneAttribute>();
            return attribute?.DefaultSpawnWeight ?? 0;
        }
    }
}