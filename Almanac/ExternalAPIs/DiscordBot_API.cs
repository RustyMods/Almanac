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
    private static bool _isLoaded;

    private static bool isLoaded
    {
        get
        {
            if (_isLoaded) return true;
            _isLoaded = Type.GetType("DiscordBot.API, DiscordBot") != null;
            return _isLoaded;
        }
    }

    public static bool IsLoaded() => isLoaded;

    private static readonly Method _RegisterCommand = new("RegisterCommand");

    /// <summary>
    /// Serializes command into json object to send to discord bot plugin
    /// </summary>
    /// <param name="command">commands saved into a dictionary, must be unique, example: !mycommand</param>
    /// <param name="description">description of command sent to discord when using !help</param>
    /// <param name="action">delegate invoked to run command</param>
    /// <param name="reaction">if action requires to send to peers to run of the client, this is the delegate the client runs</param>
    /// <param name="adminOnly">if true, only discord username registered to config file are allowed to run command</param>
    /// <param name="isSecret">if false, command description is not sent to discord when command !help is called</param>
    /// <param name="emoji">name of emoji to be displayed when description is sent to discord</param>
    [Description("Json serializes command and sends to discord bot plugin using reflection")]
    public static void RegisterCommand(string command, string description, 
        Action<string[]> action, Action<ZPackage>? reaction = null, 
        bool adminOnly = false, bool isSecret = false, string emoji = "")
    {
        _RegisterCommand.Invoke(command, description, action, reaction, adminOnly, isSecret, emoji);
    }
    
    /// <summary>
    /// List of emoji's discord plugin recognizes, send key as value
    /// </summary>
    private static readonly Dictionary<string, string> Emojis = new ()
    {
        { "smile", "😊" }, { "grin", "😁" }, { "laugh", "😂" }, { "wink", "😉" },
        { "wave", "👋" }, { "clap", "👏" }, { "thumbsup", "👍" }, { "thumbsdown", "👎" },
        { "ok", "👌" }, { "pray", "🙏" }, { "muscle", "💪" }, { "facepalm", "🤦" },
        
        { "dog", "🐶" }, { "cat", "🐱" }, { "mouse", "🐭" }, { "fox", "🦊" },
        { "bear", "🐻" }, { "panda", "🐼" }, { "koala", "🐨" }, { "lion", "🦁" },
        { "tiger", "🐯" }, { "monkey", "🐵" }, { "unicorn", "🦄" }, { "dragon", "🐉" },

        { "tree", "🌳" }, { "palm", "🌴" }, { "flower", "🌸" }, { "rose", "🌹" },
        { "sun", "☀️" }, { "moon", "🌙" }, { "star", "⭐" }, { "rain", "🌧️" },
        { "snow", "❄️" }, { "fire", "🔥" }, { "lightning", "⚡" },

        { "pizza", "🍕" }, { "burger", "🍔" }, { "fries", "🍟" }, { "taco", "🌮" },
        { "cake", "🍰" }, { "donut", "🍩" }, { "coffee", "☕" }, { "tea", "🍵" },
        { "beer", "🍺" }, { "wine", "🍷" },

        { "rocket", "🚀" }, { "car", "🚗" }, { "bike", "🚲" }, { "airplane", "✈️" },
        { "train", "🚆" }, { "bus", "🚌" }, { "ship", "🚢" },
        { "book", "📖" }, { "pencil", "✏️" }, { "pen", "🖊️" }, { "paint", "🎨" },
        { "camera", "📷" }, { "phone", "📱" }, { "computer", "💻" },
        { "gift", "🎁" }, { "balloon", "🎈" }, { "key", "🔑" }, { "lock", "🔒" },

        { "soccer", "⚽" }, { "basketball", "🏀" }, { "football", "🏈" }, { "tennis", "🎾" },
        { "golf", "⛳" }, { "run", "🏃" }, { "swim", "🏊" }, { "ski", "⛷️" },
        { "game", "🎮" }, { "music", "🎵" }, { "guitar", "🎸" }, { "drum", "🥁" },

        { "check", "✅" }, { "x", "❌" }, { "warning", "⚠️" }, { "question", "❓" },
        { "exclamation", "❗" }, { "infinity", "♾️" }, { "heart", "❤️" },
        { "brokenheart", "💔" }, { "sparkle", "✨" }, { "starstruck", "🤩" },
        
        { "plus", "✚" }, { "minus", "━" }, { "tornado", "🌪️" }, { "storm", "⛈️" },
        { "save", "💾" }, { "stop", "🔴" } 
    };
    
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