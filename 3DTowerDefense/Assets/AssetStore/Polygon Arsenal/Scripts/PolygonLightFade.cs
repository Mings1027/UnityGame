using UnityEngine;
using System.Collections;

namespace PolygonArsenal
{
    public class PolygonLightFade : MonoBehaviour
    {
        [Header("Seconds to dim the light")]
        public float life = 0.2f;
        public bool killAfterLife = true;

        private Light _li;
        private float _initIntensity;

        // Use this for initialization
        void Start()
        {
            if (gameObject.GetComponent<Light>())
            {
                _li = gameObject.GetComponent<Light>();
                _initIntensity = _li.intensity;
            }
            else
                print("No light object found on " + gameObject.name);
        }

        // Update is called once per frame
        void Update()
        {
            if (gameObject.GetComponent<Light>())
            {
                _li.intensity -= _initIntensity * (Time.deltaTime / life);
                if (killAfterLife && _li.intensity <= 0)
                    Destroy(gameObject);
            }
        }
    }
}