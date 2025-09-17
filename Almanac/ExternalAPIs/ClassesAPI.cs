using System;
using System.Reflection;

namespace Almanac.ExternalAPIs
{
    public static class ClassesAPI
    {
        private static bool isLoaded;
        private static readonly MethodInfo? API_AddExperience;
        public static void AddEXP(int amount)
        {
            API_AddExperience?.Invoke(null, new object[] { amount });
        }
    
        public static bool IsLoaded() => isLoaded;
        static ClassesAPI()
        {
            if (Type.GetType("AlmanacClasses.API.API, AlmanacClasses") is not { } api)
            {
                return;
            }

            isLoaded = true;
            API_AddExperience = api.GetMethod("AddExperience", BindingFlags.Public | BindingFlags.Static);
        }
    }
}