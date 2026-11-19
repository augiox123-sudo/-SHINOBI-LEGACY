# SHINOBI LEGACY — Unity 3D foundation

A mobile-first 3D action RPG foundation. Gameplay target: isometric real-time combat, responsive movement, enemy encounters, loot, character progression, quests, dungeons and bosses. The world, characters, names, story and assets are original.

## Engine
- Unity 2022.3 LTS
- Universal Render Pipeline (URP) 14
- Android first

## Current milestone
This repository is transitioning from an earlier browser prototype to a real Unity project. The first Unity milestone includes:
- generated 3D test arena and player capsule;
- isometric camera follow;
- camera-relative keyboard movement for editor testing;
- on-screen virtual joystick for Android movement;
- Android-oriented URP project configuration.

## Open the project
1. Install Unity Hub and Unity 2022.3 LTS with Android Build Support (SDK, NDK and OpenJDK).
2. Clone this repository and add its folder in Unity Hub.
3. Open the project and wait for package import.
4. Select **Tools → Shinobi Legacy → Create 3D Test Scene**.
5. Open `Assets/Scenes/PrototypeArena.unity` and press Play.

The generated scene uses primitive geometry as a functional blockout, not final art. No APK is claimed by this source milestone; an APK must be built in Unity Editor and tested on a device.

## Structure
- `Assets/Scripts/Player` — player movement
- `Assets/Scripts/Camera` — isometric camera
- `Assets/Editor` — test-scene generator
- `Packages` — Unity package manifest

## Implemented in source
- Player health and damage handling.
- Basic melee attack (Space in Editor/PC, on-screen button on mobile).
- Chakra Burst skill with chakra cost and cooldown (Q in Editor/PC, on-screen button on mobile).
- Fire Style skill with chakra cost, forward hit detection and cooldown (E in Editor/PC, on-screen button on mobile).
- Health and chakra bars with skill cooldown feedback in the generated HUD.
- Chakra regeneration and technique cost.
- Three training enemies that detect, approach and attack the player.
- Stone Guardian boss with a telegraphed heavy slam and a dedicated boss health bar.
- Enemy health and death handling.
- Prototype arena is registered as the first build scene.

## Next milestones
1. Improve combat feel with attack animations, hit reactions, effects and targeting.
2. Mobile skill layout, cooldown feedback and health/chakra HUD.
3. Real 3D environment, character assets and animations.
4. Loot, inventory, equipment and progression.
5. Quests, dungeons and boss encounters.
6. Android profiling, optimization and APK testing.


## RPG progression and loot prototype

- Player XP and level progression with increasing XP requirements and a health increase on level-up.
- Training enemies grant 35 XP and have a 72% chance to drop an item.
- Stone Guardian grants 250 XP and always drops one item.
- Loot has Common, Uncommon, Rare, Epic and Legendary rarity tiers, with randomized item power.
- Walk into a dropped orb to collect it; collected items are stored in the prototype inventory (60 slots).
- The HUD displays player level, current XP and inventory item count.

This is still a Unity source prototype. It has not yet been compiled or tested in the Unity Editor, and no APK has been produced.


## Equipment and item stats

- Inventory items can be equipped from their matching gear slots: Weapon, Chest, Accessory and Hands.
- The prototype HUD includes an **EQUIP BEST** action to equip the highest-power item currently in the inventory.
- Weapon power increases basic and chakra skill damage.
- Non-weapon equipment adds defense and reduces incoming damage.
- Current equipment power is displayed in the HUD.


## Safe village hub blockout

- Added an editor scene generator at **Tools → Shinobi Legacy → Create Safe Village Hub**.
- The generated hub has a guarded main gate, perimeter walls, forest edge, central market square, six named merchant stalls, residential blocks, academy, hospital, forge, mission office, training ground and a high ridge monument.
- The layout uses original placeholder geometry and abstract monument details. Merchant stands are visual placeholders at this stage; vendor dialogue, buy/sell UI, currency, player spawn and travel links are not implemented yet.
- The generator saves `Assets/Scenes/SafeVillage.unity` and adds it to Build Settings alongside the prototype arena.


## Village merchant economy prototype

- The safe village scene generator now creates a playable arrival point, isometric camera and Android virtual joystick.
- Six market stalls receive vendor interaction triggers. Walk close to a vendor and tap **OPEN SHOP** (or press E in Editor/PC).
- The prototype shop supports buying stock into the 60-slot inventory and selling carried inventory items for coins.
- New players start with 500 prototype coins. Wallet and inventory are runtime-only at this stage; persistence, equipped-item selling restrictions, consumable effects and a polished mobile shop UI remain future work.
- Generate the scene with **Tools → Shinobi Legacy → Create Safe Village Hub**. The scene is source-generated and still requires Unity Editor compilation and device testing.


## Runtime graphics settings

- Added an in-game **GRAPHICS** menu that is created automatically when a scene loads.
- Players can switch the frame-rate target between **30 FPS** and **60 FPS**.
- Players can switch real-time shadows **ON/OFF**. Turning shadows off disables Unity's shadow rendering and sets shadow distance to zero to reduce GPU load.
- Choices are saved locally with PlayerPrefs and restored on the next launch.
- The default is 30 FPS with shadows disabled for a more conservative mobile performance baseline. Selecting 60 FPS requests a 60 FPS target; actual performance depends on device capability, scene load and thermal limits.
- The settings script is source-added but has not yet been compiled in Unity Editor or tested on a physical Android device.


## Local player save

- Added a local JSON save for the safe-village player: coins, carried inventory and equipped gear.
- The save updates when currency, inventory or equipment changes and is also written when the app is paused or closed.
- The generated Safe Village player now receives the save component automatically.
- This is currently a device-local prototype save using PlayerPrefs. It is not cloud synchronization, account security or a server-authoritative multiplayer save.


## Safe-zone respawn and fast travel

- The Safe Village player now has a health component and a respawn controller. On death, the character returns to the village safe spawn after a short delay, is restored to full health and receives temporary damage protection.
- Death does not remove inventory, currency or equipped items. The player save is written when death occurs and after respawn.
- Local autosave also writes a checkpoint every 30 seconds, as well as on app pause/quit and item, currency or equipment changes.
- Added a **WORLD MAP** overlay with a stylized village map and clickable fast-travel markers. The unlocked destinations are also listed below the map.
- The village gate is the initial unlocked destination. Central Market, Village Administration and Training Grounds unlock when the player explores close enough to their markers.
- Unlocks are stored locally. Fast travel moves the player to the selected discovered point and saves the current player data.
- This is the first local prototype of the map/travel system. A full illustrated world map, multiple connected regions, dungeon maps and server-backed account saves remain future work.


## Legendary ability transformations and mastery

- Legendary gear now carries a serialized ability-modifier affix. The modifier is active only while that legendary item is equipped; removing it restores the base skill behaviour.
- Legendary loot rolls one of five first-pass skill transformations:
  - **Flame Wave** (fire_wave): Fire Style travels through three wider damage zones.
  - **Twin Dragons** (fire_dragon): Fire Style launches two parallel flame lanes.
  - **Inferno Impact** (fire_burst): Fire Style detonates in an area around its first impact.
  - **Chakra Tempest** (burst_storm): Chakra Burst creates two broader overlapping pulses.
  - **Guardian Pulse** (burst_guard): Chakra Burst also restores 18% of the player's maximum health.
- Skills also have independent mastery ranks from 1 to 5. Each successful cast records use; every 20 casts at the current rank advances mastery and increases that skill's damage by 5%.
- Ability ranks, use counts, and legendary affixes on carried/equipped gear are included in the local save.
- The combat HUD displays skill rank and the active legendary modifier.
- These are first-pass gameplay transformations using the current combat hit logic. Dedicated animated chakra/fire VFX, unique animation sets, further skills and multi-item legendary synergy are planned for the next combat-art pass.


### Elemental status effects and legendary synergy

- Legendary fire transformations now apply a 3-second burn (damage over time) to each target hit. A target is counted once per cast even when it overlaps multiple hit zones or parallel fire lanes.
- Chakra Tempest briefly stuns enemies for 0.8 seconds. Stun interrupts regular enemy movement/attacks and can cancel a boss wind-up.
- Equipping at least two legendary items with ability affixes activates Legendary Synergy, increasing technique damage by 10%. The combat HUD reports when this bonus is active.
- These effects are implemented in gameplay logic; visual burn, stun, flame trails, impact bursts and chakra shockwave effects still need dedicated VFX and animation work.


- Two additional legendary affixes are available: **Chain Lightning** (Chakra Burst jumps to up to three nearby enemies with reduced damage per jump and a brief stun) and **Meteor Flame** (Fire Style detonates in a larger, harder-hitting burning impact zone).
- Equipping a replacement item now returns the previously equipped item to the inventory instead of silently deleting it.


### First-pass real-time ability VFX

- Added `AbilityVfxController`, automatically attached to the player combat component at runtime.
- Fire Style now emits directional elemental particle streams; Flame Wave emits three impact bursts; Twin Dragons emits two parallel streams; Inferno Impact and Meteor Flame create distinct impact bursts and expanding ground shockwaves.
- Chakra Burst emits a chakra burst and ground shockwave. Chakra Tempest adds a second forward pulse; Guardian Pulse uses a green chakra treatment.
- Chain Lightning draws a short jagged LineRenderer bolt between each chained target and emits a small impact spark at the destination.
- Effects use lightweight runtime-generated Unity Particle Systems and simple geometry, with no external asset dependency. They are functional placeholder VFX, not final authored AAA-quality effects; dedicated meshes, textures, animation curves, audio and mobile GPU profiling remain required.


### Persistent character progression and equipment quality-of-life

- Local save data now includes player level and current XP in addition to coins, inventory, equipped items and skill mastery.
- Loading restores the saved level/XP and reapplies the character's level-based maximum-health bonuses.
- XP/level changes trigger an immediate local save; the existing periodic and app-pause saves remain in place.
- `Equip Best` now compares each candidate against the currently equipped item in the same slot and equips only a genuine power upgrade. It no longer picks a high-power item from an unrelated slot or replaces stronger gear with weaker gear.


## Regional world progression and expedition zones

- The Safe Village generator now lays out four distant expedition outposts: **Ashen Ruins**, **Verdant Forest**, **Thunder Shrine** and **Drowned Temple**. Each has a themed blockout terrain area, boundary stones, dungeon altar, boss monument and a named destination marker.
- Region access is level-gated: Ashen Ruins (5), Verdant Forest (10), Thunder Shrine (18) and Drowned Temple (28). Unlocks are stored locally and appear on the world map.
- The world map now spans the village and all four regional outposts. Regional fast-travel points become available when their region unlocks.
- Entering an unlocked region activates a three-enemy patrol. Clearing the patrol summons a stronger regional champion with a guaranteed item drop and a 12% legendary chance. The patrol cycle can respawn after the player leaves the area and the respawn delay passes.
- Regional bounty contracts can be accepted from the world map. One contract can be active at a time; defeat 10 enemies in the selected region to receive level-scaled coins and XP. Contract progress is stored locally.
- These regions and encounters are functional source-level blockouts. They still need authored 3D environments, unique enemy/boss models, animations, audio, and full Unity runtime validation.


## Dungeon boss phase mechanics

- Dungeon bosses and regional elite champions now enter **Phase II** when their health falls to 55%. Their movement and basic slam attacks become faster and more dangerous.
- Each region has a distinct Phase II ability:
  - **Ashen Ruins:** a high-damage burst strike.
  - **Verdant Forest:** the boss regenerates 8% of its maximum health.
  - **Thunder Shrine:** a heavy strike that briefly stuns the player.
  - **Drowned Temple:** a water-force strike that briefly interrupts the player.
- Bosses visibly change color during their second phase and telegraph special attacks with a longer wind-up, giving the player a reaction window.
- This is a first gameplay implementation using placeholder models and color cues. Dedicated attack animations, arena telegraphs, elemental VFX and balance testing remain outstanding.
