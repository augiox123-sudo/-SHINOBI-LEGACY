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

- Regional patrols now mix distinct combat roles: durable **Vanguards**, mobile **Hunters**, fast-attacking **Assassins**, plus additional Raider/Sentinel variants when patrol size is increased. Roles have different health, movement, damage, attack cadence and visual tint.



## Enemy target lock

- Player scenes attach `EnemyTargetLockSystem`.
- Nearby living enemies are discovered using a reusable, fixed-size physics overlap buffer.
- The system automatically selects a nearby enemy in the player's forward field and retains the target until it dies or leaves the acquisition range.
- A screen-space target marker, enemy name and health bar are displayed. The **NEXT TARGET** control cycles through nearby enemies and supports touch input through Unity's immediate-mode GUI.
- Target scanning is throttled; the system does not allocate a new collider array each scan.
- This is a first functional targeting pass. Camera framing, target-button placement and enemy selection feel still require testing on different Android aspect ratios.

## Mobile combat optimization pass

- Lightning target acquisition now rejects enemies behind the player and chooses the nearest valid enemy in the forward combat area.
- Combat HUD text/resource refresh is throttled to a configurable interval (default 0.15 seconds) instead of rebuilding cooldown and equipment strings every rendered frame. Health events still update the health bar immediately.
- This reduces avoidable UI work on lower-memory Android devices; actual frame-time and allocation improvements must still be measured in Unity Profiler on target hardware.

## Combat targeting and ability cooldown feedback

- Lightning Style searches for the nearest valid enemy in its forward combat area rather than relying on the first physics collider hit. Terrain and scenery colliders no longer consume the cast before an enemy can be acquired.
- The combat HUD displays remaining cooldowns for Chakra Burst, Fire, Lightning, Water and Wind abilities, alongside mastery ranks and equipped legendary modifiers.
- These changes are static-code checked only; target acquisition, HUD layout and mobile performance still require an in-Unity playtest.

## Boss-specific legendary loot

- Each of the four regional dungeon bosses and regional champions now has a dedicated legendary drop identity when the legendary rarity roll succeeds:
  - Ashen Warden: **Ashen Warden's Cinderfang** — Meteor Flame.
  - Rootbound Colossus: **Rootbound Colossus Heart** — Wind Vortex.
  - Raijin's Broken Avatar: **Raijin's Tempest Core** — Thunderstorm.
  - Abyssal Guardian: **Abyssal Guardian's Deepwater Seal** — Water Prison.
- These relics use boss-specific equipment slots and stronger secondary-stat bonuses.
- Corrected rarity roll bands so increasing a boss's legendary chance no longer unintentionally removes the Epic rarity band. Epic/Rare/Uncommon band widths remain consistent as the legendary chance changes.
- This is still a prototype loot implementation; boss drop rates and item balance require playtesting.

## Village story quest chain

- A new original **Mission Officer** NPC stands outside the Village Mission Office. Entering the interaction area opens the mission board; leaving the area closes it.
- The board offers a four-stage regional story chain:
  1. **Ashes on the Wind** — defeat 6 enemies in Ashen Ruins.
  2. **Roots Beneath the Canopy** — defeat 6 enemies in Verdant Forest.
  3. **Silence of the Storm** — defeat 8 enemies in Thunder Shrine.
  4. **The Drowned Oath** — defeat the Drowned Temple elite champion.
- Each stage requires its region to be unlocked. Only one story contract can be active at a time.
- Quest stage, active state and kill progress are stored locally. Completing a stage grants level-scaled coins and XP, then unlocks the next contract in the chain.
- Every chapter also grants a guaranteed unique legendary relic with its own equipment slot, secondary stats and skill modifier: Meteor Flame, Wind Vortex, Thunderstorm or Water Prison. If the inventory is full, the relic drops beside the player rather than being discarded.
- The mission board shows a short story briefing and previews the guaranteed legendary reward for each chapter.
- Regional patrol and champion defeats now advance both the optional bounty contract and the active story quest when their region and enemy type match.
- The quest board currently uses Unity's immediate-mode GUI as a functional prototype. It still needs a polished mobile UI, authored NPC model/animation, voiced dialogue and narrative cutscenes.

## Mobile target cycling and mission officer interaction

- Added a dedicated **TARGET** touch button to the shared mobile combat controls. It cycles through nearby valid enemies using the existing target-lock system, so Android players do not need to rely on a desktop-style GUI button.
- The mission officer board now has a persistent **TALK TO MISSION OFFICER** prompt while the player remains in the interaction area. Closing the board no longer traps the player in a state where they must leave and re-enter the area to reopen it.
- These UI changes are static-code checked; device layout and touch behavior still need Unity playtesting.

## Boss attack telegraphs

Boss wind-up attacks now display a pulsing ground ring before impact. The ring radius distinguishes the normal slam from the phase-two special attack, and its color reflects the boss profile. The indicator is hidden when the attack resolves, the boss is stunned, or the boss dies. This is a runtime-generated LineRenderer effect with no external asset dependency.

## Enemy hit reactions

Standard enemies now react visibly when damaged. Their renderers briefly flash using a shared `MaterialPropertyBlock` rather than creating new materials, then restore their original material colors. Repeated hits restart the short flash sequence. This is a lightweight first-pass combat feedback effect; final authored hit animations and impact VFX are still planned.

## Target-lock combat integration

The mobile target-lock system can cycle nearby enemies and display the selected target with its health bar. Lightning Style now prioritizes that manually selected enemy when it is inside the skill's valid search area and forward-facing arc; otherwise it falls back to the nearest eligible enemy. This keeps the lock useful without allowing attacks through the full map or behind the player.

## Mobile rendering and effect cleanup

Enemy hit flashes now write both URP Lit (`_BaseColor`) and legacy (`_Color`) shader properties through `MaterialPropertyBlock`, preserving each renderer's original supported colors. Boss attack telegraphs keep ownership of their generated material and release it when the boss object is destroyed; telegraphs also fail safely if the expected shader is unavailable.

## Android first-test build

The repository now includes an automated first-test scene and APK workflow for Unity 2022.3.62f1.

1. Install **Unity 2022.3.62f1** through Unity Hub with **Android Build Support**, including Android SDK & NDK Tools and OpenJDK.
2. Open this repository as a Unity project and wait for package import and script compilation to finish.
3. In the Unity Editor, run **Tools → Shinobi Legacy → Prepare Android First Test**. This generates the Safe Village and Prototype Arena scenes and places Safe Village first in Build Settings.
4. Run **Tools → Shinobi Legacy → Validate Android First Test**. The validator opens both generated scenes and checks for the player controller, combat, mobile controls, joystick, EventSystem, UI canvas, isometric camera, arena boss and required Build Settings entries. Fix any reported errors before continuing.
5. Switch the active build platform to **Android** in File → Build Settings if Unity requests it, then wait for the platform switch/import to finish.
6. Run **Tools → Shinobi Legacy → Build Android First Test APK**.
7. The development APK is written to `Builds/Android/SHINOBI_LEGACY_FirstTest.apk`. In the running test build, the **COMBAT TEST** button switches from the village to the prototype arena, and **VILLAGE HUB** returns to the village.

### Build the APK through GitHub Actions

A cloud build workflow is available at `.github/workflows/android-first-test.yml`. It runs when game source, packages or project settings change on `main`, and it can also be started manually from the repository's **Actions** tab.

The workflow uses GameCI with the exact Unity Editor version in this project. It regenerates the two test scenes, runs the same project validation, builds an ARM64 development APK and uploads it as the artifact **SHINOBI-LEGACY-Android-FirstTest**.

**One-time Unity Personal license setup is required:**

1. Install Unity Hub on a computer, sign in to the Unity account you will use for the build, then open **Unity Hub → Preferences/Settings → Licenses → Add → Get a free Personal license**. Complete activation and confirm that a `.ulf` license file has been created. GameCI's current guide lists the usual file locations and activation details: https://game.ci/docs/github/activation/
2. In this GitHub repository, open **Settings → Secrets and variables → Actions** and create these three repository secrets:
   - `UNITY_LICENSE` — the complete contents of the `.ulf` license file.
   - `UNITY_EMAIL` — the email address of the Unity account.
   - `UNITY_PASSWORD` — the password for that Unity account.
3. Never put these values in source files, commits, issues or chat. Keep them only in GitHub Actions secrets.
4. After copying the license file, sign out of Unity Hub on the local computer as described by GameCI so the Personal license seat is released for the build runner.
5. Open **Actions → Android first-test APK → Run workflow**. After a successful run, download the APK from the workflow run's **Artifacts** section.

The workflow checks that all three secrets are present. If one or more are missing, it deliberately skips the Unity build and shows a notice; it does not report a fake successful APK build.


Development APKs also show a small **SHINOBI LEGACY - PERFORMANCE** overlay. It reports the current sampled FPS, rolling 10-second average and minimum, approximate frame time, Unity allocated memory, device model, GPU and screen resolution. Tap the minus control to hide it, or **SHOW FPS** to reopen it. The overlay is diagnostic only; record performance with it visible and repeat a short comparison with it hidden if you suspect measurement overhead. It is automatically excluded from non-development builds.\n\nThe first-test player is configured for landscape, ARM64, Android API 26+, automatic installed target SDK selection, Gamma color space, IL2CPP, incremental garbage collection and Android optimized frame pacing. Incremental GC spreads collection work over smaller slices, while optimized frame pacing aims to distribute frames more evenly; neither guarantees a fixed FPS on a device. The runtime target remains 30 FPS with shadows disabled. This is a development test build, not a store release.

The validation step also checks that Android Build Support is installed in the Editor, both scenes have no missing-script components, the main camera is orthographic, and the EventSystem has its legacy Standalone Input Module for the current touch UI.

**Build limitation:** the repository changes can be prepared here, but an actual APK cannot be claimed until Unity's Android toolchain compiles the project successfully. Check the Unity Console and Editor.log for any compile or build errors. The APK must then be installed and tested on a physical Android device; stable 30 FPS is a target, not yet a measured result.

## First Android test: device QA checklist

Use this checklist after installing `SHINOBI_LEGACY_FirstTest.apk`. The current test scenes are generated from editor scripts, so run the preparation menu before every clean build.

### Installation and launch
- [ ] Install the APK on an ARM64 Android device running Android 8.0 (API 26) or newer.
- [ ] Confirm the app opens in landscape and starts in Safe Village.
- [ ] Confirm there is no immediate crash, black screen, missing UI or stuck loading state.
- [ ] Walk around the village with the left virtual joystick; check that the isometric camera follows the player smoothly.
- [ ] Check the village interaction areas: vendor stalls and Mission Officer.

### Combat scene
- [ ] Tap **COMBAT TEST** and confirm Prototype Arena loads.
- [ ] Move with the left joystick; test **ATTACK**, **CHAKRA BURST** and **FIRE STYLE**.
- [ ] Test **LIGHT**, **WATER** and **WIND** when those controls are visible in the mobile ability panel.
- [ ] Check enemy damage, hit reactions, enemy attacks, player health and death/respawn.
- [ ] Approach the boss and verify its ground telegraph appears before the slam.
- [ ] Tap **VILLAGE HUB** and confirm return travel works.

### Stability and performance
- [ ] Play continuously for at least 20 minutes; then repeat for 30 minutes.
- [ ] Watch for overheating, frame drops, input delay, memory-related exits and visual corruption.
- [ ] Confirm the target is stable 30 FPS; record actual performance rather than assuming the target was achieved.
- [ ] Test Android Back behavior, app pause/resume and relaunch; confirm saved character progress and inventory persist.

### Report results
Record device model, Android version, build date, test duration, observed FPS (if available), temperature/overheating symptoms, crashes and the exact reproduction steps for each issue. A failed checklist item should be fixed before treating the build as a usable test milestone.

**Current limitation:** this checklist is a test plan, not a passed test report. No physical-device results are claimed until the APK has been built, installed and exercised on hardware.

## Mobile joystick input safety

- The virtual joystick now locks to the pointer that began the touch, so a second finger cannot unexpectedly take over movement.
- Movement input and the joystick handle reset on pointer release, application focus loss, app pause and component disable. This prevents a held movement direction from persisting after common mobile interruption events.
- This is a source-level input safeguard. Touch behavior still needs verification on a physical Android device.


## Mobile ability cooldown feedback

- The six combat buttons now show a radial cooldown mask and a live whole-second countdown while an ability recharges.
- Chakra abilities change their label tint when the player does not have enough chakra to cast them.
- Cooldown values are read from the combat system; the HUD refreshes ten times per second rather than every frame.
- The target-cycle button remains separate and does not display an ability cooldown.
- This is source-level UI work. The overlay sprite, readability, safe-area placement and touch behavior still require Unity Editor and physical-device validation.


## Combat HUD event and refresh safety

- The combat HUD now manages health/resource event subscriptions idempotently. Reconfiguring the HUD no longer attaches duplicate listeners, and listeners are detached when the component is disabled.
- The equipment component is cached instead of repeatedly looked up while composing status text.
- Health/chakra bars retain a responsive 0.15-second refresh interval, while the longer skill/status line refreshes at 0.5 seconds by default to reduce repeated string/UI work on mobile.
- These are source-level safeguards; Unity compilation, profiler measurements and device testing are still required.

## Focused basic attacks

When the player has an enemy selected with the mobile target-lock control, a basic attack now prioritizes that selected enemy if it is within melee reach. If the selected enemy is out of reach or no target is selected, the existing forward-area attack remains as a fallback. This keeps target lock useful without changing the area behavior of Chakra and elemental abilities.

Focused melee attacks also rotate the player toward the currently locked enemy before applying the strike, making target-lock attacks directionally consistent.

## Enemy hit-flash restoration fix

Fixed the enemy hit-flash color restoration guard to validate the two color arrays actually initialized by the AI component. This removes a stale field reference that could otherwise prevent the enemy visual restoration method from compiling.
