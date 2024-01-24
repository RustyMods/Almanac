using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Almanac.UI;
using TMPro;
using UnityEngine;

namespace Almanac.Utilities;

public static class Utility
{
    private static readonly Color32 OrangeColor = new (255, 164, 0, 255);
    public static string ReplaceSpaceWithNewLine(string input) => input.Replace(' ', '\n');
    public static void MergeDictionaries(Dictionary<string, string> destination, Dictionary<string, string> source)
    {
        foreach (KeyValuePair<string, string> kvp in source) destination.Add(kvp.Key, kvp.Value);
    }
    public static string RemoveNumbers(string input) => Regex.Replace(input, @"\d", "");
    public static string SplitCamelCase(string input)
    {
        string result = Regex.Replace(input, "([A-Z])", " $1");
            
        result = Regex.Replace(result, "([A-Z]+)([A-Z][a-z])", "$1 $2");

        return result.TrimStart();
    }
    public static string RemoveParentheses(string input) => Regex.Replace(input, @"\([^)]*\)", "");
    public static TextMeshProUGUI AddTextMeshProGUI(GameObject prefab, bool bold = false, TextWrappingModes wrap = TextWrappingModes.Normal)
    {
        TextMeshProUGUI TMP = prefab.AddComponent<TextMeshProUGUI>();
        TMP.font = bold ? CacheAssets.NorseFontBold : CacheAssets.NorseFont;
        if (bold) TMP.fontMaterial = CacheAssets.TopicTextMeshPro.fontMaterial;
        if (bold) TMP.material = CacheAssets.TopicTextMeshPro.material;
        TMP.fontSize = 14;
        TMP.fontSizeMin = 12;
        TMP.fontSizeMax = 16;
        TMP.autoSizeTextContainer = false;
        TMP.textWrappingMode = wrap;
        TMP.overflowMode = TextOverflowModes.Overflow;
        TMP.verticalAlignment = VerticalAlignmentOptions.Middle;
        TMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
        TMP.color = OrangeColor;
        TMP.richText = true;

        return TMP;
    }
}