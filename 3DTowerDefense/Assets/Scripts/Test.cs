using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class Test : MonoBehaviour
{
    public void onSetting()

    {
        var cam = GetComponent<Camera>();

        var rect = cam.rect;

        var scaleheight = ((float)Screen.width / Screen.height) / ((float)16 / 9); // (가로 / 세로)

        var scalewidth = 1f / scaleheight;

        if (scaleheight < 1)

        {
            rect.height = scaleheight;

            rect.y = (1f - scaleheight) / 2f;
        }

        else

        {
            rect.width = scalewidth;

            rect.x = (1f - scalewidth) / 2f;
        }

        cam.rect = rect;
    }


    public void OnReset()

    {
        var camera = GetComponent<Camera>();

        camera.rect = new Rect(0, 0, 1, 1);
    }


    private void OnEnable()

    {
#if !UNITY_EDITOR
RenderPipelineManager.beginCameraRendering += RenderPipelineManager_endCameraRendering;

#endif
    }

    private void OnDisable()

    {
#if !UNITY_EDITOR
RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_endCameraRendering;

#endif
    }


    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)

    {
        GL.Clear(true, true, Color.black);
    }


    private void OnPreCull()

    {
        GL.Clear(true, true, Color.black);
    }
}