using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArrowOperator.Jeff
{
    public sealed class Arrow3DDemo : MonoBehaviour
    {
        readonly List<Material> materials = new List<Material>();
        ArrowPowerups effects;
        CountdownTimer timer;
        Material playerMaterial;
        Transform arena;
        bool crashed;

        void Start()
        {
            arena = new GameObject("3D Powerup Lab").transform;
            var floor = MakeMaterial("Floor", new Color(0.06f, 0.1f, 0.17f));
            var wall = MakeMaterial("Panels", new Color(0.16f, 0.25f, 0.35f));
            var danger = MakeMaterial("Hazards", new Color(1f, 0.24f, 0.35f));
            playerMaterial = MakeMaterial("Arrow", new Color(0.9f, 0.96f, 1f));
            Box("Floor", new Vector3(0, -0.5f, 75), new Vector3(18, 1, 170), floor);
            Box("Left wall", new Vector3(-9, 4, 75), new Vector3(1, 9, 170), wall);
            Box("Right wall", new Vector3(9, 4, 75), new Vector3(1, 9, 170), wall);
            for (int z = 0; z <= 150; z += 10)
            {
                Box("Panel stripe", new Vector3(0, 0.015f, z), new Vector3(17, 0.02f, 0.12f), wall);
                Box("Support", new Vector3(-8, 4, z), new Vector3(0.35f, 8, 0.35f), wall);
            }
            Vector3 spawn = new Vector3(0, 2, 0);
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Arrow - 3D Rigidbody"; player.transform.SetParent(arena); player.transform.position = spawn;
            Destroy(player.GetComponent<Renderer>());
            var collider = player.GetComponent<CapsuleCollider>();
            collider.direction = 2; collider.radius = 0.22f; collider.height = 1.3f;
            player.AddComponent<Rigidbody>();
            effects = player.AddComponent<ArrowPowerups>(); effects.bounceWithShield = true;
            timer = new GameObject("Demo Timer").AddComponent<CountdownTimer>();
            timer.transform.SetParent(arena); timer.time = 120f; effects.timer = timer;
            var arrow = player.AddComponent<ArrowController>(); arrow.requireBreath = true;
            GameObject shaft = Box("Arrow shaft", spawn, new Vector3(0.13f, 0.13f, 0.9f), playerMaterial);
            Destroy(shaft.GetComponent<Collider>()); shaft.transform.SetParent(player.transform, true);
            GameObject tip = Box("Arrow tip", spawn + Vector3.forward * 0.5f, new Vector3(0.3f, 0.3f, 0.3f), playerMaterial);
            Destroy(tip.GetComponent<Collider>()); tip.transform.rotation = Quaternion.Euler(45, 0, 45); tip.transform.SetParent(player.transform, true);
            var colors = new[] { Color.cyan, new Color(0.3f, 0.6f, 1), new Color(0.7f, 0.5f, 1), new Color(1, 0.8f, 0.3f) };
            for (int i = 0; i < 4; i++)
            {
                PrimitiveType shape = i == 2 ? PrimitiveType.Sphere : i == 3 ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                GameObject go = Box(((PowerupKind)i).ToString(), new Vector3(0, 2, 10 + i * 12), Vector3.one, MakeMaterial("Pickup " + i, colors[i]), shape);
                if (i == 1) go.transform.rotation = Quaternion.Euler(0, 0, 45);
                if (i == 3) go.transform.localScale = new Vector3(1, 0.2f, 1);
                go.AddComponent<PowerupPickup>().kind = (PowerupKind)i;
            }
            GameObject obstacle = Box("Shield test wall", new Vector3(0, 2, 57), new Vector3(4, 4, 0.5f), danger);
            obstacle.AddComponent<ArrowHazard>().onCrash.AddListener(() => crashed = true);
            GameObject end = Box("End wall", new Vector3(0, 4, 152), new Vector3(18, 9, 1), danger);
            end.AddComponent<ArrowHazard>().onCrash.AddListener(() => crashed = true);
            Camera camera = Camera.main;
            if (camera == null) { camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera"; }
            camera.orthographic = false; camera.fieldOfView = 65f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.025f, 0.045f, 0.08f);
            camera.transform.SetPositionAndRotation(spawn + new Vector3(0, 1.5f, -5), Quaternion.identity);
            var follow = camera.GetComponent<CameraFollow>();
            if (follow == null) follow = camera.gameObject.AddComponent<CameraFollow>();
            follow.target = player.transform; follow.offset = new Vector3(0, 1.5f, -5);
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                ResetDemo();
            if (effects != null) playerMaterial.color = effects.HasShield ? new Color(0.7f, 0.5f, 1) : new Color(0.9f, 0.96f, 1);
        }

        public void ResetDemo()
        {
            if (arena != null) { arena.gameObject.SetActive(false); Destroy(arena.gameObject); }
            foreach (Material material in materials) if (material != null) Destroy(material);
            materials.Clear();
            Time.timeScale = 1f;
            crashed = false;
            Start();
        }

        void OnGUI()
        {
            if (timer == null) return;
            float scale = Mathf.Clamp(Screen.width / 1280f, 0.65f, 2f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            GUI.Box(new Rect(18, 18, 670, 178), "ARROW OPERATOR / 3D POWERUP LAB");
            GUI.Label(new Rect(32, 48, 650, 24), "Hold SPACE to fly forward. WASD / arrows steer vertically and sideways. R restarts.");
            GUI.Label(new Rect(32, 76, 650, 24), "Cyan: speed | Blue: forward boost | Violet: shield | Gold: +10 seconds");
            GUI.Label(new Rect(32, 104, 650, 24), $"Time: {timer.time:F1}   Speed: {effects.SpeedSeconds:F1}s   Direction: {effects.DirectionSeconds:F1}s   Shield: {effects.ShieldSeconds:F1}s");
            GUI.Label(new Rect(32, 132, 650, 24), "Collect the four pickups, then touch the red wall. Steer around it to continue.");
            GUI.Label(new Rect(32, 160, 650, 24), crashed ? "CRASH - press R to retry" : timer.time <= 0 ? "TIME UP - press R to retry" : "Perspective camera / XYZ physics / team ArrowController");
            GUI.matrix = previous;
        }

        Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = name, color = color }; materials.Add(material); return material;
        }

        GameObject Box(string name, Vector3 position, Vector3 scale, Material material, PrimitiveType shape = PrimitiveType.Cube)
        {
            GameObject go = GameObject.CreatePrimitive(shape); go.name = name; go.transform.SetParent(arena);
            go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
            if (arena != null) Destroy(arena.gameObject);
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}
