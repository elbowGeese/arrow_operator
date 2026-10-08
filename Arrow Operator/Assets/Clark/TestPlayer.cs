using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public float speed = 5;

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
        transform.position += move * speed * Time.deltaTime;
    }
}