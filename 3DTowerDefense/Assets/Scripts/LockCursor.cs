using UnityEngine;

public class LockCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursor;

    private void Start()
    {
        ChangeCursor(cursor);
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void ChangeCursor(Texture2D cursor)
    {
        var cursorHotSpot = new Vector2(cursor.width * 0.5f, cursor.height * 0.5f);
        Cursor.SetCursor(cursor, cursorHotSpot, CursorMode.Auto);
    }
}