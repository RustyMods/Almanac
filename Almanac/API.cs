using System;
using System.Collections.Generic;
using System.Reflection;
using Almanac.Achievements;
using Almanac.Data;
using Almanac.Store;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac;

[PublicAPI]
public static class API
{
    // Do not copy this section
    public static int GetPlayerDeathCount(string creatureID) => !CritterHelper.namedCritters.TryGetValue(creatureID, out var critter) ? 0 : PlayerInfo.GetPlayerStat(PlayerInfo.RecordType.Death, critter.character.m_name);
    public static int GetPlayerCompletedAchievements(Player player) => player.GetCollectedAchievements().Count;
    public static void AddTokens(Player player, int amount, bool message) => player.AddTokens(amount, message);
    public static void RemoveTokens(Player player, int amount) => player.RemoveTokens(amount);
    public static int GetTokens(Player player) => player.GetTokens();
}

[PublicAPI]
public static class Almanac_API
{
    // Use this section

    private static Method? _GetPlayerDeathCount;
    private static Method? _GetPlayerCompletedAchievements;
    private static Method? _GetTokens;
    private static Method? _AddTokens;
    private static Method? _RemoveTokens;
    static Almanac_API()
    {
        if (!IsLoaded()) return;
        _GetPlayerDeathCount = new Method("GetPlayerDeathCount");
        _GetPlayerCompletedAchievements = new Method("GetPlayerCompletedAchievements");
        _GetTokens = new Method("GetTokens");
        _AddTokens = new Method("AddTokens");
        _RemoveTokens = new Method("RemoveTokens");
    }
    
    public static bool IsLoaded() => Type.GetType("Almanac.API, Almanac") != null;
    
    public static int GetPlayerDeathCount(string creatureID)
    {
        object?[]? result = _GetPlayerDeathCount?.Invoke(creatureID);
        return (int)(result?[0] ?? 0);
    }

    internal class Method
    {
        private const string Namespace = "Almanac";
        private const string ClassName = "API";
        private const string Assembly = "Almanac";
        private const string API_LOCATION = Namespace + "." + ClassName + ", " + Assembly;
        private static readonly Dictionary<string, Type> CachedTypes = new();
        private readonly MethodInfo? info;
        
        public object?[] Invoke(params object?[] args)
        {
            object? result = info?.Invoke(null, args);
            object?[] output = new object?[args.Length + 1];
            output[0] = result;
            Array.Copy(args, 0, output, 1, args.Length);
            return output;
        }
        public Method(string typeNameWithAssembly, string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static)
        {
            if (!TryGetType(typeNameWithAssembly, out Type? type)) return;
            if (type == null)
            {
                Debug.LogWarning($"Type resolution returned null for: '{typeNameWithAssembly}'");
                return;
            }
            info = type.GetMethod(methodName, bindingFlags);
            if (info == null)
            {
                Debug.LogWarning(
                    $"Failed to find public static method '{methodName}' in type '{type.FullName}'. " +
                    "Verify the method name is correct, the method exists, and it is marked as public static. ");
            }
        }
        public Method(string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static) : this(
            API_LOCATION, methodName, bindingFlags)
        {
        }
        private static bool TryGetType(string typeNameWithAssembly, out Type? type)
        {
            if (CachedTypes.TryGetValue(typeNameWithAssembly, out type)) return true;
            if (Type.GetType(typeNameWithAssembly) is not { } resolvedType)
            {
                Debug.LogWarning($"Failed to resolve type: '{typeNameWithAssembly}'. " +
                                 "Verify the namespace, class name, and assembly name are correct. " +
                                 "Ensure the assembly is loaded and accessible.");
                return false;
            }

            type = resolvedType;
            CachedTypes[typeNameWithAssembly] = resolvedType;
            return true;
        }
        public Method(string typeNameWithAssembly, string methodName, params Type[] types)
        {
            if (!TryGetType(typeNameWithAssembly, out Type? type)) return;

            // Additional null check (defensive programming, should not happen if TryGetValue succeeded)
            if (type == null)
            {
                Debug.LogWarning($"Type resolution returned null for: '{typeNameWithAssembly}'");
                return;
            }

            // Locate the static method by name
            info = type.GetMethod(methodName, types);
            if (info == null)
            {
                Debug.LogWarning(
                    $"Failed to find public static method '{methodName}' in type '{type.FullName}'. " +
                    "Verify the method name is correct, the method exists, and it is marked as public static. ");
            }
        }

        public Method(string methodName, params Type[] types) : this(API_LOCATION, methodName, types)
        {
        }
        
        [PublicAPI]
        public ParameterInfo[] GetParameters() => info?.GetParameters() ?? Array.Empty<ParameterInfo>();
        
        [PublicAPI]
        public static void ClearCache() => CachedTypes.Clear();
    }
}