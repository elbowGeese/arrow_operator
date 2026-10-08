using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡预制模块（方形地块）。挂在模块预制体根物体上。
/// 模块中心位于预制体原点，四边都不封口，与相邻地块自然连通。
/// 名字以 "EnemySpawn" 开头的子物体会被识别为敌人生成点。
/// </summary>
public class LevelModule : MonoBehaviour
{
    [Tooltip("地块边长，需要与 LevelGenerator 的 tileSize 一致")]
    public float size = 20f;

    [Tooltip("敌人生成点，位于模块的空隙处")]
    public List<Transform> enemySpawnPoints = new List<Transform>();

    void Reset()
    {
        CollectSpawnPoints();
    }

    [ContextMenu("Collect Spawn Points")]
    public void CollectSpawnPoints()
    {
        enemySpawnPoints.Clear();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t != transform && t.name.StartsWith("EnemySpawn"))
                enemySpawnPoints.Add(t);
        }
    }
}
