using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Almanac.Managers;
using Almanac.NPC;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Almanac.Utilities;

public static class Helpers
{
    internal static ZNetScene? _ZNetScene;
    internal static ObjectDB? _ObjectDB;
    internal static GameObject? GetPrefab(string prefabName)
    {
        if (ZNetScene.instance != null) return ZNetScene.instance.GetPrefab(prefabName);
        if (_ZNetScene == null) return null;
        GameObject? result = _ZNetScene.m_prefabs.Find(prefab => prefab.name == prefabName);
        if (result != null) return result;
        return Clone.registeredPrefabs.TryGetValue(prefabName, out GameObject clone) ? clone : result;
    }
    public static readonly Color32 OrangeColor = new (255, 164, 0, 255);
    public static readonly Color _OrangeColor = new Color(1f, 0.6431373f, 0f, 1f);
    public static void Add<T>(this List<T> list, params T[] values) => list.AddRange(values);
    public static void Remove<T>(this GameObject prefab) where T : Component
    {
        if (prefab.TryGetComponent(out T component)) Object.Destroy(component);
    }

    public static bool HasItem(this Player player, string sharedName, int quality)
    {
        foreach (ItemDrop.ItemData? item in player.GetInventory().GetAllItems())
        {
            if (item.m_shared.m_name == sharedName && item.m_quality == quality) return true;
        }
        return false;
    }
    public static bool IsValidHexColor(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");
    }

    public static string Vector3ToString(Vector3 vector3) => $"{vector3.x:0.00} {vector3.y:0.00} {vector3.z:0.00}";

    public static Vector3 StringToVector3(string value, Vector3 defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        
        if (value == nameof(Color.white)) return Utils.ColorToVec3(Color.white);
        if (value == nameof(Color.yellow)) return Utils.ColorToVec3(Color.yellow);
        if (value == nameof(Color.red)) return Utils.ColorToVec3(Color.red);
        if (value == nameof(Color.green)) return Utils.ColorToVec3(Color.green);
        if (value == nameof(Color.blue)) return Utils.ColorToVec3(Color.blue);
        if (value == nameof(Color.black)) return Utils.ColorToVec3(Color.black);
        if (value == nameof(Color.magenta)) return Utils.ColorToVec3(Color.magenta);
        if (value == nameof(Color.gray)) return Utils.ColorToVec3(Color.gray);
        if (value == "orange") return Utils.ColorToVec3(new Color(0.8f, 0.5f, 0.2f, 1f));
        if (value == "platinum") return new Vector3(0.95f, 0.95f, 0.87f);
        if (value == "blonde") return new Vector3(0.9f, 0.8f, 0.6f);
        if (value == "strawberry") return new Vector3(0.8f, 0.5f, 0.3f);
        if (value == "auburn") return new Vector3(0.6f, 0.3f, 0.1f);
        if (value == "brunette") return new Vector3(0.4f, 0.2f, 0.1f);
        if (value == "chestnut") return new Vector3(0.5f, 0.3f, 0.2f);
        if (value == "chocolate") return new Vector3(0.35f, 0.2f, 0.15f);
        if (value == "espresso") return new Vector3(0.25f, 0.15f, 0.1f);
        if (value == "raven") return new Vector3(0.1f, 0.1f, 0.1f);
        if (value == "ash") return new Vector3(0.6f, 0.6f, 0.55f);
        if (value == "silver") return new Vector3(0.75f, 0.75f, 0.75f);
        if (value == "copper") return new Vector3(0.72f, 0.45f, 0.2f);
        if (value == "porcelain") return new Vector3(0.98f, 0.92f, 0.84f);
        if (value == "fair") return new Vector3(0.95f, 0.87f, 0.73f);
        if (value == "light") return new Vector3(0.92f, 0.8f, 0.65f);
        if (value == "medium") return new Vector3(0.8f, 0.6f, 0.4f);
        if (value == "olive") return new Vector3(0.75f, 0.65f, 0.45f);
        if (value == "tan") return new Vector3(0.7f, 0.55f, 0.35f);
        if (value == "bronze") return new Vector3(0.6f, 0.45f, 0.3f);
        if (value == "dark") return new Vector3(0.4f, 0.3f, 0.2f);
        if (value == "deep") return new Vector3(0.3f, 0.2f, 0.15f);
        if (value == "ebony") return new Vector3(0.2f, 0.15f, 0.1f);
        
        string[] parts = value.Split(' ');
        if (parts.Length != 3) return defaultValue;
        if (!float.TryParse(parts[0].Trim(), out var x)) return defaultValue;
        if (!float.TryParse(parts[1].Trim(), out var y)) return defaultValue;
        if (!float.TryParse(parts[2].Trim(), out var z)) return defaultValue;
        return new Vector3(x, y, z).Clamp01();
    }
    private static Vector3 Clamp01(this Vector3 color) => new (Mathf.Clamp01(color.x), Mathf.Clamp01(color.y), Mathf.Clamp01(color.z));
    public static void AddRange<T, V>(this Dictionary<T, V> dict, Dictionary<T, V> other)
    {
        foreach (KeyValuePair<T, V> kvp in other)
        {
            dict[kvp.Key] = kvp.Value;
        }
    }
    public static string SplitCamelCase(string input)
    {
        string result = Regex.Replace(input, "([A-Z])", " $1");
            
        result = Regex.Replace(result, "([A-Z]+)([A-Z][a-z])", "$1 $2");

        return result.TrimStart();
    }
    
    public static string ReplacePositionTags(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
    
        string pattern = @"<pos=\d+%>";
    
        return Regex.Replace(input, pattern, ", ");
    }
    
    public static bool IsPieceKnown(this Player player, Piece piece)
    {
        foreach (Piece.Requirement? resource in piece.m_resources)
        {
            if (!player.IsMaterialKnown(resource.m_resItem.m_itemData.m_shared.m_name)) return false;
        }
        return true;
    }

    public static void AddOrSet<T, V>(this Dictionary<T, List<V>> dict, T key, V value)
    {
        if (!dict.ContainsKey(key)) dict.Add(key, new List<V>());
        dict[key].Add(value);
    }

    public static void AddOrSet<T, V, K>(this Dictionary<T, Dictionary<V, K>> dict, T Key, V VKey, K value)
    {
        if (!dict.ContainsKey(Key)) dict.Add(Key, new Dictionary<V, K>());
        dict[Key][VKey] = value;
    }
    
    public static void CopySpriteAndMaterial(this GameObject prefab, GameObject source, string childName, string sourceChildName = "")
    {
        Transform to = prefab.transform.Find(childName);
        if (to == null)
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find child {childName} on {prefab.name}");
            return;
        }

        if (!to.TryGetComponent(out Image toImage))
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find image on {to.name}");
            return;
        }
        
        Transform from = string.IsNullOrWhiteSpace(sourceChildName) ? source.transform : source.transform.Find(sourceChildName);
        if (from == null)
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find child {sourceChildName} on {source.name}");
            return;
        }

        if (!from.TryGetComponent(out Image fromImage))
        {
            Debug.LogError($"CopySpriteAndMaterial: couldn't find image on {from.name}");
            return;
        }
        toImage.sprite = fromImage.sprite;
        toImage.material = fromImage.material;
        toImage.color = fromImage.color;
        toImage.type = fromImage.type;
    }
    
    public static void CopyButtonState(this GameObject prefab, GameObject source, string childName, string sourceChildName = "")
    {
        Transform? target = prefab.transform.Find(childName);
        if (target == null)
        {
            Debug.LogError($"CopyButtonState failed to find {childName} on {prefab.name}");
            return;
        }

        if (!target.TryGetComponent(out Button button))
        {
            Debug.LogError($"CopyButtonState failed to find Button component on {target.name}");
            return;
        }

        Transform sourceChild;
        if (!string.IsNullOrWhiteSpace(sourceChildName))
        {
            sourceChild = source.transform.Find(sourceChildName);
            if (sourceChild == null)
            {
                Debug.LogError($"CopyButtonState failed to find {sourceChildName} on {source.name}");
                return;
            }
        }
        else
        {
            sourceChild = source.transform;
        }

        if (!sourceChild.TryGetComponent(out Button sourceButton))
        {
            Debug.LogError($"CopyButtonSprite {sourceChild.name} missing Button component");
            return;
        }
        button.spriteState = sourceButton.spriteState;
    }
    
    public static IEnumerable<List<T>> Batch<T>(this List<T> source, int size)
    {
        for (int i = 0; i < source.Count; i += size)
        {
            int count = Math.Min(size, source.Count - i);
            yield return source.GetRange(i, count);
        }
    }
    
    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.white;
        hex = hex.Replace("#", "");
        if (hex.Length == 6)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }
        if (hex.Length == 8)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }
        return Color.white;
    }

    public static string ColorToHex(Color color, bool includeAlpha = false)
    {
        Color32 c = color;
        return includeAlpha
            ? $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}"
            : $"#{c.r:X2}{c.g:X2}{c.b:X2}";
    }
    
    public static int GetTrailingNumber(string input)
    {
        // Iterate from the end of the string to find where the number starts
        for (int i = input.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(input[i]))
            {
                string numberPart = input.Substring(i + 1);
                if (int.TryParse(numberPart, out int result))
                    return result;
                break;
            }
        }

        return 0;
    }
}