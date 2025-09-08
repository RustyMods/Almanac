using System;

namespace Almanac.Bounties;

public static class NameGenerator
{
    private static readonly Random rng = new Random();
    private static readonly string[] Prefixes =
    {
        "Ancient", "Cursed", "Savage", "Elder", "Feral", "Dark", "Wild",
        "Forsaken", "Bloodied", "Ironclad", "Stormborn", "Grim", "Vile", "Infernal",
        "Sacred", "Shadowed", "Brutal", "Dread", "Primeval", "Ghostly", "Ashen",
        "Blighted", "Stonefang", "Venomous", "Frostbound"
    };
    private static readonly string[] Postfixes =
    {
        "the Wretched", "the Cunning", "the Fierce", "the Eternal", 
        "the Cruel", "the Brave", "the Forgotten",
        "the Unchained", "the Bloodthirsty", "the Hollow", "the Reckoner",
        "the Relentless", "the Broken", "the Unforgiving", "the Stormcaller",
        "the Devourer", "the Watcher", "the Oathbreaker", "the Howler",
        "the Undying", "the Banished", "the Desecrator", "the Hunter",
        "the Defiler", "the Harbinger", "the Reborn", "the Conqueror"
    };
    public static string GenerateName(string baseName)
    {
        bool usePrefix = rng.NextDouble() < 0.5;
        bool usePostfix = rng.NextDouble() < 0.9;

        string name = baseName;

        if (usePrefix)
            name = $"{Prefixes[rng.Next(Prefixes.Length)]} {name}";

        if (usePostfix)
            name = $"{name} {Postfixes[rng.Next(Postfixes.Length)]}";

        return name;
    }
}