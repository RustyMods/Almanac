using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.Utilities;

public static class Helpers
{
    public static readonly Color32 OrangeColor = new (255, 164, 0, 255);
    public static readonly Color _OrangeColor = new Color(1f, 0.6431373f, 0f, 1f);

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
}