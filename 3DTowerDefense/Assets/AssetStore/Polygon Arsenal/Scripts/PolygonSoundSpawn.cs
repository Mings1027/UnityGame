using UnityEngine;

namespace AssetStore.Polygon_Arsenal.Scripts
{
    public class PolygonSoundSpawn : MonoBehaviour
    {

        public GameObject prefabSound;

        public bool destroyWhenDone = true;
        public bool soundPrefabIsChild;
        [Range(0.01f, 10f)]
        public float pitchRandomMultiplier = 1f;

        // Use this for initialization
        private void Start()
        {
            //Spawn the sound object
            var sound = Instantiate(prefabSound, transform.position, Quaternion.identity);
            var source = sound.GetComponent<AudioSource>();

            //Attach object to parent if true
            if (soundPrefabIsChild)
                sound.transform.SetParent(transform);

            //Multiply pitch
            if (pitchRandomMultiplier != 1)
            {
                if (Random.value < .5)
                    source.pitch *= Random.Range(1 / pitchRandomMultiplier, 1);
                else
                    source.pitch *= Random.Range(1, pitchRandomMultiplier);
            }

            //Set lifespan if true
            if (destroyWhenDone)
            {
                var life = source.clip.length / source.pitch;
                Destroy(sound, life);
            }
        }
    }
}
