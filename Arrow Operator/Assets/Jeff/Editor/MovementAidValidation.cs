using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArrowOperator.Jeff.Editor
{
    // No additional test packages required. Tests the same runtime components used by the scene.
    public static class MovementAidValidation
    {
        [MenuItem("Arrow Operator/Jeff/Open Demo")]
        public static void OpenDemo()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/Jeff_Scene.unity");
        }

        static int assertions;
        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception("Movement aids: " + message);
            assertions++;
        }
        static void Near(float actual, float expected, string message) => Check(Mathf.Abs(actual - expected) < 0.001f, message);

        [MenuItem("Arrow Operator/Jeff/Validate Movement Aids")]
        public static void Run()
        {
            assertions = 0;
            float previousTimeScale = Time.timeScale;
            var objects = new List<GameObject>();
            try
            {
                Time.timeScale = 1f;
                var state = new MovementAidState();
                Check(state.GrantSpeed(2f, 4f), "speed grant");
                Check(state.GrantDirection(2f, 8f), "direction grant");
                Near(state.GetMultiplier(1f), 4f, "aligned effects combine");
                Near(state.GetMultiplier(0f), 2f, "perpendicular direction has no bonus");
                Near(state.GetMultiplier(-1f), 2f, "opposite direction has no bonus");
                Near(state.GetMultiplier(0.5f), 3f, "partial alignment scales bonus");
                state.Tick(4f);
                Near(state.GetMultiplier(1f), 2f, "speed expires independently");
                Check(state.GrantShield(2f), "shield grant");
                state.Tick(0f);
                Near(state.ShieldSeconds, 2f, "pause preserves duration");
                state.Tick(2f);
                Check(!state.HasShield, "shield expiry");
                state.Tick(100f);
                Near(state.GetMultiplier(1f), 1f, "all boosts expire");
                Check(state.GrantSpeed(2f, 3f) && state.GrantSpeed(2f, 3f), "repeat pickup accepted");
                Near(state.GetMultiplier(1f), 2f, "repeat pickup does not compound");
                Near(state.SpeedSeconds, 3f, "repeat pickup refreshes duration");
                Check(!state.GrantSpeed(float.NaN, 1f), "reject NaN");
                Check(!state.GrantShield(float.PositiveInfinity), "reject infinite duration");
                Check(!state.GrantDirection(2f, -1f), "reject negative duration");

                GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                objects.Add(playerObject);
                playerObject.transform.position = new Vector3(1000, 1000, 1000);
                var player = playerObject.AddComponent<MovementAidController>();
                Check(!player.GrantDirection(Vector3.zero, 2f, 5f), "reject zero direction");
                player.GrantSpeed(2f, 5f);
                player.GrantDirection(Vector3.right, 2f, 5f);
                Near(player.ModifyVelocity(Vector3.right * 5f).magnitude, 20f, "3D velocity boost");
                Near(player.ModifyVelocity(Vector3.forward * 5f).magnitude, 10f, "Z movement unaffected by X bonus");
                Near(player.ModifyVelocity(Vector3.zero).magnitude, 0f, "no movement without input");
                player.GrantSpeed(10f, 5f);
                Near(player.ModifyVelocity(Vector3.right * 5f).magnitude, 20f, "speed cap");

                GameObject timerObject = new GameObject("Validation Timer");
                objects.Add(timerObject);
                var timer = timerObject.AddComponent<CountdownTimer>();
                timer.time = 30f;
                player.timer = timer;
                Check(player.GrantTime(10f), "add time to running timer");
                Near(timer.time, 40f, "time added");
                Time.timeScale = 0f;
                Check(!player.GrantTime(10f), "cannot add time after pause/game over");
                Time.timeScale = 1f;
                timer.time = 0f;
                Check(!player.GrantTime(10f), "cannot revive expired timer");
                timer.time = 3599f;
                Check(player.GrantTime(10f), "time cap grant");
                Near(timer.time, 3600f, "time cap");

                GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                objects.Add(pickupObject);
                pickupObject.transform.position = new Vector3(2000, 2000, 2000);
                var pickup = pickupObject.AddComponent<MovementAidPickup>();
                pickup.kind = MovementAidKind.ExtraTime;
                player.timer = null;
                Check(!pickup.TryCollect(player) && pickupObject.activeSelf, "missing timer leaves pickup available");
                player.timer = timer;
                timer.time = 30f;
                Check(pickup.TryCollect(player), "valid pickup collected");
                Check(!pickup.TryCollect(player), "duplicate collider cannot collect twice");
                Near(timer.time, 40f, "one pickup adds time once");

                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                objects.Add(wall);
                wall.transform.position = playerObject.transform.position + Vector3.right * 0.75f;
                Physics.SyncTransforms();
                Check(!player.TryProtect(wall.GetComponent<Collider>()), "unshielded hazard falls through to normal death");
                player.GrantShield(3f);
                player.ModifyVelocity(Vector3.right * 5f);
                Check(player.TryProtect(wall.GetComponent<Collider>()), "shield protects");
                Check(player.ModifyVelocity(Vector3.right * 5f).x < 0f, "shield reflects incoming movement");
                Physics.SyncTransforms();
                Check(!Physics.ComputePenetration(playerObject.GetComponent<Collider>(), playerObject.transform.position,
                    playerObject.transform.rotation, wall.GetComponent<Collider>(), wall.transform.position,
                    wall.transform.rotation, out _, out _), "bounce exits wall overlap");
                Near(player.ModifyVelocity(Vector3.zero).magnitude, 0f, "release breath cancels movement even during bounce");
                player.State.Tick(3f);
                Check(!player.TryProtect(wall.GetComponent<Collider>()), "expired shield no longer protects");
                Debug.Log($"JEFF_VALIDATION_PASS: {assertions} assertions");
            }
            finally
            {
                foreach (GameObject go in objects) if (go != null) UnityEngine.Object.DestroyImmediate(go);
                Time.timeScale = previousTimeScale;
            }
        }

        public static void RunBatch()
        {
            try { Run(); EditorApplication.Exit(0); }
            catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
        }
    }
}
