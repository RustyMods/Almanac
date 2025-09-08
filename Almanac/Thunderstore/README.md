# Almanac

Welcome to Rusty's Almanac, your gateway to the enchanting world of Valheim!

Rusty's comprehensive plugin catalogs all the items, pieces, and creatures that is loaded into the game.

Explore the depths of Valheim with ease, armed with a wealth of information at your fingertips.

Moreover, Rusty's Almanac goes beyond mere documentation. With an integrated customizable achievement system and a leaderboard, users can shape their own unique experiences.


# Achievements
Almanac lets you define custom achievements using `.yml` files in the Achievements folder.
These achievements sync between server and client, and are dynamically reloaded when edited.

Below are the available **Achievement Types** you can use:
```
None
Deaths
Fish
Materials
Consumables
Weapons
Swords
Axes
PoleArms
Spears
Maces
Knives
Shields
Staves
Arrows
Bows
Valuables
Potions
Trophies
EnemyKills
TreesChopped
TimeInBase
TimeOutOfBase
ArrowsShot
TotalJumps
PlayerKills
ItemsPicked
DistanceWalked
DistanceRan
DistanceSailed
DistanceInAir
MineHits
TotalMined
CreatureTamed
FoodEaten
Recipes
CreatureGroup
Pickable
Kill
```
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
- Bounties require players to kill the target directly — indirect deaths won’t count.
- If the bounty despawns, escapes, or is killed by another player, the cost is returned.
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
Biome: BlackForest
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
Name: "Mountain Stash"
Lore: "Only those who brave the jagged cliffs will uncover what the mountains hide."
Icon: "map"
Biome: "Mountain"
Cost:
  almanac_token: 1
Loot:
  - Item: "SilverOre"
    Min: 10
    Max: 20
    Weight: 1.0
  - Item: "WolfClaw"
    Min: 1
    Max: 20
    Weight: 1.0
  - Item: "TrophyCultist"
    Min: 1
    Max: 1
    Weight: 1.0
```

![](https://i.imgur.com/lJbEYvq.png)
![](https://i.imgur.com/oh1Y7D0.png)
![](https://i.imgur.com/4d4LFnW.png)