using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public float speed = 5;
    [Tooltip("Enable for the flute demo. Space represents the Makey Makey breath contact.")]
    public bool requireBreath;
    ArrowOperator.Jeff.MovementAidController movementAids;

    void Awake() => movementAids = GetComponent<ArrowOperator.Jeff.MovementAidController>();

    void Update()
    {
        if (Time.timeScale == 0) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0;
        float y = 0;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            x -= 1;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            x += 1;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            y += 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            y -= 1;

        Vector3 move = new Vector3(x, y, 0).normalized;
        if (requireBreath && !keyboard.spaceKey.isPressed) move = Vector3.zero;
        Vector3 velocity = move * speed;
        if (movementAids != null && movementAids.isActiveAndEnabled)
            velocity = movementAids.ModifyVelocity(velocity);
        transform.position += velocity * Time.deltaTime;
    }
}
