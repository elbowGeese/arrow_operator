using UnityEngine;
using UnityEngine.InputSystem;
using ArrowOperator.Jeff;

[RequireComponent(typeof(Rigidbody))]
public class ArrowController : MonoBehaviour
{
    public float forwardSpeed = 10f;
    public float steerSpeed = 8f;
    public float acceleration = 30f;
    [Tooltip("Keep disabled for existing automatic-flight scenes. Space simulates breath when enabled.")]
    public bool requireBreath;
    public bool useKeyboardInput = true;
    Rigidbody body;
    ArrowPowerups powerups;
    PhysicsMaterial slippery;
    Vector2 steerInput;
    bool breathing;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        powerups = GetComponent<ArrowPowerups>();
        body.useGravity = false;
        body.isKinematic = false;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        slippery = new PhysicsMaterial("ArrowSlippery")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
        foreach (Collider collider in GetComponentsInChildren<Collider>()) collider.sharedMaterial = slippery;
    }

    // A hardware adapter can disable keyboard input and supply steering and breath here.
    public void SetInput(Vector2 steering, bool breath)
    {
        steerInput = Vector2.ClampMagnitude(steering, 1f);
        breathing = breath;
    }

    void Update()
    {
        if (!useKeyboardInput) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) { SetInput(Vector2.zero, false); return; }
        float x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
            - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
        float y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
            - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        SetInput(new Vector2(x, y), keyboard.spaceKey.isPressed);
    }

    void FixedUpdate()
    {
        if (requireBreath && !breathing)
        {
            body.linearVelocity = Vector3.zero;
            if (powerups != null) powerups.ModifyVelocity(Vector3.zero);
            return;
        }
        Vector3 target = transform.TransformDirection(new Vector3(steerInput.x * steerSpeed, steerInput.y * steerSpeed, forwardSpeed));
        if (powerups != null) target = powerups.ModifyVelocity(target);
        body.linearVelocity = Vector3.MoveTowards(body.linearVelocity, target, acceleration * Time.fixedDeltaTime);
    }

    void OnDisable()
    {
        if (body != null && !body.isKinematic) body.linearVelocity = Vector3.zero;
        steerInput = Vector2.zero;
        breathing = false;
    }

    void OnDestroy() { if (slippery != null) Destroy(slippery); }
}
