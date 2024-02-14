using System;
using System.Globalization;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class SoundTab : MonoBehaviour
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private TMP_Text bgmValueText;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text sfxValueText;

        private void Start()
        {
            InitSlider();
        }

        private void InitSlider()
        {
            var values = SoundManager.GetVolume();

            bgmSlider.value = values.Item1;
            sfxSlider.value = values.Item2;
            bgmValueText.text = Mathf.FloorToInt(bgmSlider.value * 100).ToString();
            sfxValueText.text = Mathf.FloorToInt(sfxSlider.value * 100).ToString();

            bgmSlider.onValueChanged.AddListener(delegate
            {
                SoundManager.SetBGMVolume(bgmSlider.value);
                bgmValueText.text = Mathf.FloorToInt(bgmSlider.value * 100).ToString(CultureInfo.InvariantCulture);
            });
            sfxSlider.onValueChanged.AddListener(delegate
            {
                SoundManager.SetSfxVolume(sfxSlider.value);
                sfxValueText.text = Mathf.FloorToInt(sfxSlider.value * 100).ToString(CultureInfo.InvariantCulture);
            });
        }
    }
}