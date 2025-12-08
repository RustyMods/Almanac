# Almanac

Welcome to Rusty's Almanac, your gateway to the enchanting world of Valheim!

Rusty's comprehensive plugin catalogs all the items, pieces, and creatures that is loaded into the game.

Explore the depths of Valheim with ease, armed with a wealth of information at your fingertips.

Moreover, Rusty's Almanac goes beyond mere documentation. With an integrated customizable achievement system and a leaderboard, users can shape their own unique experiences.


# Achievements
Almanac lets you define custom achievements using `.yml` files in the Achievements folder.
These achievements sync between server and client, and are dynamically reloaded when edited.

Below are the available **Achievement Types** you can use:

|                       |                 |                  |                       |
|-----------------------|-----------------|------------------|-----------------------|
| `Arrows`              | `ArrowsShot`    | `Axes`           | `Bows`                |
| `Consumables`         | `CreatureGroup` | `CreatureTamed`  | `Deaths`              |
| `DistanceInAir`       | `DistanceRan`   | `DistanceSailed` | `DistanceWalked`      |
| `EnemyKills`          | `Fish`          | `FoodEaten`      | `ItemsPicked`         |
| `Kill`                | `Knives`        | `Maces`          | `Materials`           |
| `MineHits`            | `Pickable`      | `PlayerKills`    | `PoleArms`            |
| `Potions`             | `Recipes`       | `Shields`        | `Spears`              |
| `Staves`              | `Swords`        | `TimeInBase`     | `TimeOutOfBase`       |
| `TotalJumps`          | `TotalMined`    | `Trophies`       | `Valuables`           |
| `Weapons`             | `CollectItems`  | `Achievements`   |                       |


### Achievement File Structure
Each achievement is defined as a YAML file with properties like:
- `UniqueID`: A unique identifier string (e.g., `Weapons.001`).
- `Name`: Display name for the achievement.
- `Lore`: A short description or flavor text.
- `Icon`: The icon name from the game's assets.
- `TokenReward`: Reward tokens for completing the achievement.
- `Requirement`: The type, threshold, and optional group or prefab name.

### Examples
- Defeating creatures from a biome: `AchievementType.CreatureGroup` with `Group = Meadows`.
- Reaching a milestone (kills, distance, etc.): set `Requirement.Type` to the relevant stat.
- Collecting all of an item type (fish, weapons, trophies, etc.): use collection-based types.

### Tips
- Files can be added, changed, or deleted while the server is running.
- Server automatically syncs achievements to clients.
- Thresholds can be left at `0` for auto-detection (e.g., total number of fish).
- PrefabName is required for `Kill` and `Pickable` types.
- `CollectItems` type Requirement.PrefabName entry format: `prefabname,amount;prefabname,amount`

# Almanac Bounties
The Almanac Bounty system lets players purchase bounty contracts to hunt special creatures.

Bounties are defined in `.yml` files inside the **Bounties** folder.
Admins can add, remove, or edit bounty entries at runtime — changes will sync to all clients.

Each bounty entry can define:
- **UniqueID**: Unique identifier for the bounty (e.g., `Troll.001`).
- **Creature**: The prefab name of the target creature (e.g., `Troll`, `Serpent`).
- **Name**: (Optional) Custom name override; if empty, a generated name will be used.
- **Icon**: Trophy sprite or icon for the bounty.
- **Biome**: The biome where the bounty will spawn (`Meadows`, `Swamp`, `AshLands`, etc.).
- **Health**: Override maximum health value of the bounty.
- **Level**: Creature level (scales difficulty).
- **DamageMultiplier**: Multiplier applied to the bounty’s attacks (e.g., `1.5`).
- **AlmanacTokenReward**: Tokens given upon completing the bounty.
- **Lore**: A short description displayed in the Almanac panel.
- **Cost**: Item or token requirements to purchase the bounty.
### Notes
- Costs can be `AlmanacToken` or regular items (e.g., `Coins`).
- If the bounty despawns or escapes the cost is returned.
- Bounties are subject to a configurable cooldown (default in minutes).
### Tips
- You can reload or edit `.yml` bounty files while the server is running; changes sync automatically.
- Each bounty spawns a pin on the map when accepted.
- Default bounties include **Boar, Neck, Troll, Serpent, Abomination, Wraith, Lox, Seeker Brute, Fallen Valkyrie**.
- Use `Lore` to tell a short story or flavor text for each hunt.
# Example Entry
```yml
UniqueID: Troll.001
Creature: Troll
Name: Forest Stalker
Icon: TrophyFrostTroll
Biome: BlackForest, Swamp
Health: 1200
Level: 3
DamageMultiplier: 1.5
AlmanacTokenReward: 5
Lore: "Lumbering through the Black Forest, the troll’s steps shake the earth as it smashes all in its path."
Cost:
  Coins: 10
```

# Almanac Store
The Almanac Store allows players to purchase temporary buffs, resources, and items.

Store items are defined in `.yml` files inside the **Store** folder.
Admins can add, remove, or modify store entries while the game is running.

Each store entry can define:
- **Name**: The store item name.
- **Lore**: A short description shown in the tooltip.
- **Icon**: The sprite name to display.
- **Cost**: Either `AlmanacToken` or other items as payment.
- **StatusEffect**: Optional effect applied when purchased.
- **Items**: Optional rewards given to the player (like resources or gear).
- **RequiredKey** Optional key required to purchase item

### Example
```yml
Name: Lolite
Cost:
  Items:
  - PrefabName: AlmanacToken
    Amount: 100
Icon: GemstoneBlue
Items:
- PrefabName: GemstoneBlue
  Amount: 1
  Quality: 1
  Variant: 0
Lore: Light is reflected sharply off this gem
RequiredKey: Charred_Melee_Dyrnwyn
```

### Notes
- `AlmanacToken` is the default store currency.
- Costs can also be regular items (e.g. `Wood`, `Coins`).
- Status effects must match valid IDs in the ObjectDB.
- Item entries require a valid prefab name and amount.
- `RequiredKey` should be `PrefabID` of creature, to check if player has killed at least one

### Tips
- Use `Lore` to explain what the store item does.
- You can reload or edit `.yml` files during runtime; the store updates automatically.
- The Almanac panel will refresh when the store tab is selected.

# Custom Status Effects
You can create new status effects using Almanac Custom Status Effects Manager.

Below are the available modifier types:

|                       |                       |                       |                       |
|-----------------------|-----------------------|-----------------------|-----------------------|
| `Armor`               | `AttackStaminaModifier` | `BlockStaminaModifier` | `BluntDamage`         |
| `BluntResistance`     | `CarryWeight`         | `ChopDamage`           | `ChopResistance`      |
| `DamageModifier`      | `DamageReduction`     | `DodgeStaminaModifier` | `Eitr`                |
| `EitrRegenModifier`   | `FallDamageModifier`  | `FireDamage`           | `FireResistance`      |
| `FrostDamage`         | `FrostResistance`     | `Health`               | `HealthRegenModifier` |
| `HomeItemStaminaModifier` | `JumpStaminaModifier` | `LifeSteal`        | `LightningDamage`     |
| `LightningResistance` | `MaxFallSpeed`        | `NoiseModifier`        | `PickaxeDamage`       |
| `PickaxeResistance`   | `PierceDamage`        | `PierceResistance`     | `PoisonDamage`        |
| `PoisonResistance`    | `RaiseSkills`         | `RunStaminaModifier`   | `SlashDamage`         |
| `SlashResistance`     | `SneakStaminaModifier`| `Speed`                | `SpiritDamage`        |
| `SpiritResistance`    | `Stamina`             | `StaminaRegenModifier` | `SwimStaminaModifier` |
| `WindMovementModifier`| `WindRunStaminaModifier` |                       |                       |

Each modifier uses a `float` value to define the effect strength.
- Some like `CarryWeight` are additive.
- Most others are multipliers.

### Tips
- You can create, delete, or edit files while in-game to preview tooltips live.
- Almanac also includes a built-in creation tool for admins.
- Only the host can add status effects, everyone else gets the data from the host
- If you are admin, you can use the creation tool to define your status effect file, then upload the file to your server.


# Treasure Hunts
Almanac lets you define custom treasure hunts using `.yml` files in the TreasureHunt folder.
These treasures sync between server and client, and are dynamically reloaded when edited.

Treasure hunts create interactive map pins that spawn loot containers when you reach their location.
Players can purchase treasure hunts using tokens from the Almanac store system.

### Core Properties
- `Name`: Display name for the treasure hunt (e.g., "Meadow Stash").
- `Lore`: Descriptive flavor text shown to players.
- `Icon`: The icon name from game assets (defaults to "map" if not specified).
- `Biome`: Target biome where the treasure will spawn (see list above).

### Cost System
- `Cost`: Dictionary of required tokens/items to purchase this treasure hunt.
    - Use `AlmanacToken` as the key for Almanac tokens.
    - Example: `Cost: { "AlmanacToken": 10 }` for 10 tokens.

### Loot Configuration
- `Loot`: List of items that can be found in the treasure container.
    - `Item`: Prefab name of the item (e.g., "Coins", "SilverOre").
    - `Min`: Minimum stack size.
    - `Max`: Maximum stack size.
    - `Weight`: Drop chance weight (higher = more likely).

## Example Treasure File
```yaml
Name: Mountain Stash
Lore: Only those who brave the jagged cliffs will uncover what the mountains hide.
Icon: map
Biome: Mountain, Plains
Cost:
  almanac_token: 1
Loot:
  - Item: SilverOre
    Min: 10
    Max: 20
    Weight: 1.0
  - Item: WolfClaw
    Min: 1
    Max: 20
    Weight: 1.0
  - Item: TrophyCultist
    Min: 1
    Max: 1
    Weight: 1.0
```

# Dialogue System
Almanac provides a comprehensive NPC dialogue system using `.yml` files in the Dialogues folder.
These dialogues sync between server and client, and are dynamically reloaded when files are edited.

Below are the available **Command Types** you can use:
- `Exit`: closes dialogue
- `Give`: adds item into player inventory
- `Take`: removes item from player inventory
- `Teleport`: teleports player to position
- `FlyTo`: Valkyrie flies player to position
- `MapPin`: adds temporary pin on the map
- `StartBounty`: starts a bounty
- `CancelBounty`: cancels active bounty
- `CompleteBounty`: rewards bounty
- `StartTreasure`: starts a treasure hunt
- `CancelTreasure`: cancels active treasure hunt
- `OpenAlmanac`: opens almanac panel
- `OpenItems`: opens almanac item tab
- `OpenPieces`: opens almanac pieces tab
- `OpenCreatures`: etc.
- `OpenAchievements`
- `OpenStore`
- `OpenLeaderboard`
- `OpenBounties`
- `OpenTreasures`
- `OpenMetrics`
- `OpenLottery`
### Dialogue File Structure
Each dialogue is defined as a YAML file with properties like:
- `UniqueID`: A unique identifier string (e.g., `npc.intro.001`).
- `Label`: Button text that appears for this dialogue option.
- `Text`: Main dialogue text displayed when requirements are met.
- `AltText`: Alternative text shown when requirements are not met.
- `Dialogues`: List of dialogue IDs that become available as options.
- `Action`: Command to execute with label and parameters.
- `Requirements`: Conditions that must be met to interact with this dialogue.

### Action Commands
**Panel Commands**: `OpenAlmanac`, `OpenItems`, `OpenCreatures`, etc. open specific UI panels.

**Item Commands**:
- `Give`: Gives items to player. Parameters: `ItemName, Amount, Quality?, Variant?` `? = optional`
- `Take`: Takes items from player. Same parameter format.

**Location Commands**:
- `Teleport`: Instantly transports player. Parameters: `X, Y, Z`
- `MapPin`: Adds temporary map marker. Parameters: `X, Y, Z, Label, Duration (seconds)`

**Activity Commands**:
- `StartBounty`: Begins bounty hunt. Parameters: `BountyID`
- `StartTreasure`: Begins treasure hunt. Parameters: `TreasureID`
- `CancelBounty`/`CancelTreasure`: Cancels active hunts. (Will not be displayed if no active hunts)

**API Commands**
- `GiveAlmanacXP`: Gives almanac xp. Parameters `amount` - Recorded dialogue
- `GiveWackyXP`: Gives EpicMMO xp. Parameters `amount` - Recorded dialogue

### Requirements System
Control when dialogues are available using:
- `Keys`: Player must have specific game keys (boss defeats, etc.)
- `NotKeys`: Player must NOT have specific keys
- `Killed`: Required creature kills. Format: `CreatureName, Count; AnotherCreature, Count`
- `NotKilled`: Creatures player must NOT have killed
- `Achievements`: Required achievement IDs (comma-separated)
- `NotAchievements`: Achievements player must NOT have
- `Dialogues`: Required Dialogue IDs (recorded by `Give` or `Take` Actions)
- `NotDialogues`: Required NOT Dialogue IDs
- `Quests`: Required accepted Quests. Format: `Quest1,Quest2`
- `NotQuests`: Required Quest never taken
- `CompletedQuests`: Required Quest fully completed
- `NotCompletedQuests`: Required Quest NOT fully completed

### Text Features
**Alternative Text**: Use `AltText` to show different messages when requirements aren't met.

**Conditional Display**: Dialogues automatically show different text based on:
- Whether player has required items (for Take commands)
- Whether player already received rewards (for Give commands)
- Whether requirements are satisfied

### Examples

**Basic Conversation**:
```yaml
UniqueID: npc.greeting
Label: Hello there
Text: Welcome to our village, traveler!
Dialogues:
  - npc.ask_directions
  - npc.ask_trade
Action:
  Type: Exit
  Label: Farewell
```

**Item Trading**:
```yaml
UniqueID: npc.trade_sword
Label: I need a weapon
Text: Here, take this iron sword for your journey.
AltText: I already gave you a sword, remember?
Action:
  Type: Give
  Label: Take Sword
  Parameters: SwordIron, 1, 2, 0
Requirements:
  Killed: Eikthyr, 1
```

**Location Marking**:
```yaml
UniqueID: npc.mark_cave
Label: Where's the nearest cave?
Text: There's a cave system to the north. Let me mark it for you.
Action:
  Type: MapPin
  Label: Mark Cave
  Parameters: 100, 25, -150, Mysterious Cave, 180 // (3min)
```

**Requirement-Based Dialogue**:
```yaml
UniqueID: npc.veteran_talk
Label: Tell me about the bosses
Text: You've proven yourself against the ancient evils!
Requirements:
  Killed: Eikthyr, 1; gd_king, 1
  Keys: defeated_bonemass
```

### Organization Tips
- Use descriptive UniqueIDs like `merchant.weapons.intro`
- Organize files by NPC type or location
- Create conversation trees using the `Dialogues` list
- Use folders to separate different areas or storylines

### Technical Notes
- Files can be added, changed, or deleted while the server is running
- Server automatically syncs dialogues to clients
- Map pins disappear after set delay seconds
- Give/Take commands automatically prevent duplicate transactions
- Requirements are checked in real-time

## NPC

NPC can be customized by being an `admin` in `no cost` mode

## Random Talk
Additionally, you can set each NPC with random talk that triggers whenever a player gets close or leaves, or every minute.

The YML files are synced and can be reloaded during gameplay.

## NPC Animations
|                       |                       |                       |                       |
|-----------------------|-----------------------|-----------------------|-----------------------|
| `Atgeir`              | `AtgeirSecondary`     | `AttachAsksvin`       | `AttachLox`              |
| `AttachMast`          | `AttachShip`          | `AttachSitShip`       | `AttachThrone`           |
| `Axe`                 | `AxeSecondary`        | `Battleaxe`           | `BattleaxeSecondary`     |
| `BlowKiss`            | `Blocking`            | `Bow`                 | `BowAim`                 |
| `BowFire`             | `Challenge`           | `Cheer`               | `ComeHere`               |
| `Cower`               | `Crouching`           | `Crossbow`            | `CrossbowFire`           |
| `Cry`                 | `Dance`               | `Despair`             | `Dodge`                  |
| `Drink`               | `DualAxes`            | `DualAxesSecondary`   | `DualKnives`             |
| `DualKnivesSecondary` | `Eat`                 | `Encumbered`          | `EquipHead`              |
| `EquipHip`            | `Equipping`           | `Flex`                | `FishingRod`             |
| `FishingRodThrow`     | `Forge`               | `GPower`              | `Greatsword`             |
| `GreatswordSecondary` | `Hammer`              | `Headbang`            | `Hoe`                    |
| `InWater`             | `Interact`            | `Kick`                | `Knife`                  |
| `KnifeSecondary`      | `Kneel`               | `KnockDown`           | `Laugh`                  |
| `MaceSecondary`       | `Nonono`              | `Pickaxe`             | `PlaceFeast`             |
| `Point`               | `Relax`               | `RechargeLightningStaff` | `Rest`                 |
| `Roar`                | `Scything`            | `Shrug`               | `Sit`                    |
| `SitChair`            | `Sledge`              | `StaffChargeAttack`   | `StaffCharging`          |
| `StaffFireball`       | `StaffLightning`      | `StaffRapidFire`      | `StaffShield`            |
| `StaffSummon`         | `StaffTrollSummon`    | `Stagger`             | `Stir`                   |
| `Stop`                | `Sword`               | `SwordSecondary`      | `ThumbsUp`               |
| `ThrowBomb`           | `ThrowSpear`          | `Toast`               | `UnequipHip`             |
| `Unarmed`             | `Wave`                |                       |                          |


# Quest System
The Almanac Quest System allows players to take on custom quests that track progress across various activities in Valheim.  
Quests are defined in `.yml` files inside the **Quests** folder, and sync between server and client. Changes are dynamically reloaded when files are added, edited, or removed.

Quest system is designed to work along with dialogue system, use the command: `StartQuest`, `CancelQuest`, `CompleteQuest` to interact with the quests.

### Notes
- Quests can only be started if player has never started said quest
- Quests can only be cancelled if quest is active
- Quests remain active after completion (that is to keep record of completion), meaning when using dialogue requirements: `Quests` or `NotQuests` it is checking if quest has ever been taken.
- Quests can only be completed if progress has met threshold
- All these behaviors are reflected in the dialogue system. If those conditions are not met, the interactable button, will not be displayed as an option.

## Core Properties
Each quest file can define:
- **UniqueID**: A unique identifier string (e.g., `001.Dandelion`).
- **Name**: Display name for the quest.
- **Type**: The quest type (see list below).
- **PrefabName**: Target prefab for the quest (e.g., `Boar`, `Pickable_Dandelion`, `SurtlingCore`).
- **PrefabNames** *(optional)*: A list of prefabs for collection or learning quests.
- **Threshold**: Amount required to complete the quest.

### Quest Types

|                       |                       |                       |                       |
|-----------------------|-----------------------|-----------------------|-----------------------|
| `Collect`             | `Farm`                | `Harvest`             | `Kill`                |
| `LearnItems`          | `Mine`                |                       |                       |

### Example Quest
```yml
UniqueID: 001.BoarHunt
Name: Hunt Boars
Type: Kill
PrefabName: Boar
Threshold: 10
```
This quest requires players to hunt 10 Boars. Progress is tracked automatically when players kill the target prefab.

### Notes

- Quests can be started, canceled, or completed dynamically in-game.
- Progress is saved to the player profile and restored on reconnect.
- Quests are synchronized from server to client.
- Multiple quests can be active at once.
- UI will display quests-in-progress, can be hidden using configured hotkey
- Quest history can be viewed in the metrics tab

### Tips

- Use Collect for item-based quests (e.g., `SurtlingCore`).
- Use Harvest for pickable quests like dandelions or mushrooms (e.g., `Pickable_Dandelion`).
- Use Farm for planted crops (e.g., `sapling_seedcarrot`).
- Use LearnItems to create discovery quests where players must learn recipes from multiple items.
- Thresholds define how much progress is needed (kills, items, harvests, etc.).

![](https://i.imgur.com/lJbEYvq.png)
![](https://i.imgur.com/oh1Y7D0.png)
![](https://i.imgur.com/4d4LFnW.png)