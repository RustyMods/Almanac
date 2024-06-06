using System;
using System.Reflection;

namespace Almanac.API;

public static class ClassesAPI
{
    private static readonly MethodInfo? API_AddExperience;

    public static void AddEXP(int amount)
    {
        API_AddExperience?.Invoke(null, new object[] { amount });
    }


    static ClassesAPI()
    {
        if (Type.GetType("AlmanacClasses.API.API, AlmanacClasses") is not { } api)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Failed to access almanac classes API");
            return;
        }
        
        API_AddExperience = api.GetMethod("AddExperience", BindingFlags.Public | BindingFlags.Static);
    }
}