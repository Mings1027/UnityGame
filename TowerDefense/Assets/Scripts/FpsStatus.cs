#if UNITY_EDITOR
using UnityEngine;

public class FpsStatus : MonoBehaviour
{
    [Range(10, 150)] [SerializeField] private int fontSize = 30;
    [SerializeField] private Color color;
    [SerializeField] private float width, height;

    private void OnGUI()
    {
        var pos = new Rect(width, height, Screen.width, Screen.height);

        var fps = 1.0f / Time.deltaTime;
        var ms = Time.deltaTime * 1000f;
        var text = $"{fps:N1} FPS ({ms:N1}ms)";

        var style = new GUIStyle
        {
            fontSize = fontSize,
            normal =
            {
                textColor = color
            }
        };

        GUI.Label(pos, text, style);
    }
}
#endif