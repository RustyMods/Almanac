using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Almanac.Data;
using Almanac.Utilities;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Almanac.Managers;

public static class SpriteManager
{
    public static void OnObjectDBStatusEffects(StatusEffect se)
    {
        icons[se.name] = se.m_icon;
    }
    public static void OnPieceHelperPiece(Piece piece)
    {
        icons[piece.name] = piece.m_icon;
    }
    public static void OnItemHelperItem(ItemDrop item)
    {
        if (!item.m_itemData.HasIcons()) return;
        icons[item.name] = item.m_itemData.GetIcon();
    }
    private static readonly Dictionary<string, Sprite> icons = new();

    public static readonly Sprite AlmanacIcon = RegisterSprite("AlmanacIconButton.png")!;
    private static readonly Sprite boneSkullIcon = RegisterSprite("bone_skull.png")!;
    private static readonly Sprite swordBasicBlueIcon = RegisterSprite("sword_basic_blue.png")!;
    private static readonly Sprite swordBasicBrownIcon = RegisterSprite("sword_basic4_blue.png")!;
    private static readonly Sprite arrowBasicIcon = RegisterSprite("arrow_basic.png")!;
    private static readonly Sprite capeHoodIcon = RegisterSprite("cape_hood_darkyellow.png")!;
    private static readonly Sprite bottleStandardEmptyIcon = RegisterSprite("bottle_standard_empty.png")!;
    private static readonly Sprite bottleStandardBlueIcon = RegisterSprite("bottle_standard_blue.png")!;
    private static readonly Sprite fishGreenIcon = RegisterSprite("fish_green.png")!;
    private static readonly Sprite bowWoodIcon = RegisterSprite("bow_wood1.png")!;
    private static readonly Sprite necklaceSilverRed = RegisterSprite("necklace_silver_red.png")!;
    private static readonly Sprite mushroomBigRedIcon = RegisterSprite("mushroom_big_red.png")!;
    private static readonly Sprite goldCoinsPileIcon = RegisterSprite("gold_coins_many.png")!;
    private static readonly Sprite keySilverIcon = RegisterSprite("key_silver.png")!;
    private static readonly Sprite boneWhiteIcon = RegisterSprite("bone_white.png")!;
    private static readonly Sprite bookClosedRedIcon = RegisterSprite("book_closed_red.png")!;
    private static readonly Sprite bottleStandardGreenIcon = RegisterSprite("bottle_standard_green.png")!;
    private static readonly Sprite crownGoldIcon = RegisterSprite("crown_gold.png")!;
    private static readonly Sprite gemDiamondRedIcon = RegisterSprite("gem_diamond_red.png")!;
    private static readonly Sprite goldBarsIcon = RegisterSprite("gold_bars_three.png")!;
    private static readonly Sprite scrollMapIcon = RegisterSprite("scroll_map2.png")!;
    private static readonly Sprite shieldBasicIcon = RegisterSprite("shield_basic_metal.png")!;
    private static readonly Sprite silverBarsIcon = RegisterSprite("silver_bars.png")!;
    private static readonly Sprite silverCoinsIcon = RegisterSprite("silver_coins_many.png")!;
    private static readonly Sprite woodLogIcon = RegisterSprite("wood_log.png")!;
    private static readonly Sprite woodLogsIcon = RegisterSprite("wood_logs_three.png")!;
    private static readonly Sprite ringGoldMagic = RegisterSprite("ring_gold_magic.png")!;

    public enum IconOption
    {
        Almanac, Skull, SwordBlue, SwordBrown, Arrow, Hood, EmptyBottle, BottleBlue,
        Fish, Bow, NecklaceRed, MushroomRed, GoldCoins, SilverKey, BoneWhite, BookRed, 
        BottleGreen, CrownGold, Gem, GoldBar, Map, Shield, SilverBar, SilverCoins, 
        WoodLog, WoodStack, GoldRing
    }

    public static Sprite? GetSprite(IconOption option)
    {
        return option switch
        {
            IconOption.Almanac => AlmanacIcon,
            IconOption.SwordBlue => swordBasicBlueIcon,
            IconOption.SwordBrown => swordBasicBrownIcon,
            IconOption.Arrow => arrowBasicIcon,
            IconOption.Hood => capeHoodIcon,
            IconOption.Bow => bowWoodIcon,
            IconOption.EmptyBottle => bottleStandardEmptyIcon,
            IconOption.BottleBlue => bottleStandardBlueIcon,
            IconOption.Fish => fishGreenIcon,
            IconOption.Skull => boneSkullIcon,
            IconOption.NecklaceRed => necklaceSilverRed,
            IconOption.MushroomRed => mushroomBigRedIcon,
            IconOption.GoldCoins => goldCoinsPileIcon,
            IconOption.SilverKey => keySilverIcon,
            IconOption.BoneWhite => boneWhiteIcon,
            IconOption.BookRed => bookClosedRedIcon,
            IconOption.BottleGreen => bottleStandardGreenIcon,
            IconOption.CrownGold => crownGoldIcon,
            IconOption.Gem => gemDiamondRedIcon,
            IconOption.GoldBar => goldBarsIcon,
            IconOption.SilverBar => silverBarsIcon,
            IconOption.SilverCoins => silverCoinsIcon,
            IconOption.WoodLog => woodLogIcon,
            IconOption.WoodStack => woodLogsIcon,
            IconOption.Map => scrollMapIcon,
            IconOption.Shield => shieldBasicIcon,
            IconOption.GoldRing => ringGoldMagic,
            _ => GetSprite(option.ToString())
        };
    }
    public static Sprite? GetSprite(string name)
    {
        return name switch
        {
            "skull" => boneSkullIcon,
            "sword_blue" => swordBasicBlueIcon,
            "sword_brown" => swordBasicBrownIcon,
            "arrow" => arrowBasicIcon,
            "hood" => capeHoodIcon,
            "bottle_empty" => bottleStandardEmptyIcon,
            "bottle_blue" => bottleStandardBlueIcon,
            "bottle_green" => bottleStandardGreenIcon,
            "fish" => fishGreenIcon,
            "bow" => bowWoodIcon,
            "necklace" => necklaceSilverRed,
            "mushroom" => mushroomBigRedIcon,
            "coins_gold" => goldCoinsPileIcon,
            "key" => keySilverIcon,
            "bone" => boneWhiteIcon,
            "book" => bookClosedRedIcon,
            "crown" => crownGoldIcon,
            "gem" => gemDiamondRedIcon,
            "gold" => goldBarsIcon,
            "map" => scrollMapIcon,
            "shield" => shieldBasicIcon,
            "silver" => silverBarsIcon,
            "coins_silver" => silverCoinsIcon,
            "log" => woodLogIcon,
            "log_stack" => woodLogsIcon,
            "ring" => ringGoldMagic,
            _ => icons.TryGetValue(name, out var icon) ? icon : null
        };
    }
    private static Sprite? RegisterSprite(string fileName, string folderName = "icons")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{AlmanacPlugin.ModName}.{folderName}.{fileName}";
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        Sprite? sprite = texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
        if (sprite != null) sprite.name = fileName;
        return sprite;
    }

    public static void RegisterCustomIcons()
    {
        string[] files = AlmanacPlugin.IconsDir.GetFiles("*.png");
        foreach (string file in files)
        {
            if (ReadSprite(file) is not {} sprite) continue;
            icons[sprite.name] = sprite;
        }
    }

    private static Sprite? ReadSprite(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var ext = Path.GetExtension(filePath);
        if (!string.Equals(ext, ".png", System.StringComparison.OrdinalIgnoreCase)) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
            bool ok = texture.LoadImage(bytes);
            if (!ok) return null;

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            sprite.name = Path.GetFileNameWithoutExtension(filePath);
            AlmanacPlugin.AlmanacLogger.LogWarning("Successfully registered icon: " + Path.GetFileName(filePath));
            return sprite;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to read custom icon: " + Path.GetFileName(filePath));
            return null;
        }
    }

}