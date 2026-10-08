using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArrowOperator.Jeff
{
    // Attached only to Jeff_Scene. Builds a disposable sandbox using the team's real gameplay scripts.
    public sealed class JeffMovementAidDemo : MonoBehaviour
    {
        readonly List<Material> materials = new List<Material>();
        readonly List<MovementAidPickup> pickups = new List<MovementAidPickup>();
        Transform arena;
        MovementAidController player;
        CountdownTimer timer;
        ScoreManager score;
        Material playerMaterial;
        Material cyan, blue, violet, gold, red, panel;
        GUIStyle title, body, small;
        Rect viewport;
        public int PickupsRemaining => pickups.FindAll(p => p != null && p.gameObject.activeSelf).Count;

        void Start()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.transform.SetPositionAndRotation(new Vector3(0, 0, -25), Quaternion.identity);
                camera.orthographic = true;
                camera.orthographicSize = 12.5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Hex("0B1220");
            }
            cyan = MakeMaterial("Speed / Cyan", "43E5E0");
            blue = MakeMaterial("Direction / Blue", "63A7FF");
            violet = MakeMaterial("Shield / Violet", "B39AFF");
            gold = MakeMaterial("Time / Gold", "FFD166");
            red = MakeMaterial("Hazard / Coral", "FF6473");
            panel = MakeMaterial("Arena / Slate", "18263A");
            playerMaterial = MakeMaterial("Player / Ice", "EAF4FF");
            BuildArena();
        }

        void ResetRound()
        {
            if (arena != null)
            {
                arena.gameObject.SetActive(false);
                Destroy(arena.gameObject);
            }
            BuildArena();
        }

        void BuildArena()
        {
            Time.timeScale = 1f;
            pickups.Clear();
            arena = new GameObject("Jeff - Movement Aid Sandbox").transform;
            arena.SetParent(transform);
            timer = new GameObject("Round Timer").AddComponent<CountdownTimer>();
            timer.transform.SetParent(arena);
            timer.time = 90f;
            score = new GameObject("Target Score").AddComponent<ScoreManager>();
            score.transform.SetParent(arena);

            GameObject arrow = Shape("Player", PrimitiveType.Sphere, new Vector3(-9, -5, 0), Vector3.one * 0.7f, playerMaterial);
            arrow.tag = "Player";
            Rigidbody rb = arrow.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            player = arrow.AddComponent<MovementAidController>();
            player.timer = timer;
            arrow.GetComponent<Renderer>().enabled = false;
            GameObject shaft = Shape("Arrow shaft", PrimitiveType.Cube, arrow.transform.position, new Vector3(0.16f, 0.7f, 0.18f), playerMaterial);
            shaft.GetComponent<Collider>().enabled = false;
            shaft.transform.SetParent(arrow.transform, true);
            GameObject tip = Shape("Arrow tip", PrimitiveType.Cube, arrow.transform.position + Vector3.up * 0.45f, new Vector3(0.4f, 0.4f, 0.18f), playerMaterial);
            tip.transform.rotation = Quaternion.Euler(0, 0, 45);
            tip.GetComponent<Collider>().enabled = false;
            tip.transform.SetParent(arrow.transform, true);
            TestPlayer motor = arrow.AddComponent<TestPlayer>();
            motor.speed = 5f;
            motor.requireBreath = true;

            // Pickups use both shape and color to identify the effect.
            Pickup(MovementAidKind.Speed, new Vector3(-6, -5, 0), cyan, PrimitiveType.Cube);
            Pickup(MovementAidKind.DirectionalSpeed, new Vector3(-2, -5, 0), blue, PrimitiveType.Cube);
            Pickup(MovementAidKind.Shield, new Vector3(2, -5, 0), violet, PrimitiveType.Sphere);
            Pickup(MovementAidKind.ExtraTime, new Vector3(6, -5, 0), gold, PrimitiveType.Cylinder);

            Wall(new Vector3(0, 7, 0), new Vector3(23, 0.7f, 1));
            Wall(new Vector3(0, -7, 0), new Vector3(23, 0.7f, 1));
            Wall(new Vector3(-11.5f, 0, 0), new Vector3(0.7f, 14, 1));
            Wall(new Vector3(11.5f, 0, 0), new Vector3(0.7f, 14, 1));
            Wall(new Vector3(3, 0, 0), new Vector3(0.7f, 4, 1));
            for (int i = 0; i < 4; i++)
            {
                GameObject target = Shape("Target " + (i + 1), PrimitiveType.Cylinder,
                    new Vector3(-7 + i * 4, 4, 0), new Vector3(0.75f, 0.15f, 0.75f), gold);
                target.transform.rotation = Quaternion.Euler(90, 0, 0);
                target.GetComponent<Collider>().isTrigger = true;
                target.AddComponent<Target>().scoreManager = score;
            }
            // Low-poly geometric grid is visual only, behind gameplay.
            for (int x = -10; x <= 10; x += 2)
                Decoration(new Vector3(x, 0, 1.5f), new Vector3(0.025f, 13, 0.02f));
            for (int y = -6; y <= 6; y += 2)
                Decoration(new Vector3(0, y, 1.5f), new Vector3(22, 0.025f, 0.02f));
        }

        void Pickup(MovementAidKind kind, Vector3 position, Material material, PrimitiveType shape)
        {
            GameObject go = Shape(kind.ToString(), shape, position, Vector3.one * 0.85f, material);
            if (kind == MovementAidKind.DirectionalSpeed) go.transform.rotation = Quaternion.Euler(0, 0, 45);
            if (kind == MovementAidKind.ExtraTime) go.transform.localScale = new Vector3(0.8f, 0.25f, 0.8f);
            go.GetComponent<Collider>().isTrigger = true;
            MovementAidPickup pickup = go.AddComponent<MovementAidPickup>();
            pickup.kind = kind;
            pickup.duration = 8f;
            pickup.direction = Vector3.right;
            pickups.Add(pickup);
        }

        void Wall(Vector3 position, Vector3 scale)
        {
            GameObject wall = Shape("Hazard", PrimitiveType.Cube, position, scale, red);
            wall.GetComponent<Collider>().isTrigger = true;
            wall.AddComponent<Obstacle>();
        }

        void Decoration(Vector3 position, Vector3 scale)
        {
            GameObject go = Shape("Grid", PrimitiveType.Cube, position, scale, panel);
            go.GetComponent<Collider>().enabled = false;
        }

        GameObject Shape(string label, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(shape);
            go.name = label;
            go.transform.SetParent(arena);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        Material MakeMaterial(string label, string hex)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            Material material = new Material(shader) { name = label, color = Hex(hex) };
            materials.Add(material);
            return material;
        }

        static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }

        void Update()
        {
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            viewport = new Rect((Screen.width - 1280f * scale) / 2f, (Screen.height - 800f * scale) / 2f, 1280f * scale, 800f * scale);
            if (Camera.main != null)
                Camera.main.rect = new Rect(viewport.x / Screen.width, viewport.y / Screen.height, viewport.width / Screen.width, viewport.height / Screen.height);
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                ResetRound();
            if (player != null)
                playerMaterial.color = player.State.HasShield ? violet.color : Hex("EAF4FF");
        }

        void OnGUI()
        {
            if (timer == null) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                body = new GUIStyle(GUI.skin.label) { fontSize = 17 };
                small = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                title.normal.textColor = body.normal.textColor = small.normal.textColor = Hex("EAF4FF");
            }
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.x, viewport.y, 0), Quaternion.identity, Vector3.one * scale);
            GUI.Label(new Rect(32, 20, 900, 45), "ARROW OPERATOR / MOVEMENT LAB", title);
            GUI.Label(new Rect(32, 65, 1100, 30), "Hold SPACE (breath) + WASD / arrows to move. Release SPACE to stop. R restarts.", body);
            GUI.Label(new Rect(32, 105, 1100, 30), $"TIME {Mathf.CeilToInt(timer.time):000}     SCORE {score.score}     PICKUPS {4 - PickupsRemaining} / 4", body);
            if (player != null)
                GUI.Label(new Rect(32, 138, 1150, 30), $"SPEED {player.State.SpeedSeconds:F1}s    RIGHT BOOST {player.State.DirectionSeconds:F1}s    SHIELD {player.State.ShieldSeconds:F1}s", small);
            GUI.Label(new Rect(32, 680, 1200, 30), "CYAN CUBE: 1.8x / 8s    BLUE DIAMOND: +X boost / 8s    VIOLET ORB: shield / 8s    GOLD: +10s", small);
            GUI.Label(new Rect(32, 714, 1200, 30), "Collect the violet shield, then touch a coral wall to test the bounce. Gold discs above are targets.", small);
            if (Time.timeScale == 0f)
            {
                GUI.Box(new Rect(375, 315, 530, 115), GUIContent.none);
                GUI.Label(new Rect(405, 330, 500, 40), player == null ? "CRASH / PRESS R TO RETRY" : "TIME UP / PRESS R TO RETRY", title);
                GUI.Label(new Rect(405, 378, 500, 35), "Collect a shield before touching the coral walls.", small);
            }
            GUI.matrix = previous;
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}
