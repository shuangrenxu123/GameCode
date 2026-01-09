using System;
using System.Reflection;
using Unity.GraphToolkit.Editor;

namespace BT.Editor.Internal
{
    static class GraphToolkitPortCapacityUtil
    {
        const string k_SingleCapacityName = "Single";

        public static bool TryForceSingleCapacity(IPort port)
        {
            if (port == null)
                return false;

            try
            {
                var capacityProperty = port.GetType().GetProperty(
                    "Capacity",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (capacityProperty == null || !capacityProperty.CanWrite)
                    return false;

                var capacityType = capacityProperty.PropertyType;
                if (!capacityType.IsEnum)
                    return false;

                var singleValue = Enum.Parse(capacityType, k_SingleCapacityName, ignoreCase: true);
                capacityProperty.SetValue(port, singleValue);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
