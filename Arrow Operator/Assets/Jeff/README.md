# Jeff: 3D arrow powerups

This replaces the old XY sandbox. The new demo uses Arthur's `ArrowController`, a dynamic
3D Rigidbody, continuous collision detection, a perspective camera and `CameraFollow`.
Open the team project in **Unity 6000.6.3f1**, then open `Assets/Scenes/Jeff_Scene.unity`.
The scene builds its disposable 3D test corridor when Play starts.

## Demo controls

- Hold Space to fly along the arrow's local +Z direction. Release it to stop immediately.
- WASD / arrow keys steer along local X and Y while flying. The team's existing fixed-heading
  flight model is retained; this does not introduce a different yaw/pitch flight system.
- Fly straight through cyan (speed), blue (forward boost), violet (shield), then gold (+10s).
- The red wall demonstrates shield protection and optional reflection. Steer around it to continue.
- R rebuilds the demo, clears effects, resets the timer and respawns pickups.
- Materials and geometry are simple sci-fi blockouts, not finished art.

## Add to the team's 3D scene

1. Keep exactly one `ArrowController` and one dynamic Rigidbody on the arrow root.
   Add `ArrowPowerups` on that same root in the Inspector, and assign the scene's `CountdownTimer`.
   Child colliders are supported. Do not also attach Clark's `TestPlayer`.
2. Add `PowerupPickup` to each pickup collider. Its collider becomes a trigger at runtime.
   Select Speed, DirectionalSpeed, Shield or ExtraTime. Visual child meshes can be replaced freely.
3. For directional pickups, rotate the pickup's +Z axis toward the world-space boost direction,
   or assign a separate `directionSource`. The direction is captured on collection.
4. Use `ArrowHazard` on damaging walls. Solid colliders support physical blocking and optional
   reflection; trigger hazards support protection and failure without physical reflection.
   Do not put Clark's `Obstacle` on the same hazard: that script does not know about these powerups.
5. Assign the existing game-over panel and/or `onCrash` event on `ArrowHazard` as needed.
   An unprotected contact disables the arrow motor and pauses the round. Scene reload resets it.
6. Enable `requireBreath` for the flute controller. Existing scenes keep automatic forward flight
   by default. Keyboard input uses Space for breath. A hardware adapter can set `useKeyboardInput`
   to false and call `SetInput(Vector2 steering, bool breath)` each frame.

## Effect rules

- Speed: multiplies the complete 3D velocity. Default 1.8x for 8 seconds.
- Direction: multiplies only the positive velocity component along the selected world direction.
  Example: an upward boost affects Y without also accelerating forward Z travel.
- Different speed effects combine, with the final magnitude capped at 4x the base velocity.
  Same-kind pickups replace strength and refresh duration instead of stacking without limit.
- Shield: protection for the entire duration, not a single charge. Reflection is optional and
  disabled by default; the demo enables it. Solid-contact normals drive the 3D reflection.
  Contact is rechecked while overlapping so an expired shield cannot keep the arrow invulnerable.
- Extra time: +10 seconds on a running timer, capped at 3600 seconds. Missing/expired timers
  and paused rounds reject the pickup without consuming it.
- Effects use scaled time. Pause freezes them. Disabling `ArrowPowerups` clears all effects.
- Pickups can be pooled: disable then re-enable to make them collectible again.

## Validation

`PowerupPlayChecks` runs the real 3D motor and physics callbacks, including XYZ movement,
breath stopping, rotated-direction boosts, effect expiry, caps, actual trigger pickups,
pool reuse, timer failures, pause, solid-wall shield reflection and unprotected crashes.
It is editor-only and does not enter player builds.

Run against an isolated test project using Unity's command line:
`-batchmode -nographics -projectPath <path> -executeMethod ArrowOperator.Jeff.PowerupPlayChecks.RunBatch -logFile <path>`
The process logs `JEFF_3D_PASS` or `JEFF_3D_FAIL` and exits with code 0 or 1.

The local installation is Unity 6000.3.7f1. Validation on that version uses a separate project;
On 2026-10-08, compilation and all 50 play-mode assertions passed with Input System 1.18.0
and uGUI 2.0.0. Unity also logged a startup search-index exception inside UnityEditor.Search;
the gameplay checks completed successfully after that editor-only message.
The team's Unity and package versions remain unchanged. Exact-version playtesting and physical
Makey Makey hardware testing are still required before final integration.

Art sources remain in `ArtReferences/Jeff`. No unrelated scene or main-menu wiring is changed.
