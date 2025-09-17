using System;

namespace Almanac.NPC;

public static class VikingNameGenerator
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] MaleBaseNames =
    {
        "Ragnar", "Bjorn", "Erik", "Olaf", "Thor", "Leif", "Gunnar", "Ulf",
        "Sven", "Magnus", "Ivar", "Harald", "Knut", "Sigurd", "Finn", "Rolf",
        "Eirik", "Dag", "Nils", "Arne", "Bard", "Einar", "Hakon", "Ingvar",
        "Rollo", "Odin", "Loki", "Balder", "Tyr", "Heimdall", "Vidar", "Vali"
    };

    private static readonly string[] FemaleBaseNames =
    {
        "Astrid", "Freydis", "Gudrun", "Helga", "Ingrid", "Sigrid", "Thora",
        "Brunhild", "Solveig", "Ragnhild", "Asa", "Gunnhild", "Bergthora",
        "Valdis", "Thyra", "Liv", "Signe", "Inga", "Kirsten", "Runa",
        "Freyja", "Frigg", "Sif", "Idun", "Nanna", "Hel", "Skadi", "Eir"
    };

    private static readonly string[] Prefixes =
    {
        "Iron", "Blood", "Storm", "Fire", "Ice", "Stone", "Wolf", "Bear",
        "Raven", "Eagle", "Sea", "Wind", "Thunder", "Frost", "Shadow",
        "Bone", "Flame", "Dark", "Red", "Black", "White", "Gold", "Silver",
        "Steel", "War", "Death", "Wild", "Mad", "Grim", "Bold", "Wise",
        "Ancient", "Cursed", "Savage", "Elder", "Feral", "Forsaken", "Bloodied",
        "Ironclad", "Stormborn", "Vile", "Sacred", "Shadowed", "Brutal", "Dread",
        "Primeval", "Ghostly", "Ashen", "Blighted", "Stonefang", "Venomous", "Frostbound"
    };

    private static readonly string[] Postfixes =
    {
        "the Bold", "the Wise", "the Strong", "the Swift", "the Fierce",
        "the Brave", "the Wild", "the Mad", "the Silent", "the Loud",
        "the Wretched", "the Cunning", "the Eternal", "the Cruel", "the Forgotten",
        "the Unchained", "the Bloodthirsty", "the Hollow", "the Reckoner",
        "the Relentless", "the Broken", "the Unforgiving", "the Stormcaller",
        "the Devourer", "the Watcher", "the Oathbreaker", "the Howler",
        "the Undying", "the Banished", "the Desecrator", "the Hunter",
        "the Defiler", "the Harbinger", "the Reborn", "the Conqueror",
        "Ironside", "Bloodaxe", "Fairhair", "Bluetooth", "Blackbeard",
        "Redbeard", "the Giant", "the Dwarf", "the Unlucky", "the Lucky",
        "Bonecrusher", "Skullsplitter", "Wolfsbane", "Dragonslayer", "Kingslayer"
    };

    private static readonly string[] Suffixes =
    {
        "axe", "sword", "shield", "hammer", "spear", "bow", "blade", "fist",
        "heart", "blood", "tooth", "claw", "eye", "beard", "mane", "hide",
        "bane", "slayer", "killer", "walker", "rider", "hunter", "seeker",
        "breaker", "crusher", "render", "splitter", "cleaver", "born"
    };

    public static string GenerateMaleName()
    {
        string baseName = MaleBaseNames[rng.Next(MaleBaseNames.Length)];
        return GenerateName(baseName);
    }

    public static string GenerateFemaleName()
    {
        string baseName = FemaleBaseNames[rng.Next(FemaleBaseNames.Length)];
        return GenerateName(baseName);
    }

    private static string GenerateName(string baseName)
    {
        double nameType = rng.NextDouble();
        
        if (nameType < 0.3)
        {
            bool usePrefix = rng.NextDouble() < 0.8;
            if (usePrefix)
            {
                string prefix = Prefixes[rng.Next(Prefixes.Length)];
                string suffix = Suffixes[rng.Next(Suffixes.Length)];
                return $"{baseName} {prefix}{suffix}";
            }
            else
            {
                string suffix = Suffixes[rng.Next(Suffixes.Length)];
                return $"{baseName} {baseName}{suffix}";
            }
        }
        if (nameType < 0.7)
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
        string postfix = Postfixes[rng.Next(Postfixes.Length)];
        return $"{baseName} {postfix}";
    }

    public static int GetMaxUniqueNames()
    {
        int baseNames = MaleBaseNames.Length + FemaleBaseNames.Length;
        int compoundNames = baseNames * Prefixes.Length * Suffixes.Length;
        int prefixPostfixNames = baseNames * (1 + Prefixes.Length) * (1 + Postfixes.Length);
        int simplePostfixNames = baseNames * Postfixes.Length;
        return (int)(compoundNames * 0.3 + prefixPostfixNames * 0.4 + simplePostfixNames * 0.3);
    }
}