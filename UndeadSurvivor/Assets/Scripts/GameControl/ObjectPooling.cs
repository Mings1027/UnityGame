using System;
using System.Collections.Generic;
using UnityEngine;

// #if UNITY_EDITOR
// using UnityEditor;

namespace GameControl
{
//     [CustomEditor(typeof(ObjectPooling))]
//     public class ObjectPoolingEditor : Editor
//     {
//         private const string Info = "풀링한 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
//                                     "    ObjectPooler.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
//                                     "    CancelInvoke();    // Monobehaviour에 Invoke가 있다면 \n}";
//
//         public override void OnInspectorGUI()
//         {
//             EditorGUILayout.HelpBox(Info, MessageType.Info);
//             base.OnInspectorGUI();
//         }
//     }
// #endif

    public class ObjectPooling : MonoBehaviour
    {
        private static ObjectPooling _inst;
        private void Awake() => _inst = this;

        [Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        [SerializeField] private Pool[] pools;
        private List<GameObject> _spawnObjects;
        private Dictionary<string, Queue<GameObject>> _poolDictionary;

        private readonly string _info = " 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
                                       "    ObjectPooling.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
                                       "    CancelInvoke();    // Mono behaviour에 Invoke가 있다면 \n}";

        public static GameObject SpawnFromPool(string tag, Vector3 position) =>
            _inst._SpawnFromPool(tag, position, Quaternion.identity);

        public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) =>
            _inst._SpawnFromPool(tag, position, rotation);

        public static T SpawnFromPool<T>(string tag, Vector3 position) where T : Component
        {
            var obj = _inst._SpawnFromPool(tag, position, Quaternion.identity);
            if (obj.TryGetComponent(out T component))
                return component;
            obj.SetActive(false);
            throw new Exception($"Component not found");
        }

        public static T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = _inst._SpawnFromPool(tag, position, rotation);
            if (obj.TryGetComponent(out T component))
                return component;
            obj.SetActive(false);
            throw new Exception($"Component not found");
        }

        private static List<GameObject> GetAllPools(string tag)
        {
            if (!_inst._poolDictionary.ContainsKey(tag))
                throw new Exception($"Pool with tag {tag} doesn't exist.");

            return _inst._spawnObjects.FindAll(x => x.name == tag);
        }

        public static List<T> GetAllPools<T>(string tag) where T : Component
        {
            var objects = GetAllPools(tag);

            if (!objects[0].TryGetComponent(out T _))
                throw new Exception("Component not found");

            return objects.ConvertAll(x => x.GetComponent<T>());
        }

        public static void ReturnToPool(GameObject obj)
        {
            if (!_inst._poolDictionary.ContainsKey(obj.name))
                throw new Exception($"Pool with tag {obj.name} doesn't exist.");

            _inst._poolDictionary[obj.name].Enqueue(obj);
        }

        [ContextMenu("GetSpawnObjectsInfo")]
        private void GetSpawnObjectsInfo()
        {
            foreach (var pool in pools)
            {
                var count = _spawnObjects.FindAll(x => x.name == pool.tag).Count;
                Debug.Log($"{pool.tag} count : {count}");
            }
        }

        private GameObject _SpawnFromPool(string stringTag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(stringTag))
                throw new Exception($"Pool with tag {stringTag} doesn't exist.");

            // 큐에 없으면 새로 추가
            var poolQueue = _poolDictionary[stringTag];
            if (poolQueue.Count <= 0)
            {
                var pool = Array.Find(pools, x => x.tag == stringTag);
                var obj = CreateNewObject(pool.tag, pool.prefab);
                ArrangePool(obj);
            }

            // 큐에서 꺼내서 사용
            var objectToSpawn = poolQueue.Dequeue();
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            return objectToSpawn;
        }

        private void Start()
        {
            _spawnObjects = new List<GameObject>();
            _poolDictionary = new Dictionary<string, Queue<GameObject>>();

            // 미리 생성
            foreach (var pool in pools)
            {
                _poolDictionary.Add(pool.tag, new Queue<GameObject>());
                for (var i = 0; i < pool.size; i++)
                {
                    var obj = CreateNewObject(pool.tag, pool.prefab);
                    ArrangePool(obj);
                }

                // OnDisable에 ReturnToPool 구현여부와 중복구현 검사
                if (_poolDictionary[pool.tag].Count <= 0)
                    Debug.LogError($"{pool.tag}{_info}");
                else if (_poolDictionary[pool.tag].Count != pool.size)
                    Debug.LogError($"{pool.tag}에 ReturnToPool이 중복됩니다");
            }
        }

        private GameObject CreateNewObject(string stringTag, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            obj.name = stringTag;
            obj.SetActive(false); // 비활성화시 ReturnToPool을 하므로 Enqueue가 됨
            return obj;
        }

        private void ArrangePool(GameObject obj)
        {
            // 추가된 오브젝트 묶어서 정렬
            var isFind = false;
            for (var i = 0; i < transform.childCount; i++)
            {
                if (i == transform.childCount - 1)
                {
                    obj.transform.SetSiblingIndex(i);
                    _spawnObjects.Insert(i, obj);
                    break;
                }

                if (transform.GetChild(i).name == obj.name)
                    isFind = true;
                else if (isFind)
                {
                    obj.transform.SetSiblingIndex(i);
                    _spawnObjects.Insert(i, obj);
                    break;
                }
            }
        }
    }
}