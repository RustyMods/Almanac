using System.Reflection;
using UnityEngine;

namespace Almanac.Utilities;

public static class SpriteManager
{
    public static readonly Sprite? AlmanacIcon = RegisterSprite("AlmanacIconButton.png");
    private static readonly Sprite? boneSkullIcon = RegisterSprite("bone_skull.png");
    private static readonly Sprite? swordBasicBlueIcon = RegisterSprite("sword_basic_blue.png");
    private static readonly Sprite? swordBasicBrownIcon = RegisterSprite("sword_basic4_blue.png");
    private static readonly Sprite? arrowBasicIcon = RegisterSprite("arrow_basic.png");
    private static readonly Sprite? capeHoodIcon = RegisterSprite("cape_hood_darkyellow.png");
    private static readonly Sprite? bottleStandardEmptyIcon = RegisterSprite("bottle_standard_empty.png");
    private static readonly Sprite? bottleStandardBlueIcon = RegisterSprite("bottle_standard_blue.png");
    private static readonly Sprite? fishGreenIcon = RegisterSprite("fish_green.png");
    private static readonly Sprite? bowWoodIcon = RegisterSprite("bow_wood1.png");
    private static readonly Sprite? necklaceSilverRed = RegisterSprite("necklace_silver_red.png");
    private static readonly Sprite? mushroomBigRedIcon = RegisterSprite("mushroom_big_red.png");
    private static readonly Sprite? goldCoinsPileIcon = RegisterSprite("gold_coins_many.png");
    private static readonly Sprite? keySilverIcon = RegisterSprite("key_silver.png");
    private static readonly Sprite? boneWhiteIcon = RegisterSprite("bone_white.png");
    public static readonly Sprite? bookClosedRedIcon = RegisterSprite("book_closed_red.png");
    private static readonly Sprite? bottleStandardGreenIcon = RegisterSprite("bottle_standard_green.png");
    public static readonly Sprite? crownGoldIcon = RegisterSprite("crown_gold.png");
    private static readonly Sprite? gemDiamondRedIcon = RegisterSprite("gem_diamond_red.png");
    private static readonly Sprite? goldBarsIcon = RegisterSprite("gold_bars_three.png");
    private static readonly Sprite? scrollMapIcon = RegisterSprite("scroll_map2.png");
    private static readonly Sprite? shieldBasicIcon = RegisterSprite("shield_basic_metal.png");
    private static readonly Sprite? silverBarsIcon = RegisterSprite("silver_bars.png");
    private static readonly Sprite? silverCoinsIcon = RegisterSprite("silver_coins_many.png");
    private static readonly Sprite? woodLogIcon = RegisterSprite("wood_log.png");
    private static readonly Sprite? woodLogsIcon = RegisterSprite("wood_logs_three.png");

    public static bool GetSprite(string name, out Sprite? sprite)
    {
        sprite = (name) switch
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
            _ => null
        };

        return sprite;
    }
    private static Sprite? RegisterSprite(string fileName, string folderName = "icons")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{AlmanacPlugin.ModName}.{folderName}.{fileName}";
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        
        return texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
    }
}