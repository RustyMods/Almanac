# 3.6.4
- added status effect option for achievement reward
- achievement either rewards status effect or token, not both
- if achievement has status effect, and config to use effects are on, achievement will never be marked as completed
- instead, it allows players to apply or remove effect
- config to set max amount of concurrent achievement effects
- transitioned to vanilla player profile kill data (found out they already record kill amount per creature)
- added notification icon to mark achievements that have been `completed` and `not collected`
- added status effect icon to mark achievements that reward effects
- added notification icon to mark active bounty
- if there is an active bounty, only active bounty is interactable
- removed log warnings if discord bot is not installed
- fixed treasure cancel message showing bounty canceled

# 3.6.3
- made npc build piece configurable
- added optional items reward to bounties
- added achievement notifications [default config is off]
- added notification when bounty is vanquished

# 3.6.2
- NPC can be removed by creator
- added new config: `admin list`
- removed `TentaRoot` from bounty generation list
- added `DiscordBot` almanac commands

# 3.6.1
- Added null check on character drop prefabs when searching for creature trophies
- Added variant check on store items
- Made Almanac NPC available to all players but dialogue and random talk settings admin only
- Added random bounty generator [beta]
- Fixed marketplace not loading saved marketplace items

# 3.6.0
- Added config toggle to check no cost for admin tools

# 3.5.3
- Fixed kill achievement not tracking correctly

# 3.5.2
- Fixed store conversion to token allowing to continually convert even without requirements

# 3.5.19
- Fixed NPC settings not being applied when leave / return to scene
- Added dialogue command: `FlyTo`
- Tweaked README.md
- Added color box to NPC customization to preview skin/hair color input

# 3.5.18
- Improved NPC animations
- Quest System - to work in conjunction with Dialogue system
- Quest UI
- Reminder: All UI can be dragged around using `L.Alt + drag`

# 3.5.17
- Fixed store item form text staying red even though required killed is correct
- Added edit button to modify store item, custom effect, achievement, bounty and treasures
- Updated readme to breakdown how to create custom status effects
- Even more null checks for modded creatures

# 3.5.16
- Added null check when indexing ZNetScene and ObjectDB prefabs
- Fixed bounty return cost default item from `$item_coins` to `Coins`, if prefab is not valid
- Fixed bounty override health not being applied
- Config to modify grid element size (if you want to change row count)

# 3.5.15
- YAML is now an external dependency: https://thunderstore.io/c/valheim/p/ValheimModding/YamlDotNet/
- Fixed leaderboard get rank method dividing by zero when player has never died
- Fixed store status effect purchase not resetting timer
- New feature: Placeable NPC (alpha) [admin only]
- All tabs are configurable, so you can use NPC to access almanac instead of normal button
- Main button (that replaced trophy button) can be disabled
- Releasing NPC (alpha) early due to some errors that needs to be addressed

# 3.5.14
- Added Icons folder to register custom icons
- Improved descriptions baits, dropped by, used by to use interactable icons
- Added filters server sync
- Fixed almanac data copying previous loaded character onto new one

# 3.5.13
- Fixed treasure not consuming cost when purchased
- Fixed bounties not consuming cost when purchased
- Added config for treasures and bounties to return cost if canceled
- Fixed being able to spawn items even when not admin and not no cost

# 3.5.12
- Added option to use arrow key up and down
- Fixed valuables showing all materials
- Fixed swords showing all weapons
- Added Bjorn and Unbjorn to default creature groups - delete old files if you want to use them
- Added used in recipes to item description

# 3.5.11
- Added tooltip for jewelcrafted items
- Added trinkets achievement category
- Fixed scroll sensitivity, configurable

# 3.5.1
- Updated filters list, delete old file
- Fixed conversion toggle targeting wrong config
- Fixed arrow keys allowing to see unknown entries

# 3.5.0
- Complete Overhaul ! Delete old files
- Added Lottery and store
- Added Almanac Tokens as reward to use in store
- Added Marketplace to sell items
- Added Create New option to create achievements, status effects, store items, bounties and treasure hunts while in game, admin only

# 3.4.2
- Overhaul!

# 3.4.1
- Fixed for recent update

# 3.4.0
- Improved bounty system

# 3.3.9
- Config to show lore regardless of completion of achievement, on by default

# 3.3.8
- Fixed achievement not being able to see progress due to button not being interactable

# 3.3.7
- Fixed missing outline gameobject on achievements

# 3.3.6
- Fixed kills not being recorded when monster was not killed by zone owner
- Added auto-graphics change if using MinimalUI

# 3.3.5
- Added checks on files to avoid user errors
- Send data to server the first time user checks their completed achievements

# 3.3.4
- Fixed achievement effect resistances weakening player
- Added more data to the metrics: resistances, total achievement effects

# 3.3.3
- Added Health, Stamina, Eitr, LifeSteal, Armor as achievement effect bonuses

# 3.3.2
- Added config to disable achievements, bounties or treasure hunts

# 3.3.1
- Fixed trophies

# 3.3.0
- Added cooldown to treasure and bounty hunts

# 3.2.9
- Removed cost return for treasure hunts exploit

# 3.2.8
- Removed player data directory

# 3.2.7
- Ashland update

# 3.2.6
- Resolved bug when trying to create custom runetexts with other mods

# 3.2.5
- Fixed a minor issue of trying to send leaderboard data while on menu rather than in-game

# 3.2.4
- Bounties and achievements can reward almanac class experience

# 3.2.3
- Attempt to fix bounties not registering correctly

# 3.2.2
- Minor tweak to player patches

# 3.2.1
- Added ability to add cost to bounties and treasure hunts

# 3.2.0
- Fixed logout issues and patched eating food metric

# 3.1.9
- Improved Bounty UI, Added Treasure Hunt

# 3.1.8
- Fixed achievement button

# 3.1.7
- Added new achievement type: CustomPickable and Almanac Bounties

# 3.1.6
- Improved terminal commands
- Tweaked player controller to not move while almanac is open

# 3.1.5
- Added feature to create grouped achievements

# 3.1.4
- Added function that updates leaderboard if player is server

# 3.1.3
- Added new achievement reward types (Items, Skills, StatusEffect)
- Added a redundancy if almanac fails to get item icon

# 3.1.2
- Moved player tracked data to player custom data
- Added configurable hotkey to open almanac

# 3.1.1
- Tweaked visuals of achievement panel to showcase active effects
- Effects persist upon death and log out / log in

# 3.1.0
- Small fixes for compatibility with Krumpac

# 3.0.9
- Fixed achievements rewards

# 3.0.8
- Hotfix if defeat key not found in player data

# 3.0.7
- Fixed interact button not working until you opened inventory

# 3.0.6
- Check completed achievements when achievements are changed

# 3.0.5
- Added further information on fish
- Fixed achievement completion percentage

# 3.0.4
- Small localization fixes and filtering

# 3.0.3
- Fixed creature panel
- Achievement panel improved
- Localization improved

# 3.0.2
- Localization for many languages built-in
- Some minor fixes

# 3.0.1
- Fixed the leaderboard
- Tweaked the creature panel

# 3.0.0
- Overhaul of entire project. Delete all old config files to clean up workspace

# 2.3.1
- Fix for list of biome creatures config

# 2.3.0
- Hotfix compatibility with recent valheim patch update

# 2.2.9
- Some krumpac compatibility work
- Moved ignore list to a yml format in the config folder

# 2.2.8
- More redundancy on custom status effects
- French translations update

# 2.2.7
- Auga incompatibility

# 2.2.6
- Added Auga incompatibility flag
- Fixed guardian power icon for custom powers

# 2.2.5
- Compatibility with World Advancement Progression

# 2.2.4
- Another minor bug fix

# 2.2.3
- Minor bug fix with kill tracker
- Added config to make almanac panel transparent to use with minimal ui

# 2.2.2
- Controller Support

# 2.2.1
- Added item count / total to panels
- Compatibility with MinimalUI

# 2.2.0
- Player Metrics and Achievement system

# 2.1.3
- Improved blacklist
- Added drop chance to creature info

# 2.1.2
- Fixed black list and duplicate pieces

# 2.1.1
- Fixed minor bug

# 2.1.0
- Added Black List feature
- Fixed minor bugs

# 2.0.1
- Almanac now supports pieces

# 2.0.0
- Major update - Almanac now supports items

# 1.0.7
- Hotfix for latest valheim patch - more updates to come soon

# 1.0.6
- Added a patch to fix any overlapping trophies

# 1.0.5
- Minor changes to the logic on how it finds creature to display in languages other than English

# 1.0.4
- Minor tweaks to make mod work in various languages by having text dynamically resize

# 1.0.3
- Fixed compatibility with modded monster mods that have missing values

# 1.0.2
- Small tweak for compatibility with RtdMonsters and monsterlabz

# 1.0.1
- Wrong name lol

# 1.0.0
- Initial Release
