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
- Achievements
- Achievement Rewards
- Leaderboard
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

Do not touch!
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


