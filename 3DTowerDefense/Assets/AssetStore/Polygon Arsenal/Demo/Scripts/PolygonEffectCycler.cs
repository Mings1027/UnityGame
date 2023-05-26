using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonArsenal
{
    public class PolygonEffectCycler : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> listOfEffects;

        [Header("Loop length in seconds")]
        [SerializeField]
        float loopTimeLength = 5f;

        float _timeOfLastInstantiate;

        GameObject _instantiatedEffect;

        int _effectIndex = 0;

        // Use this for initialization
        void Start()
        {
            _instantiatedEffect = Instantiate(listOfEffects[_effectIndex], transform.position, transform.rotation) as GameObject;
            _effectIndex++;
            _timeOfLastInstantiate = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= _timeOfLastInstantiate + loopTimeLength)
            {
                Destroy(_instantiatedEffect);
                _instantiatedEffect = Instantiate(listOfEffects[_effectIndex], transform.position, transform.rotation) as GameObject;
                _timeOfLastInstantiate = Time.time;
                if (_effectIndex < listOfEffects.Count - 1)
                    _effectIndex++;
                else
                    _effectIndex = 0;
            }
        }
    }
}
