using UnityEditor;
using UnityEngine;

/// <summary>
/// 用 Kenney Space Station Kit 的模型生成 4 个方形关卡模块预制体和敌人预制体。
/// 菜单：Arthur > Build Level Prefabs。重复执行会覆盖同名预制体，并清理旧版的占位材质。
/// 每个模块是 20x20 的地面加若干墙和立柱，四边不封口，墙与边缘保持间距，保证相邻模块连通。
/// Kenney 的模型以 1 米为一格，这里统一放大 5 倍，所以一个模块是 4x4 块地板。
/// </summary>
public static class LevelPrefabBuilder
{
    const string Root = "Assets/Arthur";
    const string Kit = "Assets/kenney_space-station-kit/Models/FBX format/";
    const float TileSize = 20f;
    const float S = 5f;          // 模型放大倍数
    const float FloorThick = 0.3f * S;

    [MenuItem("Arthur/Build Level Prefabs")]
    public static void Build()
    {
        EnsureFolder("Prefabs");

        // 清理旧版占位材质和长条模块
        foreach (string old in new[] { "Level_Floor", "Level_Wall", "Enemy_Red" })
            AssetDatabase.DeleteAsset(Root + "/Materials/" + old + ".mat");
        foreach (string old in new[] { "Module_A_Straight", "Module_B_Zigzag", "Module_C_Gate", "Module_D_Pillars" })
            AssetDatabase.DeleteAsset(Root + "/Prefabs/" + old + ".prefab");

        // A 开阔地：中间铺特殊地板，四角各一根立柱，用作起始地块
        GameObject a = NewModule("Module_A_Open", true);
        AddPost(a, -5, -5);
        AddPost(a, 5, -5);
        AddPost(a, -5, 5);
        AddPost(a, 5, 5);
        AddSpawn(a, 0, 6);
        AddSpawn(a, 0, -6);
        SaveModule(a);

        // B 错位横墙：两道沿 X 轴的长墙前后错开，形成之字形通道
        GameObject b = NewModule("Module_B_Bars", false);
        AddWallRun(b, true, -5, -5, 0);
        AddWallRun(b, true, 5, 0, 5);
        AddSpawn(b, 6, -6);
        AddSpawn(b, -6, 6);
        AddSpawn(b, 0, 0);
        SaveModule(b);

        // C 十字：中心留空，四个方向各伸出一段墙
        GameObject c = NewModule("Module_C_Cross", false);
        AddWallRun(c, true, 0, -5, 5);
        AddWallRun(c, false, 0, -5, 5);
        AddSpawn(c, -5, -5);
        AddSpawn(c, 5, 5);
        AddSpawn(c, 5, -5);
        SaveModule(c);

        // D 转角：一道 L 形墙加两根立柱
        GameObject d = NewModule("Module_D_Corner", false);
        AddWallRun(d, true, 5, -5, 0);
        AddWallRun(d, false, 5, -5, 0);
        AddPost(d, -5, -5);
        AddPost(d, 0, -5);
        AddSpawn(d, 0, 0);
        AddSpawn(d, -6, 1);
        AddSpawn(d, 7, 6);
        SaveModule(d);

        BuildEnemy();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LevelPrefabBuilder: 已用 Kenney 素材生成 4 个模块预制体和 Enemy 预制体。");
    }

    static void EnsureFolder(string name)
    {
        if (!AssetDatabase.IsValidFolder(Root + "/" + name))
            AssetDatabase.CreateFolder(Root, name);
    }

    static GameObject LoadModel(string name)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Kit + name + ".fbx");
        if (model == null) Debug.LogError("找不到模型 " + Kit + name + ".fbx");
        return model;
    }

    // 实例化 Kenney 模型，统一放大 S 倍，并按网格的包围盒添加 BoxCollider
    static GameObject AddPiece(GameObject parent, string modelName, Vector3 pos, float yaw, bool collider)
    {
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(LoadModel(modelName));
        inst.transform.SetParent(parent.transform, false);
        inst.transform.localPosition = pos;
        inst.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        inst.transform.localScale = Vector3.one * S;

        if (collider)
        {
            Bounds mb = inst.GetComponent<MeshFilter>().sharedMesh.bounds;
            var box = inst.AddComponent<BoxCollider>();
            box.center = mb.center;
            box.size = mb.size;
        }
        return inst;
    }

    // 20x20 的地面，由 4x4 块地板拼成，四边不设围墙
    static GameObject NewModule(string name, bool detailCenter)
    {
        var go = new GameObject(name);
        float[] cells = { -7.5f, -2.5f, 2.5f, 7.5f };
        foreach (float x in cells)
        {
            foreach (float z in cells)
            {
                bool center = Mathf.Abs(x) < 5f && Mathf.Abs(z) < 5f;
                string model = (detailCenter && center) ? "floor-detail" : "floor";
                AddPiece(go, model, new Vector3(x, -FloorThick, z), 0f, true);
                // 天花板：底面贴在墙顶（高度 S），与地面使用同一模型
                AddPiece(go, "floor", new Vector3(x, S, z), 0f, true);
            }
        }
        return go;
    }

    // 沿 X 轴（alongX 为真）或 Z 轴延伸的一排墙。
    // fixedCoord 是墙所在的固定坐标，from 到 to 是首尾两块墙中心的位置，每隔 S/2 放一块。
    static void AddWallRun(GameObject parent, bool alongX, float fixedCoord, float from, float to)
    {
        int index = 0;
        for (float t = from; t <= to + 0.01f; t += S)
        {
            string model = (index % 2 == 1) ? "wall-window" : "wall";
            Vector3 pos = alongX ? new Vector3(t, 0, fixedCoord) : new Vector3(fixedCoord, 0, t);
            AddPiece(parent, model, pos, alongX ? 0f : 90f, true);
            index++;
        }
    }

    // 立柱：用墙角模型
    static void AddPost(GameObject parent, float x, float z)
    {
        AddPiece(parent, "wall-corner", new Vector3(x, 0, z), 0f, true);
    }

    static void AddSpawn(GameObject parent, float x, float z)
    {
        var point = new GameObject("EnemySpawn");
        point.transform.SetParent(parent.transform, false);
        point.transform.localPosition = new Vector3(x, 1.5f, z);
    }

    static void SaveModule(GameObject go)
    {
        LevelModule module = go.AddComponent<LevelModule>();
        module.size = TileSize;
        module.CollectSpawnPoints();
        PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/" + go.name + ".prefab");
        Object.DestroyImmediate(go);
    }

    // 敌人：橙色圆桶（container）。模型放大 2.2 倍，中心对齐到根物体原点。
    static void BuildEnemy()
    {
        const float scale = 2.2f;
        var root = new GameObject("Enemy");

        var model = (GameObject)PrefabUtility.InstantiatePrefab(LoadModel("container"));
        model.transform.SetParent(root.transform, false);
        model.transform.localScale = Vector3.one * scale;
        Bounds mb = model.GetComponent<MeshFilter>().sharedMesh.bounds;
        model.transform.localPosition = -mb.center * scale;

        var box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = mb.size * scale;

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        root.AddComponent<Enemy>();

        PrefabUtility.SaveAsPrefabAsset(root, Root + "/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(root);
    }
}
