using System;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text uiText;
    
    public int Duration { get; private set; }
    private int remainingDuration;

    private void Awake()
    {
        ResetTimer();
    }

    private void ResetTimer()
    {
        uiText.text = "00:00";
        fillImage.fillAmount = 0;
        Duration = remainingDuration = 0;
    }

    private Timer SetDuration(int seconds)
    {
        Duration = remainingDuration = seconds;
        return this;
    }
    
    
}
