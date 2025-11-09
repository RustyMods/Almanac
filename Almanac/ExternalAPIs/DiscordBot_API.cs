using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac.ExternalAPIs;

[PublicAPI]
public static class DiscordBot_API
{
    private static readonly Method? _RegisterCommand;
    private static readonly Method? _SendWebhookMessage;
    private static readonly Method? _SendWebhookTable;

    static DiscordBot_API()
    {
        if (!IsLoaded()) return;
        _RegisterCommand = new Method("RegisterCommand");
        _SendWebhookMessage = new Method("SendWebhookMessage");
        _SendWebhookTable = new Method("SendWebhookTable");
    }
    
    public static bool IsLoaded() => Type.GetType("DiscordBot.API, DiscordBot") != null;

    [PublicAPI]
    public enum Channel { Notifications, Chat, Commands, }
    public static void SendWebhookTable(Channel channel, string title, Dictionary<string, string> tableData) => _SendWebhookTable?.Invoke(channel.ToString(), title, tableData);
    public static void SendWebhookMessage(Channel channel, string message) => _SendWebhookMessage?.Invoke(channel.ToString(), message);
    public static void RegisterCommand(string command, string description, Action<string[]> action, Action<ZPackage>? reaction = null, bool adminOnly = false, bool isSecret = false, string emoji = "")
    {
        _RegisterCommand?.Invoke(command, description, action, reaction, adminOnly, isSecret, emoji);
    }
    internal class Method
    {
        private const string Namespace = "DiscordBot";
        private const string ClassName = "API";
        private const string Assembly = "DiscordBot";
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