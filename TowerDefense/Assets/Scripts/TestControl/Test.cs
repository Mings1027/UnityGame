using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TestControl
{
    public class Test : MonoBehaviour
    {
        private Image image;
        private Transform shadowParent;
        private Transform shadow;

        private static readonly int FadeSize = Shader.PropertyToID("_Fade_Size");

        [SerializeField] private float rotateSpeed;

        // private void Awake()
        // {
        //     shadowParent = transform.GetChild(0);
        //     shadow = shadowParent.GetChild(0);
        //     image = GetComponentInChildren<Image>();
        // }

        // private void OnEnable()
        // {
        //     DOTween.Sequence().Append(transform.DOMoveZ(3, 1)).Append(transform.DORotate(new Vector3(0, 0, 360), 1, RotateMode.FastBeyond360));
        // }

        // private void Update()
        // {
        //     transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed);
        // }

        private void RotateShadows()
        {
            shadowParent.Rotate(Vector3.up, Time.deltaTime * rotateSpeed);
            shadow.Rotate(Vector3.forward, Time.deltaTime * rotateSpeed);
        }

        private void CircleFadeIn()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                image.materialForRendering.DOFloat(1.5f, FadeSize, 1).From(0);
            // image.materialForRendering.SetFloat(Size, size);
        }
    }
}