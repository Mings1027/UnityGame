using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolygonArsenal
{
    public class PolygonSoundSpawn : MonoBehaviour
    {

        public GameObject prefabSound;

        public bool destroyWhenDone = true;
        public bool soundPrefabIsChild = false;
        [Range(0.01f, 10f)]
        public float pitchRandomMultiplier = 1f;

        // Use this for initialization
        void Start()
        {
            //Spawn the sound object
            GameObject mSound = Instantiate(prefabSound, transform.position, Quaternion.identity);
            AudioSource mSource = mSound.GetComponent<AudioSource>();

            //Attach object to parent if true
            if (soundPrefabIsChild)
                mSound.transform.SetParent(transform);

            //Multiply pitch
            if (pitchRandomMultiplier != 1)
            {
                if (Random.value < .5)
                    mSource.pitch *= Random.Range(1 / pitchRandomMultiplier, 1);
                else
                    mSource.pitch *= Random.Range(1, pitchRandomMultiplier);
            }

            //Set lifespan if true
            if (destroyWhenDone)
            {
                float life = mSource.clip.length / mSource.pitch;
                Destroy(mSound, life);
            }
        }
    }
}
