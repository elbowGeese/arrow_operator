using UnityEngine;

/// <summary>
/// 敌人。被带有 killerTag 标签的物体（玩家的箭）碰到后死亡。
/// 需要敌人或箭其中一方带有 Rigidbody，并且敌人的 Collider 勾选 Is Trigger。
/// </summary>
public class Enemy : MonoBehaviour
{
    [Tooltip("能击杀该敌人的物体标签")]
    public string killerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(killerTag))
            Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
