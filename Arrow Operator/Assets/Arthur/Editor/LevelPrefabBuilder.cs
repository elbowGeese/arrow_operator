using UnityEditor;
using UnityEngine;

/// <summary>
/// 用 Unity 内置模型生成 4 个方形关卡模块预制体、敌人预制体和材质。
/// 菜单：Arthur > Build Level Prefabs。重复执行会覆盖同名预制体，并清理旧版的长条模块。
/// 每个模块是 20x20 的地面加若干墙和立柱，四边不封口，墙与边缘保持间距，保证相邻模块连通。
/// </summary>
public static class LevelPrefabBuilder
{
    const string Root = "Assets/Arthur";
    const float TileSize = 20f;
    const float WallHeight = 6f;

    static Material floorMat, wallMat, enemyMat;

    [MenuItem("Arthur/Build Level Prefabs")]
    public static void Build()
    {
        EnsureFolder("Prefabs");
        EnsureFolder("Materials");

        // 清理旧版沿 Z 轴的长条模块
        foreach (string old in new[] { "Module_A_Straight", "Module_B_Zigzag", "Module_C_Gate", "Module_D_Pillars" })
            AssetDatabase.DeleteAsset(Root + "/Prefabs/" + old + ".prefab");

        floorMat = MakeMaterial("Level_Floor", new Color(0.55f, 0.55f, 0.58f));
        wallMat = MakeMaterial("Level_Wall", new Color(0.25f, 0.28f, 0.35f));
        enemyMat = MakeMaterial("Enemy_Red", new Color(0.85f, 0.1f, 0.1f));

        // A 开阔地：只有四角的立柱，用作起始地块
        GameObject a = NewModule("Module_A_Open");
        AddPillar(a, -6, -6);
        AddPillar(a, 6, -6);
        AddPillar(a, -6, 6);
        AddPillar(a, 6, 6);
        AddSpawn(a, 0, 4);
        AddSpawn(a, 0, -4);
        SaveModule(a);

        // B 错位横墙：两道沿 X 轴的长墙前后错开，形成之字形通道
        GameObject b = NewModule("Module_B_Bars");
        AddWallX(b, -3, -4, 10);
        AddWallX(b, 3, 4, 10);
        AddSpawn(b, 5, -6);
        AddSpawn(b, -5, 6);
        AddSpawn(b, 0, 0);
        SaveModule(b);

        // C 十字：中心留空，四个方向各伸出一段墙
        GameObject c = NewModule("Module_C_Cross");
        AddWallX(c, -5, 0, 6);
        AddWallX(c, 5, 0, 6);
        AddWallZ(c, 0, -5, 6);
        AddWallZ(c, 0, 5, 6);
        AddSpawn(c, -5, -5);
        AddSpawn(c, 5, 5);
        AddSpawn(c, 5, -5);
        SaveModule(c);

        // D 转角：一道 L 形墙加两根立柱
        GameObject d = NewModule("Module_D_Corner");
        AddWallX(d, -4.5f, 6, 7);
        AddWallZ(d, 6, -4.5f, 7);
        AddPillar(d, -3, -3);
        AddPillar(d, 3, 3);
        AddSpawn(d, 0, 0);
        AddSpawn(d, -6, 1);
        AddSpawn(d, 1, -6);
        SaveModule(d);

        BuildEnemy();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LevelPrefabBuilder: 已生成 4 个方形模块预制体和 Enemy 预制体。");
    }

    static void EnsureFolder(string name)
    {
        if (!AssetDatabase.IsValidFolder(Root + "/" + name))
            AssetDatabase.CreateFolder(Root, name);
    }

    static Material MakeMaterial(string name, Color color)
    {
        string path = Root + "/Materials/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(m);
        return m;
    }

    // 只有 20x20 的地面，四边不设围墙
    static GameObject NewModule(string name)
    {
        var go = new GameObject(name);
        AddBox(go, "Floor", new Vector3(0, -0.5f, 0), new Vector3(TileSize, 1, TileSize), floorMat);
        return go;
    }

    static void AddBox(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent.transform, false);
        box.transform.localPosition = pos;
        box.transform.localScale = scale;
        box.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // 沿 X 轴延伸的墙，中心在 (x, z)，长度 length
    static void AddWallX(GameObject parent, float x, float z, float length)
    {
        AddBox(parent, "Wall_X", new Vector3(x, WallHeight * 0.5f, z), new Vector3(length, WallHeight, 1), wallMat);
    }

    // 沿 Z 轴延伸的墙，中心在 (x, z)，长度 length
    static void AddWallZ(GameObject parent, float x, float z, float length)
    {
        AddBox(parent, "Wall_Z", new Vector3(x, WallHeight * 0.5f, z), new Vector3(1, WallHeight, length), wallMat);
    }

    static void AddPillar(GameObject parent, float x, float z)
    {
        AddBox(parent, "Pillar", new Vector3(x, WallHeight * 0.5f, z), new Vector3(2, WallHeight, 2), wallMat);
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

    static void BuildEnemy()
    {
        GameObject e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        e.name = "Enemy";
        e.transform.localScale = Vector3.one * 1.2f;
        e.GetComponent<MeshRenderer>().sharedMaterial = enemyMat;
        e.GetComponent<SphereCollider>().isTrigger = true;
        Rigidbody rb = e.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        e.AddComponent<Enemy>();
        PrefabUtility.SaveAsPrefabAsset(e, Root + "/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(e);
    }
}
