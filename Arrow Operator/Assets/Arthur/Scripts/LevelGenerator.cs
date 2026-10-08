using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡与敌人生成器（四向网格）。
/// 世界被划分为边长 tileSize 的方格，以玩家所在的格子为中心，向前后左右四个方向生成地块。
/// 玩家移动到新的格子时补齐周围的地块，距离超过 hideRadius 格的地块被隐藏并放入对象池。
/// 同一个格子在任何时候生成的地块种类都相同。
/// 敌人在地块的空隙（EnemySpawn 生成点）生成，所在地块被隐藏或距离玩家过远时删除。
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("玩家（箭）的 Transform")]
    public Transform player;
    [Tooltip("4 个关卡预制模块，第 1 个会作为玩家出生的起始地块")]
    public LevelModule[] modulePrefabs;
    [Tooltip("敌人预制体")]
    public GameObject enemyPrefab;

    [Header("关卡生成")]
    [Tooltip("地块边长，需要与模块预制体一致")]
    public float tileSize = 20f;
    [Tooltip("以玩家所在格为中心，向四个方向各生成多少格")]
    public int generateRadius = 2;
    [Tooltip("距离玩家超过多少格的地块会被隐藏，必须大于生成半径")]
    public int hideRadius = 3;
    [Tooltip("随机种子，0 表示每次运行都不同")]
    public int seed = 0;

    [Header("敌人生成")]
    [Range(0f, 1f)]
    [Tooltip("每个生成点生成敌人的概率")]
    public float enemySpawnChance = 0.7f;
    [Tooltip("起始格周围多少格以内不生成敌人")]
    public int safeRadius = 0;
    [Tooltip("敌人距离玩家超过该值时删除")]
    public float enemyDespawnDistance = 100f;

    class TileRecord
    {
        public LevelModule module;
        public int prefabIndex;
        public List<GameObject> enemies = new List<GameObject>();
    }

    readonly Dictionary<Vector2Int, TileRecord> active = new Dictionary<Vector2Int, TileRecord>();
    readonly List<Stack<LevelModule>> pools = new List<Stack<LevelModule>>();
    readonly List<GameObject> allEnemies = new List<GameObject>();
    readonly List<Vector2Int> removeBuffer = new List<Vector2Int>();

    Transform moduleRoot;
    Transform enemyRoot;
    System.Random enemyRng;
    int levelSeed;
    Vector2Int lastCell;
    bool hasCell;

    void Start()
    {
        if (player == null || modulePrefabs == null || modulePrefabs.Length == 0)
        {
            Debug.LogError("LevelGenerator: 请指定 player 和 modulePrefabs。", this);
            enabled = false;
            return;
        }

        levelSeed = seed != 0 ? seed : new System.Random().Next(1, int.MaxValue);
        enemyRng = new System.Random(levelSeed);

        moduleRoot = new GameObject("Modules").transform;
        moduleRoot.SetParent(transform, false);
        enemyRoot = new GameObject("Enemies").transform;
        enemyRoot.SetParent(transform, false);

        for (int i = 0; i < modulePrefabs.Length; i++)
            pools.Add(new Stack<LevelModule>());

        RefreshTiles(GetPlayerCell());
    }

    void Update()
    {
        Vector2Int cell = GetPlayerCell();
        if (!hasCell || cell != lastCell)
            RefreshTiles(cell);

        UpdateEnemies();
    }

    // 玩家所在的格子坐标。生成器所在位置是 (0,0) 格的中心。
    Vector2Int GetPlayerCell()
    {
        Vector3 local = player.position - transform.position;
        return new Vector2Int(Mathf.RoundToInt(local.x / tileSize), Mathf.RoundToInt(local.z / tileSize));
    }

    void RefreshTiles(Vector2Int center)
    {
        lastCell = center;
        hasCell = true;

        for (int dx = -generateRadius; dx <= generateRadius; dx++)
        {
            for (int dz = -generateRadius; dz <= generateRadius; dz++)
            {
                var cell = new Vector2Int(center.x + dx, center.y + dz);
                if (!active.ContainsKey(cell))
                    SpawnTile(cell);
            }
        }

        removeBuffer.Clear();
        foreach (var pair in active)
        {
            int distance = Mathf.Max(Mathf.Abs(pair.Key.x - center.x), Mathf.Abs(pair.Key.y - center.y));
            if (distance > hideRadius)
                removeBuffer.Add(pair.Key);
        }
        foreach (Vector2Int cell in removeBuffer)
            HideTile(cell);
    }

    void SpawnTile(Vector2Int cell)
    {
        int index = PickPrefabIndex(cell);
        LevelModule module = GetFromPool(index);

        Vector3 pos = transform.position + new Vector3(cell.x * tileSize, 0f, cell.y * tileSize);
        module.transform.SetPositionAndRotation(pos, Quaternion.identity);
        module.gameObject.SetActive(true);

        var record = new TileRecord { module = module, prefabIndex = index };
        active[cell] = record;

        int fromStart = Mathf.Max(Mathf.Abs(cell.x), Mathf.Abs(cell.y));
        if (fromStart > safeRadius)
            SpawnEnemies(record);
    }

    // 由格子坐标和种子决定地块种类，同一个格子每次都一样。起始格固定使用第 1 个模块。
    int PickPrefabIndex(Vector2Int cell)
    {
        if (cell == Vector2Int.zero) return 0;

        unchecked
        {
            int h = levelSeed;
            h = h * 73856093 ^ cell.x * 19349663 ^ cell.y * 83492791;
            h ^= h >> 13;
            h *= 1274126177;
            h ^= h >> 16;
            return (int)((uint)h % (uint)modulePrefabs.Length);
        }
    }

    LevelModule GetFromPool(int index)
    {
        Stack<LevelModule> pool = pools[index];
        while (pool.Count > 0)
        {
            LevelModule pooled = pool.Pop();
            if (pooled != null) return pooled;
        }
        return Instantiate(modulePrefabs[index], moduleRoot);
    }

    void HideTile(Vector2Int cell)
    {
        TileRecord record = active[cell];
        foreach (GameObject enemy in record.enemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        record.enemies.Clear();

        record.module.gameObject.SetActive(false);
        pools[record.prefabIndex].Push(record.module);
        active.Remove(cell);
    }

    void SpawnEnemies(TileRecord record)
    {
        if (enemyPrefab == null) return;

        foreach (Transform point in record.module.enemySpawnPoints)
        {
            if (point == null) continue;
            if (enemyRng.NextDouble() > enemySpawnChance) continue;

            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation, enemyRoot);
            record.enemies.Add(enemy);
            allEnemies.Add(enemy);
        }
    }

    void UpdateEnemies()
    {
        float sqrLimit = enemyDespawnDistance * enemyDespawnDistance;
        for (int i = allEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = allEnemies[i];
            if (enemy == null)
            {
                allEnemies.RemoveAt(i);
                continue;
            }
            if ((enemy.transform.position - player.position).sqrMagnitude > sqrLimit)
            {
                Destroy(enemy);
                allEnemies.RemoveAt(i);
            }
        }
    }

    void OnValidate()
    {
        if (generateRadius < 1) generateRadius = 1;
        if (hideRadius <= generateRadius) hideRadius = generateRadius + 1;

        // 敌人删除距离必须大于最远的生成距离（对角方向），否则刚生成就被删除
        float minDespawn = (generateRadius + 1f) * tileSize * 1.42f + 5f;
        if (enemyDespawnDistance < minDespawn) enemyDespawnDistance = minDespawn;
    }
}
