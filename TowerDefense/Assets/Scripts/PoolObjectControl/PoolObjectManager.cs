using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace PoolObjectControl
{
    public class PoolObjectManager : Singleton<PoolObjectManager>
    {
        [Serializable]
        public class Pool : IComparable<Pool>
        {
            public PoolObjectKey poolObjectKey;
            public GameObject prefab;
            public byte initSize;
            public byte maxSize;

            public int CompareTo(Pool other)
            {
                return string.CompareOrdinal(poolObjectKey.ToString(), other.poolObjectKey.ToString());
            }
        }

        private static PoolObjectManager _inst;
        private Dictionary<PoolObjectKey, Stack<GameObject>> _prefabDictionary;
        private Dictionary<PoolObjectKey, Pool> _poolDictionary;

        private const string Info = "You have to attach PoolObject";

        [SerializeField] private Pool[] pools;

        private void Awake()
        {
            _inst = this;
            _prefabDictionary = new Dictionary<PoolObjectKey, Stack<GameObject>>();
            _poolDictionary = new Dictionary<PoolObjectKey, Pool>();
            PoolInit();
#if UNITY_EDITOR
            SortPool();
#endif
        }

        [ContextMenu("Sort Pool")]
        private void SortPool() => Array.Sort(pools);

        [ContextMenu("Match Pool Object Key To Prefab Key")]
        private void MatchPoolKeyToPrefabKey()
        {
            for (var i = 0; i < pools.Length; i++)
            {
                pools[i].prefab.GetComponent<PoolObject>().PoolObjKey = pools[i].poolObjectKey;
            }
        }

        private void PoolInit()
        {
            for (var i = 0; i < pools.Length; i++)
            {
                _poolDictionary.Add(pools[i].poolObjectKey, pools[i]);
                pools[i].prefab.GetComponent<PoolObject>().PoolObjKey = pools[i].poolObjectKey;
            }

            for (var i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];
                if (pool.prefab == null) throw new Exception($"{pool.poolObjectKey} doesn't exist");
                _prefabDictionary.Add(pool.poolObjectKey, new Stack<GameObject>());
                if (pool.maxSize == 0) pool.maxSize = 30;
                for (var j = 0; j < pool.initSize; j++)
                {
                    CreateNewObject(pool.poolObjectKey, pool.prefab);
// #if UNITY_EDITOR
                    // SortObject(obj);
                }

                // if (_prefabDictionary[pool.poolObjectKey].Count <= 0)
                //     print($"{Info} in {pool.poolObjectKey} Prefab");
                // else if (_prefabDictionary[pool.poolObjectKey].Count != pool.initSize)
                //     print($"It's duplicated ReturnToPool in{pool.poolObjectKey} object");
// #endif
            }
        }

        public static void Get(PoolObjectKey poolObjectKey, Transform t) =>
            _inst.Spawn(poolObjectKey, t.position, t.rotation);

        public static void Get(PoolObjectKey poolObjectKey, Vector3 position) =>
            _inst.Spawn(poolObjectKey, position, Quaternion.identity);

        public static void Get(PoolObjectKey poolObjectKey, Vector3 position, Quaternion rotation) =>
            _inst.Spawn(poolObjectKey, position, rotation);

        public static T Get<T>(PoolObjectKey poolObjectKey, Transform t) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, t.position, t.rotation);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, Quaternion.identity);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, rotation);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static void ReturnToPool(GameObject obj, PoolObjectKey poolObjKey)
        {
            if (!_inst._poolDictionary.TryGetValue(poolObjKey, out _)) return;
            _inst._prefabDictionary[poolObjKey].Push(obj);
        }

        public async void PoolCleaner()
        {
            foreach (var poolKey in _prefabDictionary.Keys)
            {
                var outOfRange = _prefabDictionary[poolKey].Count > _poolDictionary[poolKey].maxSize;

                while (outOfRange)
                {
                    Destroy(_prefabDictionary[poolKey].Pop());
                    outOfRange = _prefabDictionary[poolKey].Count > _poolDictionary[poolKey].maxSize;
                    await Task.Delay(100);
                }
            }
        }

        private GameObject Spawn(PoolObjectKey poolObjKey, Vector3 position, Quaternion rotation)
        {
            if (!_prefabDictionary.ContainsKey(poolObjKey))
                throw new Exception($"Pool with tag {poolObjKey} doesn't exist.");
            var poolStack = _prefabDictionary[poolObjKey];
            if (poolStack.Count <= 0)
            {
                var pool = GetPool_OrNull(poolObjKey);
                if (pool == null) throw new Exception($"Pool with tag {poolObjKey} doesn't exist.");
                CreateNewObject(poolObjKey, pool.prefab);
            }

            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.SetActive(true);
            return poolObj;
        }

        private Pool GetPool_OrNull(PoolObjectKey poolObjectKey)
        {
            return _poolDictionary.TryGetValue(poolObjectKey, out var pool) ? pool : null;
        }

        private void CreateNewObject(PoolObjectKey poolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            if (!obj.TryGetComponent(out PoolObject poolObject))
                throw new Exception($"You have to attach PoolObject.cs in {prefab.name} prefab");
            poolObject.PoolObjKey = poolObjectKey;
            obj.name = poolObjectKey.ToString();
            obj.SetActive(false);
            // return obj;
        }

        private void SortObject(GameObject obj)
        {
            var isFind = false;
            for (var i = 0; i < transform.childCount; i++)
            {
                if (i == transform.childCount - 1)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }

                if (transform.GetChild(i).name == obj.name) isFind = true;
                else if (isFind)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }
            }
        }
    }
}