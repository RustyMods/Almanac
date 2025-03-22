# Almanac
Welcome to Rusty's Almanac, your gateway to the enchanting world of Valheim! 
Rusty's comprehensive plugin catalogs all the items, pieces, and creatures 
that is loaded into the game. 

Explore the depths of Valheim with ease, armed with a wealth of information 
at your fingertips. 

Moreover, Rusty's Almanac goes beyond mere documentation. 
With an integrated customizable achievement system and a leaderboard,
users can shape their own unique experiences. 

## Features

- Indexed items, pieces and creatures
- ServerSynced Filters
- Player Metrics
- Leaderboard
- Achievements
  - Achievement Rewards
  - Achievement Groups
- ServerSynced Achievements

## Achievements
The plugin comes pre-loaded with a set of default achievements crafted by Rusty himself. 
Head to the configuration folder, easily accessible at:

[ BepinEx/config/Almanac/Achievements ]

In the configuration folder, discover a collection of YML files. 
Create and add your own to introduce custom achievements into the game. 

### Achievement Configurations
Make sure each achievement has a unique name or else it will be ignored - no white spaces,
use underscore

#### Active vs Passive Effects
If duration is set to anything higher than 0, power is set as guardian power. 
Effects only applied while active, and countdown is running.

#### Reward Types

- StatusEffect
- Item
- Skill

Make sure you follow the syntax for reward types.
- Skill rewards experience rather than direct level increment.
- Skills are validated by plugin. If it fails, it defaults to None.
- Items are validated by plugin. If it fails to find, it will display a red message on the achievement panel.

#### Icons
You can use any loaded prefabs (ex:HardAntler) to use as sprite_name  
or you can choose from almanac's custom icons:
- skull
- sword_blue
- sword_brown
- arrow
- hood
- bottle_empty
- bottle_blue
- bottle_green
- fish
- bow
- necklace
- mushroom
- coins_gold
- key
- bone
- book
- crown
- gem
- gold
- map
- shield
- silver
- coins_silver
- log
- log_stack
- ring
- checkmark

If the sprite_name is invalid, the Almanac will default to use almanac icon

#### Achievement Types
```
Warning: 
If this is not set correctly, the game will give you errors. 
Make sure to use formatted text below.
```

These are categories to set for the Almanac to use to determine which values to use
to check against. Some require a goal to be set, while others do not. You can find a
comprehensive list of available types below:
```
Goal Achievements
Requires a goal to be set:
- Deaths
- EnemyKills
- RuneStones
- Recipes
- CustomPickable
```
```
Knowledge Achievements
Goal is ignored, these compare to the total values of loaded lists:
- Fish
- Materials
- Consumables
- Weapons
- Swords
- Axes
- PoleArms
- Spears
- Maces
- Knives
- Shields
- Staves
- Arrows
- Bows
- Valuables
- Potions
- Trophies
```
```
Custom Creature List Achievements
These can be defined in the configurations of the Almanac in the "Creatures" Folder
Goal is ignored, these compare to the total  values of the loaded lists:
- MeadowCreatures
- BlackForestCreatures
- SwampCreatures
- MountainCreatures
- PlainsCreatures
- MistLandCreatures
- AshLandCreatures
- DeepNorthCreatures
- OceanCreatures
- CustomCreatureGroups
    To use custom creature groups, you will need to fill the entry: "custom_group_key"
    With the YML file name (ex: Custom_Brute.yml key is Custom_Brute)
```
```
Player Stats Achievements
Requires a goal to be set:
- DeathByFall
- TreesChopped
- DeathByTree
- DeathByEdgeOfWorld
- TimeInBase
- TimeOutOfBase
- ArrowsShot
- TotalJumps
- TotalBuilds
- EnemyHits
- PlayerKills
- HitsTaken
- ItemsPicked
- DistanceWalked
- DistanceRan
- DistanceSailed
- DistanceInAir
- MineHits
- TotalMined
- CreatureTamed
- FoodEaten
- SkeletonSummoned
- DeathByDrowning
- DeathByBurning
- DeathByFreezing
- DeathByPoisoned
- DeathBySmoke
- DeathByStalagtite
- BeesHarvested
- SapHarvested
- TrapsArmed
```
```
Defeat Achievement
You can find a list of defeat keys available by looking at your configurations PlayerData folder,
There, you can find a list of all the creature's defeat keys and your characters saved stats
Requires a goal and a defeat key to be set:
- CustomKills
```
#### Terminal Command
```
almanac help                                      - List of available almanac commands
almanac keys                                      - Prints current global keys and private player keys
almanac size                                      - Prints size of almanac data saved to player file
almanac write                                     - Writes to file all the default achievements
almanac pickable [prefabName<string>]             - Prints recorded almanac picked prefab data, use all to print entire list
almanac reload                                    - Reloads graphic assets
almanac kd [prefabName<string>]                   - Prints almanac kill / death tracked data
almanac achievement [type<string>] [key?<string>] - List of prefabs included in the completion count, only list achievements
almanac reset                                     - Clears almanac data from player save file and removes active effects
```

#### Achievement Groups
version 3.1.5 introduces achievement groups

You can now set the name of a group of achievements and their index order.

This only works for reward types: Item or Skill

The achievement will only show up if

- previous achievement is completed
- previous achievement has been collected
#### Achievement Descriptors
- lore is displayed on the UI
- start_message is shown when activating achievement effect
- stop_message is shown when stopping achievement effect
- tooltip shown on the UI and is followed by all the modifiers associated with it
#### Achievement Visual and Sound Effects
start_effects and stop_effects is a list prefab names that will be created upon starting or stopping effect.
These prefabs go through Almanac validation filters, so you may end up with no effects being applied.
This is done to avoid any unwanted things being created upon effect activation or deactivation.

ex: sfx_coins_placed - will play the sound effect
#### Achievement Modifiers
Most of these key, values are self evident in their naming scheme,
Do not change m_type since I list all available damage modifiers for you,
Change the m_modifier value to the desired value. 
```
Make sure to follow the format of the text
Available damage modifier values:
- Normal
- Resistant
- Weak
- Immune
- Ignore
- VeryResistant
- VeryWeak
```
Normal will be ignored from tooltip as it changes nothing.
```
Values below 1 reduces player output
ex:
  0 = 100% decrease 
0.5 = 50% decrease
  1 = 0% no change
  2 = 100% increase

- Attack
- HealthRegen
- RaiseSkills
- Speed
- Noise
- Stealth
- RunStaminaDrain
- DamageReduction
- FallDamage
- EitrRegen
```
```
Values are direct additions or substractions
ex:

-50 = -50
  0 = no change
 50 = 50

- MaxCarryWeight
```
#### Player Data
You will find in your Almanac configuration folder and folder aptly named PlayerData
This is a local folder that saves custom Almanac data. 

Since version 3.1.2, I've moved the player data into the player save file, once you've loaded your game, it will automatically take the file data and place it inside the player save file.
It will then ignore the almanac player tracker data file.
#### Leaderboard
If the system is recognized as a server, a folder named "Players" will be generated where it will save
the players information to populate the leaderboard. This file will be updated upon new client log in and logout,
as well as periodic updates during runtime of the server.

Do not touch!

the data will be overwritten by the incoming data being sent by the clients.
#### Filters
In this folder you will find a YML file with a list of prefab names that the Almanac ignores.
Feel free to add or remove from this list. It will hot reload while the game is running.

- Any values added prefixed with a # will be ignored
#### Creatures
In this folder you find a list of YML files with a list of prefab names that the Almanac will use to determine
the set of creatures per biome. This is to be used in conjunction with Achievements.

- The list will be validated by the Almanac against its cached creature data.
- Any values added prefixed with a # will be ignored

version 3.1.3 - Adds custom creature groups

- To use these groups with your achievements, the name of your file becomes the key to the group.
- As an example, your file name is: "Custom_Brutes.yml", then you use "Custom_Brutes" as the key.

#### Example Achievement File
Any entries that has no value can be removed
```yaml
unique_name: make sure this is a unique name
display_name: ''
sprite_name: prefab ID or almanac icon name
description: ''
lore: ''
start_message: ''
stop_message: ''
tooltip: ''
defeat_key: ''
achievement_type: PlainsCreatures
custom_group_key: ''
goal: 0
duration: 0
reward_type: StatusEffect
achievement_group: ''
achievement_index: 0
custom_pickable_name: Pickable Prefab Name
item: ''
item_amount: 0
skill: ''
skill_amount: 0
start_effects:
- sfx_coins_placed
stop_effects: []
damage_modifiers:
- m_type: Blunt
  m_modifier: Normal
- m_type: Slash
  m_modifier: Normal
- m_type: Pierce
  m_modifier: Normal
- m_type: Chop
  m_modifier: Normal
- m_type: Pickaxe
  m_modifier: Normal
- m_type: Fire
  m_modifier: Normal
- m_type: Frost
  m_modifier: Normal
- m_type: Lightning
  m_modifier: Normal
- m_type: Poison
  m_modifier: Normal
- m_type: Spirit
  m_modifier: Normal
modifiers:
  Attack: 1
  HealthRegen: 1
  StaminaRegen: 1
  RaiseSkills: 1
  Speed: 1
  Noise: 1
  MaxCarryWeight: 0
  Stealth: 1
  RunStaminaDrain: 1
  DamageReduction: 0
  FallDamage: 1
  EitrRegen: 1
```
#### Bounties
New feature with version 3.1.7

Almanac bounties allows for users to select a bounty to hunt down. 
The system, randomly finds a location using the information given in the YML files and once player enters the area, will spawn said bounty.
If the player leaves the area, then the bounty will escape. This is to avoid any lingering bounties in the world.

Notes:
- You can only have one bounty active at a time.
- You can cancel bounty by selecting the same bounty.
- damages are additive
- health is fixed
- Player who activated bounty must get the last hit to get the reward
- If player does not have defeat_key, he cannot accept bounty

Server:

- Server owners can update and create bounties during run-time

#### Example Bounty YML
```yaml
creature_prefab_name: Bonemass
bounty_name: Bonemass the Decaying
sprite_name: TrophyBonemass
biome: BlackForest
reward_type: Skill
item_reward:
  item_name: ''
  amount: 0
skill_reward:
  type: Clubs
  amount: 20000
bounty_health: 5000
damage_multiplier: 2
damages:
  blunt: 60
  slash: 0
  pierce: 0
  fire: 0
  frost: 0
  lightning: 0
  poison: 30
  spirit: 0
level: 3
defeat_key: defeated_bonemass
```
You can delete any void entries

#### Example Treasure YML
```yaml
name: Meadow Treasure
sprite_name: map
biome: Meadows
currency: Coins
cost: 10
loot:
- item_name: Coins
  min_stack: 10
  max_stack: 20
  weight: 1
- item_name: Flint
  min_stack: 2
  max_stack: 10
  weight: 1
- item_name: DeerStew
  min_stack: 1
  max_stack: 20
  weight: 1
- item_name: SurtlingCore
  min_stack: 1
  max_stack: 5
  weight: 1

```


