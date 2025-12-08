namespace Almanac.Bounties;

public partial class BountyManager
{
    private static void LoadDefaults()
    {
        BountyData boar = new BountyData();
        boar.UniqueID = "Boar.001";
        boar.Creature = "Boar";
        boar.Lore = "The meadows have been wicked to the boars, hunted endlessly by ruthless vikings - it breeds true beasts";
        boar.Icon = "TrophyBoar";
        boar.Biome = "Meadows";
        boar.Health = 1000f;
        boar.DamageMultiplier = 1.5f;
        boar.AlmanacTokenReward = 1;
        boar.Level = 3;
        boar.Cost.Add("Coins", 10);
        bounties[boar.UniqueID] = boar;
        
        BountyData neck = new BountyData();
        neck.UniqueID = "Neck.001";
        neck.Creature = "Neck";
        neck.Lore = "The meadows may seem calm, but when the rains fall, the Neck emerges to claim the unwary.";
        neck.Icon = "TrophyNeck";
        neck.Biome = "Meadows";
        neck.Health = 1000f;
        neck.DamageMultiplier = 1.5f;
        neck.AlmanacTokenReward = 1;
        neck.Level = 3;
        neck.Cost.Add("Coins", 10);
        bounties[neck.UniqueID] = neck;

        BountyData troll = new BountyData();
        troll.UniqueID = "Troll.001";
        troll.Creature = "Troll";
        troll.Lore = "Lumbering through the Black Forest, the troll’s steps shake the earth as it smashes all in its path.";
        troll.Icon = "TrophyFrostTroll";
        troll.Biome = "BlackForest";
        troll.Health = 1200f;
        troll.DamageMultiplier = 1.5f;
        troll.AlmanacTokenReward = 5;
        troll.Level = 3;
        troll.Cost.Add("Coins", 10);
        bounties[troll.UniqueID] = troll;

        BountyData serpent = new BountyData();
        serpent.UniqueID = "Serpent.001";
        serpent.Creature = "Serpent";
        serpent.Lore = "Sailors whisper of a serpent that drags ships beneath the waves, leaving only foam and silence.";
        serpent.Icon = "TrophySerpent";
        serpent.Biome = "Ocean";
        serpent.Health = 1000f;
        serpent.DamageMultiplier = 1.5f;
        serpent.AlmanacTokenReward = 5;
        serpent.Level = 3;
        serpent.Cost.Add("Coins", 10);
        bounties[serpent.UniqueID] = serpent;

        BountyData abomination = new BountyData();
        abomination.UniqueID = "Abomination.001";
        abomination.Creature = "Abomination";
        abomination.Lore = "From the mire it rises, a tangle of roots and hate, the swamp itself given monstrous form.";
        abomination.Icon = "TrophyAbomination";
        abomination.Biome = "Swamp";
        abomination.Health = 1600f;
        abomination.DamageMultiplier = 1.5f;
        abomination.AlmanacTokenReward = 5;
        abomination.Level = 3;
        abomination.Cost.Add("Coins", 10);
        bounties[abomination.UniqueID] = abomination;
        
        BountyData wraith = new BountyData();
        wraith.UniqueID = "Wraith.001";
        wraith.Creature = "Wraith";
        wraith.Lore = "When the mists thicken and the air grows cold, the Wraith glides forth to claim the living.";
        wraith.Icon = "TrophyWraith";
        wraith.Biome = "Swamp";
        wraith.Health = 1000f;
        wraith.DamageMultiplier = 1.5f;
        wraith.AlmanacTokenReward = 5;
        wraith.Level = 3;
        wraith.Cost.Add("Coins", 10);
        bounties[wraith.UniqueID] = wraith;
        
        BountyData lox = new BountyData();
        lox.UniqueID = "Lox.001";
        lox.Creature = "Lox";
        lox.Lore = "The ground trembles beneath its hooves, for the Lox knows no predator but death itself.";
        lox.Icon = "TrophyLox";
        lox.Biome = "Plains";
        lox.Health = 2000f;
        lox.DamageMultiplier = 1.5f;
        lox.AlmanacTokenReward = 5;
        lox.Level = 3;
        lox.Cost.Add("Coins", 10);
        bounties[lox.UniqueID] = lox;
        
        BountyData seekerSoldier = new BountyData();
        seekerSoldier.UniqueID = "SeekerSoldier.001";
        seekerSoldier.Creature = "SeekerBrute";
        seekerSoldier.Lore = "Forged in the mists, this armored brute marches with the strength of many men and the hunger of a swarm.";
        seekerSoldier.Icon = "TrophySeekerBrute";
        seekerSoldier.Biome = "Mistlands";
        seekerSoldier.Health = 3000f;
        seekerSoldier.DamageMultiplier = 1.5f;
        seekerSoldier.AlmanacTokenReward = 10;
        seekerSoldier.Level = 3;
        seekerSoldier.Cost.Add("Coins", 10);
        bounties[seekerSoldier.UniqueID] = seekerSoldier;
        
        BountyData fallenValkyrie = new BountyData();
        fallenValkyrie.UniqueID = "FallenValkyrie.001";
        fallenValkyrie.Creature = "FallenValkyrie";
        fallenValkyrie.Lore = "Once a chooser of the slain, now cursed in flame, the Fallen Valkyrie haunts the Ashlands with broken wings.";
        fallenValkyrie.Icon = "TrophyFallenValkyrie";
        fallenValkyrie.Biome = "AshLands";
        fallenValkyrie.Health = 3000f;
        fallenValkyrie.DamageMultiplier = 1.5f;
        fallenValkyrie.AlmanacTokenReward = 20;
        fallenValkyrie.Level = 3;
        fallenValkyrie.Cost.Add("Coins", 10);
        bounties[fallenValkyrie.UniqueID] = fallenValkyrie;
    }
}