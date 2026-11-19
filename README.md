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

## Next milestones
1. Mobile joystick and skill buttons.
2. Combat, hit detection and enemy AI.
3. 3D environment, character assets and animations.
4. Loot, inventory, equipment and progression.
5. Quests, dungeons and bosses.
6. Android profiling, optimization and APK testing.
