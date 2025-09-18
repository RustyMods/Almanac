using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.Managers;

public static class FontManager
{
    private enum FontOptions
    {
        [InternalName("Norse")] Norse, 
        [InternalName("Norsebold") ]NorseBold, 
        [InternalName("AveriaSerifLibre-Regular")] AveriaSerifLibre,
        [InternalName("AveriaSerifLibre-Bold")] AveriaSerifLibreBold,
        [InternalName("AveriaSerifLibre-Light")] AveriaSerifLibreLight,
        [InternalName("LegacyRuntime")] LegacyRuntime
    }

    private class InternalName : Attribute
    {
        public readonly string internalName;
        public InternalName(string internalName)
        {
            this.internalName = internalName;
        }
    }
    
    private static readonly Dictionary<FontOptions, Font?> m_fonts = new();
    private static readonly List<TextFont> m_allTexts = new();

    private static Font? GetFont(FontOptions option)
    {
        if (m_fonts.TryGetValue(option, out Font? font)) return font;
        Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
        Font? match = fonts.FirstOrDefault(x => x.name == option.GetAttributeOfType<InternalName>().internalName);
        m_fonts[option] = match;
        return match;
    }

    public static void OnFontChange(object sender, EventArgs args)
    {
        var font = GetFont(FontOptions.AveriaSerifLibre);
        foreach (var text in m_allTexts) text.Update(font);
    }

    public static void SetFont(Text[] array)
    {
        foreach (Text text in array)
        {
            _ = new TextFont(text, GetFont(FontOptions.AveriaSerifLibre));
        }
    }

    private class TextFont
    {
        private readonly Text m_text;

        public TextFont(Text text, Font? font)
        {
            m_text = text;
            Update(font);
            m_allTexts.Add(this);
        }

        public void Update(Font? font) => m_text.font = font;
    }
}