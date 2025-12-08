# Almanac

Welcome to Rusty's Almanac, your gateway to the enchanting world of Valheim!

Rusty's comprehensive plugin catalogs all the items, pieces, and creatures loaded into the game. Explore Valheim with ease, armed with a wealth of information at your fingertips.

Rusty's Almanac also includes a customizable achievement system and a leaderboard, letting players shape their own unique experiences.


# Achievements

Almanac lets you define custom achievements using .yml files. Achievements sync between server and client and can be edited while the server is running.

Below are the available Achievement Types:

• *Arrows*  
• *ArrowsShot*  
• *Axes*  
• *Bows*  
• *Consumables*  
• *CreatureGroup*  
• *CreatureTamed*  
• *Deaths*  
• *DistanceInAir*  
• *DistanceRan*  
• *DistanceSailed*  
• *DistanceWalked*  
• *EnemyKills*  
• *Fish*  
• *FoodEaten*  
• *ItemsPicked*  
• *Kill*  
• *Knives*  
• *Maces*  
• *Materials*  
• *MineHits*  
• *Pickable*  
• *PlayerKills*  
• *PoleArms*  
• *Potions*  
• *Recipes*  
• *Shields*  
• *Spears*  
• *Staves*  
• *Swords*  
• *TimeInBase*  
• *TimeOutOfBase*  
• *TotalJumps*  
• *TotalMined*  
• *Trophies*  
• *Valuables*  
• *Weapons*  
• *CollectItems*  
• *Achievements*

### Achievement File Structure

Each achievement can define:

• *UniqueID*: A unique identifier  
• *Name*: Display name  
• *Lore*: Description text  
• *Icon*: Icon name from assets  
• *TokenReward*: Tokens granted  
• *Requirement*: Type, threshold, optional group or prefab name

### Examples

• Defeating creatures in a biome: use *AchievementType.CreatureGroup* with *Group = Meadows*  
• For kill or distance achievements, set the Requirement Type to the desired stat  
• Use collection-based types for fish, weapons, trophies, and more

### Tips

• Achievements can be edited while server is running  
• Server syncs entries to clients  
• Threshold 0 enables auto-detection (e.g., number of fish)  
• *Kill* and *Pickable* require PrefabName  
• *CollectItems* requires entries like: *prefabname,amount;prefabname,amount*


# Almanac Bounties

Players may purchase bounty contracts for special creatures. Bounties are defined in .yml files and sync dynamically.

Each bounty defines:

• *UniqueID*  
• *Creature*  
• *Name*  
• *Icon*  
• *Biome*  
• *Health*  
• *Level*  
• *DamageMultiplier*  
• *AlmanacTokenReward*  
• *Lore*  
• *Cost*

### Notes

• Costs may use *AlmanacToken* or regular items  
• Only direct kills count  
• Costs are refunded if bounty despawns or dies incorrectly  
• Bounties have cooldowns  
• Runtime edits sync automatically

### Default Bounties

Boar, Neck, Troll, Serpent, Abomination, Wraith, Lox, Seeker Brute, Fallen Valkyrie


# Almanac Store

Store items are defined in .yml files and sync dynamically.

Each store entry may define:

• *Name*  
• *Lore*  
• *Icon*  
• *Cost*  
• *StatusEffect*  
• *Items*  
• *RequiredKey*

### Notes

• Default currency is *AlmanacToken*  
• Costs may also use items  
• Status effects must exist in ObjectDB  
• Items require valid prefab names  
• RequiredKey checks if player has ever killed a creature

### Tips

• Store updates automatically when reopened  
• You can edit files during gameplay


# Custom Status Effects

You can create new status effects with the Almanac Custom Status Effects Manager.

Available modifier types include:

• *Armor*  
• *AttackStaminaModifier*  
• *BlockStaminaModifier*  
• *BluntDamage*  
• *BluntResistance*  
• *CarryWeight*  
• *ChopDamage*  
• *ChopResistance*  
• *DamageModifier*  
• *DamageReduction*  
• *DodgeStaminaModifier*  
• *Eitr*  
• *EitrRegenModifier*  
• *FallDamageModifier*  
• *FireDamage*  
• *FireResistance*  
• *FrostDamage*  
• *FrostResistance*  
• *Health*  
• *HealthRegenModifier*  
• *HomeItemStaminaModifier*  
• *JumpStaminaModifier*  
• *LifeSteal*  
• *LightningDamage*  
• *LightningResistance*  
• *MaxFallSpeed*  
• *NoiseModifier*  
• *PickaxeDamage*  
• *PickaxeResistance*  
• *PierceDamage*  
• *PierceResistance*  
• *PoisonDamage*  
• *PoisonResistance*  
• *RaiseSkills*  
• *RunStaminaModifier*  
• *SlashDamage*  
• *SlashResistance*  
• *SneakStaminaModifier*  
• *Speed*  
• *SpiritDamage*  
• *SpiritResistance*  
• *Stamina*  
• *StaminaRegenModifier*  
• *SwimStaminaModifier*  
• *WindMovementModifier*  
• *WindRunStaminaModifier*

### Notes

• Some values are additive; others are multipliers  
• Only the host can add effects  
• Files sync to clients  
• Editor tool included in-game


# Treasure Hunts

Treasure hunts are defined in .yml files and sync automatically. Hunts create map pins and generate loot chests.

### Core Properties

• *Name*  
• *Lore*  
• *Icon*  
• *Biome*  
• *Cost*  
• *Loot*

### Loot Entries

Each loot definition includes:

• *Item*  
• *Min*  
• *Max*  
• *Weight*


# Dialogue System

Dialogues are defined in .yml files and sync dynamically.

### Command Types

• *Exit*  
• *Give*  
• *Take*  
• *Teleport*  
• *FlyTo*  
• *MapPin*  
• *StartBounty*  
• *CancelBounty*  
• *CompleteBounty*  
• *StartTreasure*  
• *CancelTreasure*  
• *OpenAlmanac*  
• *OpenItems*  
• *OpenPieces*  
• *OpenCreatures*  
• *OpenAchievements*  
• *OpenStore*  
• *OpenLeaderboard*  
• *OpenBounties*  
• *OpenTreasures*  
• *OpenMetrics*  
• *OpenLottery*

### Dialogue Structure

• *UniqueID*  
• *Label*  
• *Text*  
• *AltText*  
• *Dialogues*  
• *Action*  
• *Requirements*

### Requirement Types

• *Keys*  
• *NotKeys*  
• *Killed*  
• *NotKilled*  
• *Achievements*  
• *NotAchievements*  
• *Dialogues*  
• *NotDialogues*  
• *Quests*  
• *NotQuests*  
• *CompletedQuests*  
• *NotCompletedQuests*

### Notes

• Alternate text appears when requirements are unmet  
• Commands prevent duplicate actions  
• Requirements are evaluated live


# NPC System

NPCs can be configured using admin mode and no-cost mode.

## Random Talk

NPCs can speak when players approach, leave, or periodically.

## NPC Animations

Available animations include:

• *Atgeir*  
• *AtgeirSecondary*  
• *AttachAsksvin*  
• *AttachLox*  
• *AttachMast*  
• *AttachShip*  
• *AttachSitShip*  
• *AttachThrone*  
• *Axe*  
• *AxeSecondary*  
• *Battleaxe*  
• *BattleaxeSecondary*  
• *BlowKiss*  
• *Blocking*  
• *Bow*  
• *BowAim*  
• *BowFire*  
• *Challenge*  
• *Cheer*  
• *ComeHere*  
• *Cower*  
• *Crouching*  
• *Crossbow*  
• *CrossbowFire*  
• *Cry*  
• *Dance*  
• *Despair*  
• *Dodge*  
• *Drink*  
• *DualAxes*  
• *DualAxesSecondary*  
• *DualKnives*  
• *DualKnivesSecondary*  
• *Eat*  
• *Encumbered*  
• *EquipHead*  
• *EquipHip*  
• *Equipping*  
• *Flex*  
• *FishingRod*  
• *FishingRodThrow*  
• *Forge*  
• *GPower*  
• *Greatsword*  
• *GreatswordSecondary*  
• *Hammer*  
• *Headbang*  
• *Hoe*  
• *InWater*  
• *Interact*  
• *Kick*  
• *Knife*  
• *KnifeSecondary*  
• *Kneel*  
• *KnockDown*  
• *Laugh*  
• *MaceSecondary*  
• *Nonono*  
• *Pickaxe*  
• *PlaceFeast*  
• *Point*  
• *Relax*  
• *RechargeLightningStaff*  
• *Rest*  
• *Roar*  
• *Scything*  
• *Shrug*  
• *Sit*  
• *SitChair*  
• *Sledge*  
• *StaffChargeAttack*  
• *StaffCharging*  
• *StaffFireball*  
• *StaffLightning*  
• *StaffRapidFire*  
• *StaffShield*  
• *StaffSummon*  
• *StaffTrollSummon*  
• *Stagger*  
• *Stir*  
• *Stop*  
• *Sword*  
• *SwordSecondary*  
• *ThumbsUp*  
• *ThrowBomb*  
• *ThrowSpear*  
• *Toast*  
• *UnequipHip*  
• *Unarmed*  
• *Wave*


# Quest System

Quests are defined in .yml files and update automatically.

### Notes

• Quests can only be started once  
• They can be cancelled only if active  
• Completed quests remain recorded forever  
• Dialogue requirements use the same rules  
• Progress persists between sessions

### Core Properties

• *UniqueID*  
• *Name*  
• *Type*  
• *PrefabName*  
• *PrefabNames*  
• *Threshold*

### Quest Types

• *Collect*  
• *Farm*  
• *Harvest*  
• *Kill*  
• *LearnItems*  
• *Mine*

### Tips

• Use Collect for item-based quests  
• Use Harvest for pickables  
• Use Farm for crops  
• Use LearnItems for discovery quests  
• Threshold defines the required amount
