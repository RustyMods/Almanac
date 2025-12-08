using System;
using System.Reflection;

namespace Almanac.ExternalAPIs;

public static class RustyBagsAPI
{
    private const string Namespace = "RustyBags";
    private const string ClassName = "API";
    private const string Assembly = " RustyBags";
    
    private static readonly bool isLoaded = false;
    public static bool IsLoaded() => isLoaded;

    private static readonly MethodInfo? API_IsBag;
    private static readonly MethodInfo? API_IsQuiver;
    
    static RustyBagsAPI()
    {
        if (Type.GetType($"{Namespace}.{ClassName}, {Assembly}") is not { } api) return;
        isLoaded = true;

        API_IsBag = api.GetMethod("IsBag", BindingFlags.Public | BindingFlags.Static);
        API_IsQuiver = api.GetMethod("IsQuiver", BindingFlags.Public | BindingFlags.Static);
    }

    public static bool IsBag(this ItemDrop.ItemData item) => IsBag(item.m_shared.m_name);
    public static bool IsQuiver(this ItemDrop.ItemData item) => IsQuiver(item.m_shared.m_name);

    public static bool IsBag(string sharedName) =>
        (bool)(API_IsBag?.Invoke(null, new object[] { sharedName }) ?? false);

    public static bool IsQuiver(string sharedName) =>
        (bool)(API_IsQuiver?.Invoke(null, new object[] { sharedName }) ?? false);
}