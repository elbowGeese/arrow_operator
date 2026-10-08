#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArrowOperator.Jeff
{
    // Play-mode integration checks exercise the real motor and 3D physics callbacks.
    public sealed class PowerupPlayChecks : MonoBehaviour
    {
        const string BatchKey = "Jeff3DPowerupChecks";
        int passed;
        bool finished;
        float deadline;

        [MenuItem("Arrow Operator/Jeff/Open 3D Demo")]
        public static void OpenDemo()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene("Assets/Scenes/Jeff_Scene.unity");
        }

        public static void RunBatch()
        {
            SessionState.SetBool(BatchKey, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            if (!SessionState.GetBool(BatchKey, false)) return;
            SessionState.SetBool(BatchKey, false);
            new GameObject("3D Powerup Checks").AddComponent<PowerupPlayChecks>();
        }

        void Start() { deadline = Time.realtimeSinceStartup + 30f; StartCoroutine(Guard(Run())); }
        void Update() { if (!finished && Time.realtimeSinceStartup > deadline) Finish(false, "Timed out"); }

        IEnumerator Guard(IEnumerator routine)
        {
            while (true)
            {
                object step;
                try { if (!routine.MoveNext()) break; step = routine.Current; }
                catch (Exception error) { Finish(false, error.ToString()); yield break; }
                yield return step;
            }
            Finish(true, passed + " assertions");
        }

        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            passed++;
            Debug.Log("JEFF_3D_CHECK: " + message);
        }

        void Near(float a, float b, string message) => Check(Mathf.Abs(a - b) < 0.02f, message);

        IEnumerator Run()
        {
            Time.timeScale = 1f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = new Vector3(0, 4, 0);
            go.transform.localScale = Vector3.one * 0.5f;
            var body = go.AddComponent<Rigidbody>();
            var effects = go.AddComponent<ArrowPowerups>();
            var timer = new GameObject("Timer").AddComponent<CountdownTimer>(); timer.time = 90f; effects.timer = timer;
            var motor = go.AddComponent<ArrowController>(); motor.requireBreath = true; motor.useKeyboardInput = false; motor.acceleration = 1000f;
            yield return null;
            motor.SetInput(Vector2.one, false);
            Vector3 before = body.position;
            yield return new WaitForSeconds(0.12f);
            Check(Vector3.Distance(body.position, before) < 0.01f, "No breath stops the 3D body");
            motor.SetInput(Vector2.zero, true);
            yield return new WaitForSeconds(0.2f);
            Check(body.position.z > before.z + 1f, "Real motor flies along Z");
            motor.SetInput(Vector2.one, true); before = body.position;
            yield return new WaitForSeconds(0.2f);
            Check(body.position.x > before.x && body.position.y > before.y && body.position.z > before.z, "Steering changes X and Y while flying along Z");
            motor.SetInput(Vector2.zero, false);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Near(body.linearVelocity.magnitude, 0f, "Breath release stops immediately");

            effects.GiveSpeed(2f, 4f);
            effects.GiveDirection(Vector3.up, 2f, 8f);
            Vector3 boosted = effects.ModifyVelocity(new Vector3(3, 4, 5));
            Near(boosted.x, 6, "General speed affects X"); Near(boosted.y, 16, "Directional speed affects positive Y"); Near(boosted.z, 10, "Y boost does not amplify Z again");
            Near(effects.ModifyVelocity(Vector3.down).y, -2f, "Opposite direction receives no directional bonus");
            effects.Advance(4f); Near(effects.ModifyVelocity(Vector3.forward).z, 1, "Speed expires independently");
            effects.Advance(4f); Near(effects.ModifyVelocity(Vector3.up).y, 1, "Direction expires");
            Check(!effects.GiveSpeed(float.NaN, 1) && !effects.GiveShield(-1) && !effects.GiveDirection(Vector3.zero, 2, 1), "Reject invalid effect settings");
            effects.GiveSpeed(2, 3); effects.Advance(1); effects.GiveSpeed(2, 3);
            Near(effects.SpeedSeconds, 3, "Repeat pickup refreshes duration"); Near(effects.ModifyVelocity(Vector3.forward).z, 2, "Repeat pickup does not compound");
            effects.GiveSpeed(100, 3); effects.GiveDirection(Vector3.forward, 100, 3);
            Near(effects.ModifyVelocity(Vector3.forward).magnitude, 4, "Combined speed is capped");
            effects.Clear();

            foreach (PowerupKind kind in Enum.GetValues(typeof(PowerupKind)))
            {
                motor.SetInput(Vector2.zero, false);
                var pickupObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pickupObject.transform.position = body.position;
                var pickup = pickupObject.AddComponent<PowerupPickup>(); pickup.kind = kind;
                if (kind == PowerupKind.DirectionalSpeed) pickupObject.transform.rotation = Quaternion.LookRotation(Vector3.up);
                float timeBefore = timer.time;
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); yield return null;
                Check(!pickupObject.activeSelf, "Actual 3D trigger collects " + kind);
                Check(!pickup.TryCollect(effects), "Duplicate contact cannot recollect " + kind);
                if (kind == PowerupKind.ExtraTime) Check(timer.time > timeBefore + 9f, "Time pickup updates the real timer");
                if (kind == PowerupKind.DirectionalSpeed) Check(Vector3.Dot(effects.BoostDirection, Vector3.up) > 0.99f, "Pickup rotation defines world direction");
                pickupObject.transform.position += Vector3.right * 50;
                pickupObject.SetActive(true);
                Check(pickup.TryCollect(effects), "Pooled pickup can be collected again: " + kind);
                Destroy(pickupObject);
            }
            var timePickupObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            timePickupObject.transform.position = Vector3.one * 100;
            var timePickup = timePickupObject.AddComponent<PowerupPickup>(); timePickup.kind = PowerupKind.ExtraTime;
            effects.timer = null;
            Check(!timePickup.TryCollect(effects) && timePickupObject.activeSelf, "Missing timer does not consume pickup"); effects.timer = timer;
            timer.time = 3599; Check(effects.GiveTime(10), "Timer accepts valid time"); Near(timer.time, 3600, "Timer cap");
            timer.time = 0; Check(!effects.GiveTime(10), "Expired timer cannot be revived"); timer.time = 90;
            float shieldBefore = effects.ShieldSeconds;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(0.15f);
            Near(effects.ShieldSeconds, shieldBefore, "Pause freezes effects");
            Check(!effects.GiveTime(10), "Pause rejects time pickup"); Time.timeScale = 1;

            effects.Clear(); effects.GiveSpeed(2, 8); motor.SetInput(Vector2.zero, true);
            yield return new WaitForSeconds(0.15f);
            Near(body.linearVelocity.z, 20, "Speed effect changes actual Rigidbody velocity");
            effects.Clear(); effects.GiveDirection(Vector3.forward, 1.8f, 8);
            yield return new WaitForSeconds(0.15f);
            Near(body.linearVelocity.z, 18, "Forward directional boost changes actual Rigidbody velocity");
            effects.Clear(); effects.GiveShield(8); effects.bounceWithShield = true; effects.bounceDuration = 0.5f;
            motor.SetInput(Vector2.zero, false);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            body.position = new Vector3(0, 4, 20); body.linearVelocity = Vector3.zero;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0, 4, 22); wall.transform.localScale = new Vector3(8, 8, 0.1f);
            wall.AddComponent<ArrowHazard>();
            Physics.SyncTransforms(); motor.SetInput(Vector2.zero, true);
            float limit = Time.time + 1f; bool reflected = false;
            while (Time.time < limit) { yield return new WaitForFixedUpdate(); if (body.linearVelocity.z < -1f) { reflected = true; break; } }
            Check(reflected && motor.enabled && Time.timeScale > 0, "Real solid Z-wall collision reflects shielded arrow");
            Check(body.position.z < 22, "Continuous collision does not tunnel through the wall");
            foreach (Vector3 axis in new[] { Vector3.right, Vector3.up })
            {
                motor.SetInput(Vector2.zero, false);
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                effects.Clear(); effects.GiveShield(8);
                Vector3 origin = new Vector3(30, 30, 30);
                body.position = origin; body.rotation = Quaternion.FromToRotation(Vector3.forward, axis);
                body.linearVelocity = Vector3.zero;
                wall.transform.position = origin + axis * 2f;
                wall.transform.rotation = body.rotation;
                Physics.SyncTransforms(); motor.SetInput(Vector2.zero, true);
                limit = Time.time + 1f; reflected = false;
                while (Time.time < limit) { yield return new WaitForFixedUpdate(); if (Vector3.Dot(body.linearVelocity, axis) < -1f) { reflected = true; break; } }
                Check(reflected && motor.enabled && Time.timeScale > 0, "Rotated 3D flight and shield reflection along " + axis);
            }
            motor.SetInput(Vector2.zero, false);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            body.rotation = Quaternion.identity;
            wall.transform.SetPositionAndRotation(new Vector3(0, 4, 22), Quaternion.identity);
            effects.Clear(); motor.SetInput(Vector2.zero, false);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            body.position = new Vector3(0, 4, 20); body.linearVelocity = Vector3.zero; Physics.SyncTransforms();
            motor.SetInput(Vector2.zero, true);
            yield return new WaitForSecondsRealtime(0.5f);
            Check(!motor.enabled && Time.timeScale == 0, "Unshielded 3D collision ends the round");
            Time.timeScale = 1;
            wall.SetActive(false); wall.SetActive(true);
            motor.enabled = true; effects.GiveShield(0.4f); effects.bounceWithShield = false;
            body.position = new Vector3(0, 4, 21); body.linearVelocity = Vector3.zero;
            Physics.SyncTransforms(); motor.SetInput(Vector2.zero, true);
            yield return new WaitForSecondsRealtime(0.2f);
            Check(motor.enabled && Time.timeScale > 0, "Protection without reflection survives solid contact");
            yield return new WaitForSecondsRealtime(0.5f);
            Check(!motor.enabled && Time.timeScale == 0, "Shield expiry during continued solid contact ends round");
            Time.timeScale = 1;
            Destroy(wall); Destroy(go); Destroy(timer.gameObject); Destroy(timePickupObject);
            yield return null;
            var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
            var demo = camera.gameObject.AddComponent<Arrow3DDemo>();
            yield return null; yield return null;
            var demoArrow = FindFirstObjectByType<ArrowController>();
            Check(demoArrow != null && !demoArrow.transform.IsChildOf(camera.transform), "Demo arena is independent of the moving camera");
            Check(!camera.orthographic && camera.GetComponent<CameraFollow>().target == demoArrow.transform, "Demo uses perspective and the team's follow camera");
            demoArrow.useKeyboardInput = false; demoArrow.SetInput(Vector2.zero, true);
            Vector3 cameraBefore = camera.transform.position;
            yield return new WaitForSeconds(0.3f);
            Check(camera.transform.position.z > cameraBefore.z, "Demo camera follows forward flight");
            demoArrow.GetComponent<ArrowPowerups>().GiveShield(8);
            demo.ResetDemo(); yield return null; yield return null;
            demoArrow = FindFirstObjectByType<ArrowController>();
            Check(demoArrow != null && !demoArrow.GetComponent<ArrowPowerups>().HasShield, "Demo restart clears effects");
            Check(FindObjectsByType<PowerupPickup>(FindObjectsSortMode.None).Length == 4, "Demo restart restores exactly four pickups");
            Check(FindObjectsByType<ArrowController>(FindObjectsSortMode.None).Length == 1, "Demo restart leaves exactly one arrow");
            Check(demoArrow.GetComponent<ArrowPowerups>().timer.time > 119, "Demo restart resets timer");
        }

        void Finish(bool success, string message)
        {
            if (finished) return; finished = true; Time.timeScale = 1;
            Debug.Log((success ? "JEFF_3D_PASS: " : "JEFF_3D_FAIL: ") + message);
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
#endif
