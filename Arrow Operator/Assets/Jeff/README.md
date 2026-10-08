# Jeff: movement aids

Open this project with **Unity 6000.6.3f1**, as specified by ProjectVersion.txt.
Open `Assets/Scenes/Jeff_Scene.unity` and press Play. The attached
`JeffMovementAidDemo` creates the sandbox at runtime; objects disappear on exiting Play.

## Try it

- Hold **Space + WASD / arrow keys** to move. Space simulates the Makey Makey breath contact.
- Release Space to stop. This sandbox uses Clark's XY movement prototype, not a final 3D flight controller.
- Walk right through the four stations: cyan cube, blue diamond, violet orb, gold time pickup.
- Cyan: 1.8x speed for 8 seconds. Blue: up to another 1.8x while moving right (+X), for 8 seconds.
- Violet: 8 seconds of crash protection. Touch a coral wall while active to bounce away.
- Gold: +10 seconds. Gold discs across the top are Clark's score targets.
- The HUD shows remaining time, score, pickups, and effect durations. **R** resets everything.
- Without a shield, a wall stops the round. An expired timer also stops the round.

The direction rule, numerical values, and optional reflection are prototype decisions based on
the J-marked FigJam notes, not requirements already approved by the team.

## Integrate with another scene

1. Add `MovementAidController` to the Player root **before** `TestPlayer.Awake` runs.
   Assign the existing scene's `CountdownTimer` to its `timer` field.
2. Keep a collider and a Rigidbody on the player. For the existing transform-driven prototype,
   use a kinematic Rigidbody without gravity. Keep the Player tag on the collider GameObject.
3. Place `MovementAidPickup` on a trigger collider. Set its kind, duration, multiplier,
   extraSeconds, or world-space direction. The pickup looks for the controller on the touching
   collider's parent hierarchy. Missing timers do not consume time pickups.
4. Clark's `TestPlayer` now passes movement through `ModifyVelocity`. Without the controller,
   movement stays unchanged. `requireBreath` defaults to false in existing scenes.
5. Clark's `Obstacle` asks `TryProtect` before its normal death path, including while overlapping.
   `CountdownTimer.TryAddTime` accepts time only during an active round, with a 3600-second cap.
6. For a future 3D motor, pass its world-space base velocity to `ModifyVelocity` once per movement
   step and apply that result yourself. Check breath first and pass zero when inactive. Do not run
   TestPlayer at the same time. The current bounce adapter assumes one root collider; a custom
   motor with compound colliders or non-trigger collision needs its own contact-normal adapter.

Same-kind pickups replace strength and refresh duration. Different speed types multiply, capped
at 4x. Directional bonus fades with alignment; perpendicular/opposite travel receives none.
Effects use scaled game time and pause with `Time.timeScale`. Shield lasts for its full duration,
not one charge. Disable `bounceWithShield` for protection without reflection.

## Validation

Compilation and all 38 validator assertions passed on 2026-10-07 in an isolated Unity
6000.3.7f1 project using Input System 1.18.0 and uGUI 2.0.0. The team's project settings and
package versions remain unchanged. A full playtest in Unity 6000.6.3f1 is still pending.

Use **Arrow Operator > Jeff > Validate Movement Aids** (outside Play mode).
The validator checks duration/refresh, invalid values, direction alignment, caps, no-input motion,
time-pickup failure and deduplication, actual collider separation and reflected velocity.
For automation, invoke `ArrowOperator.Jeff.Editor.MovementAidValidation.RunBatch` using Unity's
`-batchmode -nographics -projectPath ... -executeMethod ... -logFile ...` flags.

Also play through all four pickups, bounce, expiry, death, timeout, pause, and restart in the
team's exact Unity version before merging. Hardware breath contact and the final 3D motor
remain integration checks. No change to the main menu or build scene list is included.

Reference: [J-marked project board](https://www.figma.com/board/kuDRB1OyUNp7UFiDbajD8A/EGAM353-Flute).
Physics reference: [Unity trigger callbacks](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/collider/ontriggerenter).

Art samples and their source/license notes are in the repository's `ArtReferences/Jeff` folder.
