using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteAlways]
public class DayLightController : MonoBehaviour
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private LightPreset preset;
    [SerializeField, Range(0, 24)] private float timeOfDay;

    private void OnValidate()
    {
        if (directionalLight != null) return;
        if (RenderSettings.sun != null)
        {
            directionalLight = RenderSettings.sun;
        }
    }

    private void Update()
    {
        if (preset == null) return;
        if (Application.isPlaying)
        {
            timeOfDay += Time.deltaTime;
            timeOfDay %= 24;
            UpdateLighting(timeOfDay / 24f);
        }
        else
        {
            UpdateLighting(timeOfDay / 24f);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);
        if (directionalLight != null)
        {
            directionalLight.color = preset.directionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3(timePercent * 360f - 90f, 170f, 0));
        }
        else
        {
            var lights = FindObjectsOfType<Light>();
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    directionalLight = l;
                    return;
                }
            }
        }
    }
}