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
