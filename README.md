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

## Boss encounter range handling

Boss windups are now cancelled if the player leaves the configured encounter detection range before the attack resolves. The warning telegraph is hidden and the boss returns to its normal or phase-two color, preventing a stale attack warning or delayed strike after the player disengages.

## Mobile HUD update throttling

Chakra regeneration notifications are throttled to a 0.15-second interval instead of firing every rendered frame. Ability casts still emit an immediate resource-change notification, so the HUD responds immediately when chakra is spent while avoiding unnecessary per-frame UI callbacks during passive regeneration.

## Enemy hit-flash efficiency

Enemy damage feedback now iterates renderer arrays by index and reuses the precomputed color-support flags directly. This removes a repeated linear `Array.IndexOf` lookup for each renderer during each hit-flash update while preserving MaterialPropertyBlock-based color changes (no per-hit material instance creation).

## Boss visual material efficiency

Boss color changes now use a cached `MaterialPropertyBlock` and the shader's supported `_BaseColor` or `_Color` property. The boss AI no longer accesses `Renderer.material` during combat, avoiding an unnecessary per-boss material instance while keeping phase, attack-warning, and stun color feedback.

## Mobile target-lock control cleanup

Target cycling is now provided only by the shared Canvas combat controls. The duplicate IMGUI `NEXT TARGET` button was removed from `EnemyTargetLockSystem.OnGUI`; the target marker and target health panel remain. This avoids showing two target-cycle controls on mobile and keeps interactive combat input in one UI system.

## Mobile ability control compile fix

The shared `MobileAbilityControls.CreateButton` method now makes cooldown and resource-state callbacks optional. This fixes the target-lock button call, which intentionally supplies only its label, position, action, and color, while all six combat ability buttons continue to provide cooldown and chakra callbacks.

## Player movement input safety

`PlayerMotor.SetMovementEnabled(false)` now clears the last virtual-joystick input immediately. The motor also clears external movement input when disabled and on component shutdown. This prevents a stale touch direction from being applied when movement is re-enabled after a dialogue, UI lock, pause, or other control transition.

### Mobile graphics preference consistency
- Mobile startup now reads the same saved 30/60 FPS and shadow preferences as the in-game Graphics Settings menu.
- Scene changes no longer force the mobile bootstrap back to 30 FPS and shadows OFF after the player has selected different settings.
- Default remains 30 FPS with shadows OFF for entry-level Android devices; user selections are retained through PlayerPrefs.

### Enemy AI death-state cleanup
- Enemies now drop their player target when the target has died, preventing pointless chase and attack updates.
- Enemy death stops any active hit-flash coroutine, restores renderer property colors, disables the CharacterController, and then completes the existing short death-removal delay.

### Boss target and interrupted-attack safeguards
- Boss AI now clears a missing or defeated target and hides its attack telegraph instead of continuing encounter logic against a dead player.
- Stun interruption also clears the special-attack windup state, preventing stale special-attack state from carrying into the next action.
- Slam and special attacks recheck that the player is alive before applying damage or control effects; boss death safely disables its CharacterController when present.

### Animator integration foundation
- Added an optional `CombatAnimationBridge` that connects gameplay events to an authored Unity Animator Controller through `MoveSpeed`, `IsAttacking`, `IsDead`, `Attack`, `Hit`, and `Ability` parameters.
- The bridge checks which parameter names and types actually exist before setting them, and safely does nothing when a character has no Animator or controller.
- Player movement and combat, combatant hit/death events, and enemy/boss movement and attacks now feed the bridge. This is an integration layer only: animation clips, rigged 3D character models, and Animator Controller assets still need to be authored and assigned.

- Animator attack state is now driven through a timed `IsAttacking` boolean (when that parameter exists) alongside the `Attack` trigger. The state automatically resets after the attack window and is cleared if the bridge is disabled or the character dies.

## Procedural shinobi character visual pass

- Added an Editor-side `ShinobiCharacterVisualBuilder` that assembles an original stylized shinobi from lightweight mesh parts: cloth torso, chest and shoulder armor, headband, face mask, forearm and shin guards, sandals and a sheathed katana. Rogue enemies and the Stone Guardian receive a distinct silhouette and material treatment.
- Both generated test scenes now hide the single-capsule placeholder renderer and build the multi-part visual rig under the existing gameplay root. Gameplay colliders and movement/combat components remain on the root, while decorative mesh-part colliders are removed.
- This is a procedural visual prototype made from Unity primitive meshes, not a final rigged character asset. It has no authored skinning, animation clips, textures or production-quality facial detail. The Animator bridge is ready for a later rigged model integration. Unity scene generation, compilation and Android performance still need Editor/device validation.

## Enemy visual archetypes

- The prototype arena's three training enemies now use three distinct original visual profiles: a crimson hooded rogue, a moss-toned forest raider and a violet storm fighter. Their head crests, shoulder details, sash colors and armor accents help distinguish silhouettes at the isometric camera distance.
- The Stone Guardian boss now has a broader torso, chest core, crown-like stone crests and a reinforced shoulder plate to make its boss silhouette easier to read.
- These profiles remain lightweight procedural blockout visuals built from Unity primitive meshes. They are not final character art, do not include rigged animation or authored textures, and require Unity Editor plus device validation.

## Procedural locomotion and combat motion fallback

- Added `ShinobiProceduralMotion` to the generated player, enemy and boss visual rigs. It drives a lightweight run cycle, alternating arm/leg swings, subtle idle/run body bob, attack/ability arm sweeps, hit recoil and a simple death pose.
- `CombatAnimationBridge` now forwards movement speed, attacks, hit reactions, abilities and death state to this procedural layer while continuing to support a future authored Animator Controller.
- Enemy and boss rigs now include arm transforms so their movement and attack motions are visible. Reviving a combatant clears the procedural death state.
- This is a code-driven fallback, not skeletal animation: the current meshes are not skinned, motion is intentionally simple, and Unity Editor compilation, scene playback, animation feel and Android performance remain unverified.

## Melee combat feedback and boss telegraph polish

- Basic attacks now show a compact forward swing effect when no target is locked, and a small impact burst/ring at a focused locked target when the strike connects.
- Melee effects use the existing lightweight runtime particle/line helpers and short lifetimes; they are prototype feedback, not final authored combat VFX.
- Boss slam and phase-two special telegraphs now intensify as the windup approaches: pulse frequency, line width and opacity increase. The actual attack radius and damage checks are unchanged.
- Unity compilation, in-editor combat validation and Android GPU/performance profiling are still required.


## Enemy combat archetypes

- Training arena enemies are assigned three distinct behaviors: Hunter (direct pursuit and balanced melee), Skirmisher (keeps distance, circles and performs short lateral dodges), and Brute (slower movement, heavier damage, longer attack preparation and cooldown).
- Enemy melee attacks now have a short windup and re-check target life and distance before applying damage, giving the player a reaction window to move away.
- AI cancels pending attacks and stops its movement animation when stunned, when its target dies or disappears, or when the target leaves detection range.
- These behaviors are an initial tuning pass. Actual difficulty, dodge readability, hit timing and mobile performance need validation in Unity Play Mode and on Android.

## Boss phase-two signature attacks

- The four guardian profiles now resolve their phase-two special differently: Ashen deals a heavy burst followed by a short burn, Verdant restores 10% of its own maximum health, Thunder deals moderate damage and applies a longer stun, and Drowned deals a heavier controlled hit with a shorter stun.
- Each effect is checked against the player's alive state before it is applied. The existing windup telegraph and encounter-range checks remain in place.
- These are distinct gameplay effects built on the current health/status system; profile tuning, readability, balance and actual combat timing still require Unity Play Mode and Android testing.

## Dodgeable boss phase-two specials

- Phase-two damaging specials now resolve against a close-range impact radius instead of hitting the player anywhere inside the boss's full detection/special range.
- The warning ring uses the same profile-specific radius as the final hit check: Ashen 4.6 units, Thunder 5.2 units, and Drowned 4.2 units. Thunder has the widest damaging zone; Drowned trades area for control pressure.
- Ashen's burn and Thunder/Drowned control effects are only applied when the player remains inside the impact zone when the windup ends. Moving clear of the warning area avoids the damaging special.
- Verdant retains its defensive self-heal special; its short warning radius communicates the local recovery action rather than a player damage zone.
- This is gameplay/source logic only. Telegraph readability, actual dodge timing, balance and collision behavior still need Unity Play Mode and Android device testing.


## Boss phase-two summoned raiders

- When the Stone Guardian enters phase two (55% health), it begins summoning up to two raiders after a short 2.5-second delay. Each spawn location displays a pulsing violet warning ring for 0.9 seconds before the raider appears. If a raider is defeated, a later wave can refill the empty slot; waves are spaced by 19 seconds.
- Raiders use the existing Hunter/Skirmisher enemy AI, target the player, have 85 health, and grant 45 XP with an 85% loot-drop chance. Their inactive scene template remains available even after the original training enemies are defeated.
- Summoning is bounded to two active raiders to limit mobile CPU and memory load. The summon logic only runs while the boss has a live target and is within its encounter range.
- This is source-level implementation. Spawn placement, enemy navigation, reward behavior, scene generation and performance still require Unity compilation and Play Mode/device testing.


## Heavy guardian attack motion

- Added a dedicated heavy-attack motion cue to the procedural character motion layer. The boss drives both arms into a stronger downward swing, adds a brief torso lean, and triggers the Animator attack event when an authored controller is present.
- Stone Guardian slam and phase-two special attacks now use this heavy-attack cue instead of the lighter generic attack/ability motion.
- The motion is deliberately lightweight and reuses the existing procedural rig transforms; it does not add meshes, particles or per-frame allocations.
- This remains a blockout animation pass. The motion timing and silhouette need Unity Play Mode review, and a final boss requires authored rigging, animation clips, impact VFX/audio and Android profiling.


## Guardian impact feedback

- Stone Guardian slam and phase-two specials now produce a compact two-layer ground shockwave and a profile-colored particle burst on impact.
- A short additive camera shake is applied only when the player remains inside a damaging impact zone; a successful dodge still shows the boss impact without shaking the camera.
- The shake is capped at a low intensity and short duration, and runs after the isometric follow camera so it does not permanently alter the follow offset.
- This is lightweight procedural feedback, not final authored VFX. Visual layering, camera comfort, Unity compilation and Redmi 13C performance still require Play Mode and device testing.


## Directional hit knockback

- Successful player skill hits now apply a short, low-force push away from the player to surviving enemies.
- Enemy melee attacks and Stone Guardian slam/phase-two damaging specials apply a stronger, brief push away from the attacker to the player.
- Knockback is handled by a small `CharacterController` component in `LateUpdate`, after normal movement/AI updates, with capped strength and duration. It does not apply to defeated targets or the Verdant boss heal.
- Existing procedural hit animation and enemy hit flash remain in place. The knockback is a first-pass gameplay reaction; collision feel, attack interruption and device behavior still need Unity Play Mode and Android testing.


## Enemy attack windup animation

- Enemy AI now signals a visible procedural windup when a melee attack begins. Hunter and Skirmisher use a shorter arm preparation; Brute uses a longer, heavier two-arm preparation and torso lean.
- The windup is cancelled when the enemy is stunned, loses its target, or the target leaves detection range. The attack still rechecks target life and distance at resolution, so the animation does not guarantee a hit.
- This is a lightweight transform-based fallback for the current blockout rigs. Authored skeletal animation clips and in-game timing validation remain future work.


## Light and heavy hit reactions

- The procedural character motion now distinguishes ordinary hits from heavier impacts. Light hits use a short, restrained torso recoil; knockbacks with strength 2.3 or higher trigger a stronger body lean and arm recoil.
- The reaction is routed through the existing animation bridge and reuses the current knockback strength, with no additional rigidbody simulation or per-frame allocations.
- These are placeholder transform motions. Final hit-stun timing, animation clips and impact feel still require Unity Play Mode and Android device testing.


## Enemy impact stagger

- Strong knockbacks now apply a short stagger window to the affected combatant. The duration scales modestly with knockback strength and is capped at 0.34 seconds.
- Standard enemy AI pauses movement and cancels any pending melee wind-up while staggered, then resumes normal behavior automatically. Player and boss controllers do not consume this enemy-only AI pause.
- This adds a brief hit-confirmation window without introducing physics ragdolls or long crowd-control locks. Timing and combat balance still require Unity Play Mode and Android device testing.


## Visible enemy stagger pose

- The enemy AI now forwards its stagger state to the procedural motion layer. During the short impact pause, the enemy holds a distinct arm-and-torso recoil pose instead of only freezing its movement.
- The pose is driven by the existing rig transforms and is cleared automatically when the stagger window ends. It adds no physics bodies, animation assets or per-frame allocations.
- This remains a blockout motion cue; pose readability and transition timing need Unity Play Mode and Android validation.


## Collider-aware melee hit validation

- Basic melee attacks now measure reach against the closest point on the target's collider instead of relying only on the target transform pivot. This gives large enemies and the guardian a more representative strike distance.
- The melee check also rejects targets with an excessive vertical separation and keeps the forward-arc and obstruction checks, so solid walls and arena props can still block a strike.
- Focused target-lock attacks retain their turn-to-target behavior, but validate range and line of sight against the collider shape before applying damage.
- This is a source-level collision improvement. Collider setup, actual hit feel, Unity compilation and Android device testing still need validation.

## Dodgeable enemy melee strikes

- Standard enemy melee attacks now validate the player's closest collider point at the end of the windup, rather than checking only the player transform pivot.
- A strike is rejected when the player has moved beyond melee reach, has excessive vertical separation, or has stepped outside the enemy's forward attack arc.
- The enemy commits to its facing direction when the windup begins instead of continuously turning to track the player. This gives lateral movement a real chance to evade the committed strike.
- A non-allocating raycast check ignores the attacking enemy's own colliders while allowing walls and other solid objects to obstruct the hit.
- This is source-level combat logic. Collision layers, actual dodge timing, Unity compilation, Play Mode and Android device behavior still require testing.

## Guardian impact collision validation

- Stone Guardian slam and phase-two special attacks now measure the impact radius against the closest point on the player's collider, with a vertical-separation limit, instead of checking only the player transform pivot.
- The final hit check performs a non-allocating raycast toward that collider point. The boss's own colliders are ignored, while a nearer solid obstruction can block the impact.
- The slam and profile-specific special still use their existing visible warning radii; the final damage decision now uses the same radius with collider-aware validation.
- This is source-level collision logic. Collider hierarchy, raycast layers, edge cases, Unity compilation, Play Mode and Android device behavior still require testing.

## Directional enemy melee warning telegraphs

- Standard enemies now display a ground-level directional arc during their melee wind-up. The warning follows the committed facing direction and spans the same forward attack sector used by the strike validation.
- The warning radius matches the melee reach check, including its small collider tolerance. It pulses and becomes more prominent as the wind-up approaches completion.
- Brutes use a stronger red warning and heavier line treatment; Hunter and Skirmisher attacks use an amber warning.
- The telegraph is hidden when the strike resolves, the wind-up is cancelled, or the enemy dies. Its runtime material is released when the AI component is destroyed.
- This is a lightweight LineRenderer indicator. Ground clipping, visual readability, shader availability, Unity compilation, Play Mode timing and Android performance still require validation.


## Player directional dodge

- Added a dedicated mobile dodge action. It uses the current virtual-joystick direction relative to the isometric camera; with no movement input, it dodges forward.
- The evade applies a short CharacterController dash after the normal motor update, has a 1.25-second cooldown, and grants 0.18 seconds of temporary damage immunity through the existing health damage gate.
- The mobile HUD creates a DODGE button with a cooldown overlay when the player has the dodge component. The button is placed beside the target-cycle control.
- Dodge is intended to escape committed enemy melee arcs and guardian impact zones through movement and timing. Cooldown, dash distance, immunity window, collision behavior and touch layout still require Unity Play Mode and Android device testing.
