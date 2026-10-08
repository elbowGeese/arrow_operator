using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家操控的箭。
/// 按住空格键向前飞行，松开后不再前进。
/// 方向键上下控制上升和下降，左右控制向左和向右，使用 Rigidbody 与墙壁、地面碰撞。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ArrowController : MonoBehaviour
{
    [Tooltip("按住空格时向前飞行的速度")]
    public float forwardSpeed = 10f;
    [Tooltip("上下左右移动的速度")]
    public float steerSpeed = 8f;
    [Tooltip("加速和减速的快慢，数值越大响应越快")]
    public float acceleration = 30f;

    Rigidbody body;
    Vector2 steerInput;
    bool forwardHeld;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = false;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // 零摩擦，贴着墙壁和地面时仍能顺畅滑动
        var slippery = new PhysicsMaterial("ArrowSlippery")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.sharedMaterial = slippery;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || Time.timeScale == 0f)
        {
            steerInput = Vector2.zero;
            forwardHeld = false;
            return;
        }

        float x = 0f;
        float y = 0f;
        if (keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.upArrowKey.isPressed) y += 1f;
        if (keyboard.downArrowKey.isPressed) y -= 1f;

        steerInput = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        forwardHeld = keyboard.spaceKey.isPressed;
    }

    void FixedUpdate()
    {
        float forward = forwardHeld ? forwardSpeed : 0f;
        Vector3 localVelocity = new Vector3(steerInput.x * steerSpeed, steerInput.y * steerSpeed, forward);
        Vector3 target = transform.TransformDirection(localVelocity);
        body.linearVelocity = Vector3.MoveTowards(body.linearVelocity, target, acceleration * Time.fixedDeltaTime);
    }
}
