using UnityEngine;
using UnityEngine.InputSystem;

public class CursorTest : MonoBehaviour
{
    private void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard != null && keyboard[Key.N].wasPressedThisFrame)
        {
            Cursor.visible = !Cursor.visible;

            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}