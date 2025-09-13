// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Reflection;
// using BepInEx;
// using BepInEx.Bootstrap;
// using HarmonyLib;
// using JetBrains.Annotations;
// using UnityEngine;
//
// namespace Almanac.Data;
//
// public static class ModHelper
// {
//     private static readonly List<string> bundlesToIgnore = new()
//     {
//         "2c2cce25",
//         "86c3d76e",
//         "8d5dbad8",
//         "9fe0899c",
//         "6a33a62",
//         "c4210710",
//         "61c598bb",
//         "ca737529",
//         "b182d691",
//         "fd03cf1f",
//         "9ff2a6c8",
//         "ff4fb6f",
//         "79092c8d",
//         "6f765361",
//         "17245031",
//         "9ac395d7"
//     };
//
//     private static readonly Dictionary<string, AssetInfo> assetToBundle = new();
//     private static readonly Dictionary<string, PluginInfo> bundleToPlugin = new();
//     
//     private static readonly List<Action> pluginSearchQueue = new();
//     private static readonly List<Action> bundleSearchQueue = new();
//
//     public static void MapAssets()
//     {
//         bundleSearchQueue.Add(() =>
//         {
//             foreach (AssetBundle? bundle in Resources.FindObjectsOfTypeAll<AssetBundle>())
//             {
//                 if (bundlesToIgnore.Contains(bundle.name)) continue;
//                 IEnumerable<string> assets = bundle.GetAllAssetNames().Where(asset => asset.EndsWith(".prefab"));
//             
//                 PluginInfo? pluginInfo = bundleToPlugin.TryGetValue(bundle.name, out var info) ? info : null;
//                 foreach (string? asset in assets)
//                 {
//                     if (assetToBundle.ContainsKey(asset)) continue;
//                     string name = asset.Split('/').Last().Replace(".prefab", string.Empty);
//                     assetToBundle[name] = new AssetInfo(name, bundle.name, pluginInfo);
//                 }
//             }
//         });
//         foreach(Action action in pluginSearchQueue) action.Invoke();
//         foreach(Action? action in bundleSearchQueue) action.Invoke();
//         pluginSearchQueue.Clear();
//         bundleSearchQueue.Clear();
//     }
//
//     public static void Setup()
//     {
//         AlmanacPlugin.OnZNetScenePrefabs += prefab =>
//         {
//             
//         };
//     }
//
//     public struct AssetInfo
//     {
//         public readonly string name;
//         public readonly string bundle;
//         public readonly PluginInfo? info;
//
//         public AssetInfo(string name, string bundle, PluginInfo? info = null)
//         {
//             this.name = name;
//             this.bundle = bundle;
//             this.info = info;
//         }
//     }
//     
//     public static bool TryGetAssetInfo(string assetName, out AssetInfo assetInfo) => assetToBundle.TryGetValue(assetName, out assetInfo);
//
//     [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadAsset), typeof(string), typeof(Type))]
//     private static class AssetBundle_LoadAsset_Patch
//     {
//         [UsedImplicitly]    
//         private static void Postfix(AssetBundle __instance, string name)
//         {
//             bundleSearchQueue.Add(() =>
//             {
//                 PluginInfo? pluginInfo = bundleToPlugin.TryGetValue(__instance.name, out var info) ? info : null;
//                 assetToBundle[name] = new AssetInfo(name, __instance.name, pluginInfo);
//             });
//         }
//     }
//     
//     [HarmonyPatch]
//     public static class AssetBundleLoadPatch
//     {
//         [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadFromFile), typeof(string))]
//         [HarmonyPostfix]
//         private static void LoadFromFilePostfix(string path, AssetBundle __result)
//         {
//             TrackBundleLoad(System.IO.Path.GetFileNameWithoutExtension(path), __result);
//         }
//
//         [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadFromMemory), typeof(byte[]))]
//         [HarmonyPostfix]
//         private static void LoadFromMemoryPostfix(byte[] binary, AssetBundle __result)
//         {
//             if (__result != null) TrackBundleLoad(__result.name, __result);
//         }
//
//         [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.LoadFromStream), typeof(System.IO.Stream))]
//         [HarmonyPostfix]
//         private static void LoadFromStreamPostfix(System.IO.Stream stream, AssetBundle __result)
//         {
//             if (__result != null) TrackBundleLoad(__result.name, __result);
//         }
//
//         private static void TrackBundleLoad(string bundleName, AssetBundle bundle)
//         {
//             if (bundle == null || bundlesToIgnore.Contains(bundleName)) return;
//             StackTrace trace = new StackTrace(true);
//             StackFrame[]? frames = trace.GetFrames();
//             if (frames == null || frames.Length == 0) return;
//             pluginSearchQueue.Add(() =>
//             {
//                 PluginInfo? pluginInfo = GetCallingPlugin(frames);
//                 if (pluginInfo != null)
//                 {
//                     bundleToPlugin[bundleName] = pluginInfo;
//                 }
//             });
//         }
//
//
//         private static PluginInfo? GetCallingPlugin(StackFrame[] frames)
//         {
//             Dictionary<Assembly, PluginInfo?> assemblyToPlugin = new();
//             foreach (PluginInfo plugin in Chainloader.PluginInfos.Values)
//             {
//                 try
//                 {
//                     Assembly? pluginAssembly = plugin.Instance?.GetType().Assembly;
//                     if (pluginAssembly != null)
//                     {
//                         assemblyToPlugin[pluginAssembly] = plugin;
//                     }
//
//                     Type? pluginType = plugin.Instance?.GetType();
//                     if (pluginType == null) continue;
//                     AssemblyName[]? referencedAssemblies = pluginAssembly?.GetReferencedAssemblies();
//                     if (referencedAssemblies == null) continue;
//                     foreach (AssemblyName refAssemblyName in referencedAssemblies)
//                     {
//                         try
//                         {
//                             Assembly? refAssembly = Assembly.Load(refAssemblyName);
//                             if (!assemblyToPlugin.ContainsKey(refAssembly) && !IsSystemAssembly(refAssembly))
//                             {
//                                 assemblyToPlugin[refAssembly] = plugin;
//                             }
//                         }
//                         catch
//                         {
//                             // continue
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     AlmanacPlugin.AlmanacLogger.LogDebug($"Error mapping plugin {plugin.Metadata.GUID}: {ex.Message}");
//                 }
//             }
//
//             foreach (StackFrame frame in frames)
//             {
//                 MethodBase? method = frame.GetMethod();
//                 Assembly? assembly = method?.DeclaringType?.Assembly;
//                 if (assembly == null) continue;
//                 if (assembly == typeof(AssetBundleLoadPatch).Assembly) continue;
//                 if (assembly.FullName.Contains("HarmonyLib")) continue;
//                 if (IsSystemAssembly(assembly)) continue;
//                 if (assemblyToPlugin.TryGetValue(assembly, out PluginInfo? plugin) && plugin != null)
//                 {
//                     return plugin;
//                 }
//                 string? assemblyName = assembly.GetName().Name;
//                 string? assemblyLocation = GetAssemblyLocation(assembly);
//                 
//                 foreach (KeyValuePair<string, PluginInfo> kvp in Chainloader.PluginInfos)
//                 {
//                     PluginInfo? pluginInfo = kvp.Value;
//                     try
//                     {
//                         if (assemblyName.Contains(pluginInfo.Metadata.GUID) || assemblyName.Contains(pluginInfo.Metadata.Name.Replace(" ", "")) || pluginInfo.Metadata.GUID.Contains(assemblyName))
//                         {
//                             assemblyToPlugin[assembly] = pluginInfo;
//                             return pluginInfo;
//                         }
//                         if (!string.IsNullOrEmpty(assemblyLocation))
//                         {
//                             string? pluginLocation = GetAssemblyLocation(pluginInfo.Instance?.GetType().Assembly);
//                             if (!string.IsNullOrEmpty(pluginLocation))
//                             {
//                                 string? pluginDir = System.IO.Path.GetDirectoryName(pluginLocation);
//                                 string? assemblyDir = System.IO.Path.GetDirectoryName(assemblyLocation);
//
//                                 if (string.IsNullOrEmpty(pluginDir) || string.IsNullOrEmpty(assemblyDir) || !assemblyDir.StartsWith(pluginDir, StringComparison.OrdinalIgnoreCase)) continue;
//                                 assemblyToPlugin[assembly] = pluginInfo;
//                                 return pluginInfo;
//                             }
//                         }
//                     }
//                     catch { /* Continue searching */ }
//                 }
//             }
//
//             return null;
//         }
//
//         private static string? GetAssemblyLocation(Assembly? assembly)
//         {
//             try
//             {
//                 return assembly?.Location;
//             }
//             catch
//             {
//                 return null;
//             }
//         }
//
//         private static bool IsSystemAssembly(Assembly assembly)
//         {
//             string name = assembly.FullName;
//             return name.StartsWith("System") || 
//                    name.StartsWith("Unity") || 
//                    name.StartsWith("mscorlib") ||
//                    name.StartsWith("netstandard") ||
//                    assembly == typeof(AssetBundle).Assembly;
//         }
//     }
// }