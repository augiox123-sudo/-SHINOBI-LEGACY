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
- Enemy health and death handling.
- Prototype arena is registered as the first build scene.

## Next milestones
1. Improve combat feel with attack animations, hit reactions, effects and targeting.
2. Mobile skill layout, cooldown feedback and health/chakra HUD.
3. Real 3D environment, character assets and animations.
4. Loot, inventory, equipment and progression.
5. Quests, dungeons and boss encounters.
6. Android profiling, optimization and APK testing.
