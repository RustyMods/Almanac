using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Managers;

public static class FontManager
{
    public enum FontOptions
    {
        Norse, 
        NorseBold, 
        AveriaSerifLibre,
        AveriaSerifLibreBold,
        AveriaSerifLibreLight,
        LegacyRuntime
    }
    
    private static readonly Dictionary<FontOptions, Font?> m_fonts = new();
    private static readonly List<TextFont> m_allTexts = new();
    
    private static string GetFontName(FontOptions option) => option switch
    {
        FontOptions.Norse => "Norse",
        FontOptions.AveriaSerifLibre => "AveriaSerifLibre-Regular",
        FontOptions.AveriaSerifLibreBold => "AveriaSerifLibre-Bold",
        FontOptions.AveriaSerifLibreLight => "AveriaSerifLibre-Light",
        FontOptions.NorseBold => "Norsebold",
        FontOptions.LegacyRuntime => "LegacyRuntime",
        _ => "AveriaSerifLibre-Regular"
    };

    public static Font? GetFont(FontOptions option)
    {
        if (m_fonts.TryGetValue(option, out Font? font)) return font;
        Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
        var match = fonts.FirstOrDefault(x => x.name == GetFontName(option));
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
            var _ = new TextFont(text, GetFont(FontOptions.AveriaSerifLibre));
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