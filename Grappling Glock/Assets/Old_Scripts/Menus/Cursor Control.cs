using UnityEngine;

public class CursorControl : MonoBehaviour
{
    [Header("Cursor Settings")]
    public bool lockCursor = false;  // Toggle this in the Inspector

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
