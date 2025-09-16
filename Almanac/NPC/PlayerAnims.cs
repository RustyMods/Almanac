using System;
using JetBrains.Annotations;

namespace Almanac.NPC;

[PublicAPI]
public enum PlayerAnims
{
    [AnimType(trigger = "")] None,
    [AnimType(trigger = "inWater", isBool = true)] InWater,
    [AnimType(trigger = "onGround", isBool = true)] OnGround,
    [AnimType(trigger = "blocking", isBool = true)] Blocking,
    [AnimType(trigger = "crouching", isBool = true)] Crouching,
    [AnimType(trigger = "equipping", isBool = true)] Equipping,
    [AnimType(trigger = "encumbered", isBool = true)] Encumbered,
    [AnimType(trigger = "attach_bed", isBool = true)] AttachBed,
    [AnimType(trigger = "attach_throne", isBool = true)] AttachThrone,
    [AnimType(trigger = "attach_sitship", isBool = true)] AttachSitShip,
    [AnimType(trigger = "attach_dragon", isBool = true)] AttachShip,
    [AnimType(trigger = "attach_mast", isBool = true)] AttachMast,
    [AnimType(trigger = "attach_lox", isBool = true)] AttachLox,
    [AnimType(trigger = "attach_asksvin", isBool = true)] AttachAsksvin,
    [AnimType(trigger = "equip_head")] EquipHead,
    [AnimType(trigger = "equip_hip")] EquipHip,
    [AnimType(trigger = "unequip_hip")] UnequipHip,
    [AnimType(trigger = "stagger")] Stagger,
    [AnimType(trigger = "dodge")] Dodge,
    [AnimType(trigger = "eat")] Eat,
    [AnimType(trigger = "interact")] Interact,
    [AnimType(trigger = "gpower")] GPower,
    [AnimType(trigger = "knockdown")] KnockDown,
    [AnimType(trigger = "swing_axe", isChain = true, chainMax = 2)] Axe,
    [AnimType(trigger = "axe_secondary")] AxeSecondary,
    [AnimType(trigger = "swing_pickaxe")] Pickaxe,
    [AnimType(trigger = "swing_longsword", isChain = true, chainMax = 2)] Sword,
    [AnimType(trigger = "sword_secondary")] SwordSecondary,
    [AnimType(trigger = "mace_secondary")] MaceSecondary,
    [AnimType(trigger = "swing_sledge")] Sledge,
    [AnimType(trigger = "swing_hammer")] Hammer,
    [AnimType(trigger = "swing_hoe")] Hoe,
    [AnimType(trigger = "bow_fire", isSequential = true, nextSequence = BowAim)] BowFire,
    [AnimType(trigger = "bow_aim", isBool = true, isSequential = true, nextSequence = BowFire)] BowAim,
    [AnimType(trigger = "atgeir_attack", isChain = true, chainMax = 2)] Atgeir,
    [AnimType(trigger = "atgeir_secondary")] AtgeirSecondary,
    [AnimType(trigger = "battleaxe_attack", isChain = true, chainMax = 2, chainInterval = 2f)] Battleaxe,
    [AnimType(trigger = "battleaxe_secondary")] BattleaxeSecondary,
    [AnimType(trigger = "spear_throw")] ThrowSpear,
    [AnimType(trigger = "spear_poke")] Spear,
    [AnimType(trigger = "unarmed_attack", isChain = true, chainMax = 1)] Unarmed,
    [AnimType(trigger = "kick")] Kick,
    [AnimType(trigger = "throw_bomb")] ThrowBomb,
    [AnimType(trigger = "knife_stab", isChain = true, chainMax = 2)] Knife,
    [AnimType(trigger = "knife_secondary")] KnifeSecondary,
    [AnimType(trigger = "fishingrod_throw", isSequential = true, nextSequence = FishingRod)] FishingRodThrow,
    [AnimType(trigger = "fishingrod_charge", isBool = true, isSequential = true, nextSequence = FishingRodThrow, chargeTime = 1f)] FishingRod,
    [AnimType(trigger = "crossbow_fire", isSequential = true, nextSequence = Crossbow)] CrossbowFire,
    [AnimType(trigger = "reload_crossbow", isBool = true, isSequential = true, nextSequence = CrossbowFire, chargeTime = 3f)] Crossbow,
    [AnimType(trigger = "dual_knives", isChain = true, chainMax = 1)] DualKnives,
    [AnimType(trigger = "dual_knives_secondary")] DualKnivesSecondary,
    [AnimType(trigger = "staff_fireball", isChain = true, chainMax = 1)] StaffFireball,
    [AnimType(trigger = "staff_rapidfire", isBool = true)] StaffRapidFire,
    [AnimType(trigger = "staff_shield")] StaffShield,
    [AnimType(trigger = "staff_summon")] StaffSummon,
    [AnimType(trigger = "staff_charging", isBool = true, isSequential = true, nextSequence = StaffChargeAttack, chargeTime = 1.8f)] StaffCharging,
    [AnimType(trigger = "staff_charge_attack", isSequential = true, nextSequence = StaffCharging)] StaffChargeAttack,
    [AnimType(trigger = "greatsword", isChain = true, chainMax = 2)] Greatsword,
    [AnimType(trigger = "greatsword_secondary")] GreatswordSecondary,
    [AnimType(trigger = "dualaxes", isChain = true, chainMax = 3)] DualAxes,
    [AnimType(trigger = "dualaxes_secondary")] DualAxesSecondary,
    [AnimType(trigger = "recharge_lightningstaff", isBool = true, isSequential = true, nextSequence = StaffLightning, chargeTime = 2f)] RechargeLightningStaff,
    [AnimType(trigger = "staff_lightningshot", isSequential = true, nextSequence = RechargeLightningStaff)] StaffLightning,
    [AnimType(trigger = "staff_trollsummon")] StaffTrollSummon,
    [AnimType(trigger = "place_feast")] PlaceFeast,
    [AnimType(trigger = "scything")] Scything,
    [AnimType(trigger = "crafting", isIndex = true, index = 1)] Work,
    [AnimType(trigger = "crafting", isIndex = true, index = 2)] Forge,
    [AnimType(trigger = "crafting", isIndex = true, index = 3)] Stir,
    [AnimType(trigger = "emote_blowkiss", isEmote = true)] BlowKiss,
    [AnimType(trigger = "emote_bow", isEmote = true)] Bow,
    [AnimType(trigger = "emote_challenge", isEmote = true)] Challenge,
    [AnimType(trigger = "emote_cheer", isEmote = true)] Cheer,
    [AnimType(trigger = "emote_comehere", isEmote = true)] ComeHere,
    [AnimType(trigger = "emote_cower", isEmote = true)] Cower,
    [AnimType(trigger = "emote_cry", isEmote = true)] Cry,
    [AnimType(trigger = "emote_dance", isBool = true, isEmote = true)] Dance,
    [AnimType(trigger = "emote_despair", isEmote = true)] Despair,
    [AnimType(trigger = "emote_drink", isEmote = true)] Drink,
    [AnimType(trigger = "emote_flex", isEmote = true)] Flex,
    [AnimType(trigger = "emote_headbang", isBool = true, isEmote = true)] Headbang,
    [AnimType(trigger = "emote_kneel", isBool = true, isEmote = true)] Kneel,
    [AnimType(trigger = "emote_laugh", isEmote = true)] Laugh,
    [AnimType(trigger = "emote_nonono", isEmote = true)] Nonono,
    [AnimType(trigger = "emote_point", isEmote = true)] Point,
    [AnimType(trigger = "emote_relax", isBool = true, isEmote = true)] Relax,
    [AnimType(trigger = "emote_rest", isBool = true, isEmote = true)] Rest,
    [AnimType(trigger = "emote_roar", isEmote = true)] Roar,
    [AnimType(trigger = "emote_shrug", isEmote = true)] Shrug,
    [AnimType(trigger = "emote_sit", isBool = true, isEmote = true)] Sit,
    [AnimType(trigger = "emote_sitchair", isEmote = true, isBool = true)] SitChair,
    [AnimType(trigger = "emote_stop", isEmote = true)] Stop,
    [AnimType(trigger = "emote_thumbsup", isEmote = true)] ThumbsUp,
    [AnimType(trigger = "emote_toast", isEmote = true)] Toast,
    [AnimType(trigger = "emote_wave", isEmote = true)] Wave,
}

public class AnimType : Attribute
{
    public string trigger = string.Empty;
    public bool isBool;
    public bool isIndex;
    public bool isEmote;
    public bool isChain;
    public bool isSequential;
    public int chainMax;
    public float chainInterval = 1f;
    public float chargeTime = 2f;
    public int index;
    public PlayerAnims nextSequence = PlayerAnims.None;
}

public class AnimChain
{
    public readonly int index;
    public readonly string trigger;

    public AnimChain(string trigger, int index)
    {
        this.trigger = trigger;
        this.index = index;
    }
}