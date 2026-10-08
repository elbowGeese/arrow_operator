using UnityEngine;

/// <summary>
/// 摄像机跟随目标。位置按目标朝向的局部偏移平滑跟随，朝向与目标保持一致。
/// 偏移设为 (0, 0, 0) 就是纯第一人称视角。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Tooltip("相对目标的局部偏移")]
    public Vector3 offset = new Vector3(0f, 0.6f, -2.5f);
    [Tooltip("位置跟随的平滑程度，数值越大跟得越紧")]
    public float followSharpness = 12f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + target.rotation * offset;
        float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, t);
        transform.rotation = target.rotation;
    }
}
