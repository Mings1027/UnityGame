using System;
using System.Collections.Generic;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace PoolObjectControl
{
    public class PoolObjectManager : MonoBehaviour
    {
        [Serializable]
        public class Pool : IComparable<Pool>
        {
            public PoolObjectKey poolObjectKey;
            public GameObject prefab;
            public byte initSize;

            public int CompareTo(Pool other)
            {
                return string.CompareOrdinal(poolObjectKey.ToString(), other.poolObjectKey.ToString());
            }
        }

        [Serializable]
        public class UIPool : IComparable<UIPool>
        {
            public UIPoolObjectKey uiPoolObjectKey;
            public GameObject uiPrefab;
            public byte initSize;

            public int CompareTo(UIPool other)
            {
                return string.CompareOrdinal(uiPoolObjectKey.ToString(), other.uiPoolObjectKey.ToString());
            }
        }

        [Serializable]
        public class EnemyPool : IComparable<EnemyPool>
        {
            public EnemyPoolObjectKey enemyPoolObjKey;
            public GameObject enemyPrefab;
            public byte initSize;

            public int CompareTo(EnemyPool other)
            {
                return string.CompareOrdinal(enemyPoolObjKey.ToString(), other.enemyPoolObjKey.ToString());
            }
        }

        private static PoolObjectManager _inst;
        private Camera _cam;
        private Dictionary<PoolObjectKey, Stack<GameObject>> _prefabDictionary;
        private Dictionary<PoolObjectKey, Pool> _poolDictionary;
        private Dictionary<UIPoolObjectKey, Stack<GameObject>> _uiPrefabDictionary;
        private Dictionary<UIPoolObjectKey, UIPool> _uiPoolDictionary;
        private Dictionary<EnemyPoolObjectKey, Stack<GameObject>> _enemyPrefabDictionary;
        private Dictionary<EnemyPoolObjectKey, EnemyPool> _enemyPoolDictionary;

        [SerializeField] private Transform canvas;
        [SerializeField] private byte poolMaxSize;
        [SerializeField] private Pool[] pools;
        [SerializeField] private UIPool[] uiPools;
        [SerializeField] private EnemyPool[] enemyPools;

        private void Start()
        {
            _inst = this;
            _cam = Camera.main;
            _prefabDictionary = new Dictionary<PoolObjectKey, Stack<GameObject>>();
            _poolDictionary = new Dictionary<PoolObjectKey, Pool>();
            _uiPrefabDictionary = new Dictionary<UIPoolObjectKey, Stack<GameObject>>();
            _uiPoolDictionary = new Dictionary<UIPoolObjectKey, UIPool>();
            _enemyPrefabDictionary = new Dictionary<EnemyPoolObjectKey, Stack<GameObject>>();
            _enemyPoolDictionary = new Dictionary<EnemyPoolObjectKey, EnemyPool>();
            PoolInit();
            MatchPoolKeyToPrefabKey();
#if UNITY_EDITOR
            SortPool();
#endif
        }

        [ContextMenu("Sort Pool")]
        private void SortPool() => Array.Sort(pools);

        [ContextMenu("Init Prefab key from Pool")]
        private void MatchPoolKeyToPrefabKey()
        {
            for (var i = 0; i < pools.Length; i++)
            {
                pools[i].prefab.GetComponent<PoolObject>().PoolObjKey = pools[i].poolObjectKey;
            }

            for (var i = 0; i < enemyPools.Length; i++)
            {
                enemyPools[i].enemyPrefab.GetComponent<EnemyPoolObject>().enemyPoolObjKey =
                    enemyPools[i].enemyPoolObjKey;
            }

            for (int i = 0; i < uiPools.Length; i++)
            {
                uiPools[i].uiPrefab.GetComponent<UIPoolObject>().UIPoolObjKey = uiPools[i].uiPoolObjectKey;
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
                for (var j = 0; j < pool.initSize; j++)
                {
                    CreateNewObject(pool.poolObjectKey, pool.prefab);
                }
            }

            for (int i = 0; i < uiPools.Length; i++)
            {
                _uiPoolDictionary.Add(uiPools[i].uiPoolObjectKey, uiPools[i]);
                uiPools[i].uiPrefab.GetComponent<UIPoolObject>().UIPoolObjKey = uiPools[i].uiPoolObjectKey;
            }

            for (int i = 0; i < uiPools.Length; i++)
            {
                var uiPool = uiPools[i];
                if (uiPool.uiPrefab == null) throw new Exception($"{uiPool.uiPoolObjectKey} doesn't exist");
                _uiPrefabDictionary.Add(uiPool.uiPoolObjectKey, new Stack<GameObject>());
                for (int j = 0; j < uiPool.initSize; j++)
                {
                    CreateUIObject(uiPool.uiPoolObjectKey, uiPool.uiPrefab);
                }
            }

            for (var i = 0; i < enemyPools.Length; i++)
            {
                _enemyPoolDictionary.Add(enemyPools[i].enemyPoolObjKey, enemyPools[i]);
                enemyPools[i].enemyPrefab.GetComponent<EnemyPoolObject>().enemyPoolObjKey =
                    enemyPools[i].enemyPoolObjKey;
            }

            for (var i = 0; i < enemyPools.Length; i++)
            {
                var enemyPool = enemyPools[i];
                if (enemyPool.enemyPrefab == null) throw new Exception($"{enemyPool.enemyPoolObjKey} doesn't exist");
                _enemyPrefabDictionary.Add(enemyPool.enemyPoolObjKey, new Stack<GameObject>());
                for (var j = 0; j < enemyPool.initSize; j++)
                {
                    CreateEnemyObject(enemyPool.enemyPoolObjKey, enemyPool.enemyPrefab);
                }
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
            throw new Exception($"{poolObjectKey} Component not found");
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, rotation);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(UIPoolObjectKey uiPoolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(uiPoolObjectKey, _inst._cam.WorldToScreenPoint(position), Quaternion.identity);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(EnemyPoolObjectKey enemyPoolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(enemyPoolObjectKey, position, Quaternion.identity);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static void ReturnToPool(GameObject obj, PoolObjectKey poolObjKey)
        {
            if (!_inst._poolDictionary.TryGetValue(poolObjKey, out _)) return;
            _inst._prefabDictionary[poolObjKey].Push(obj);
        }

        public static void ReturnToPool(GameObject obj, UIPoolObjectKey uiPoolObjectKey)
        {
            if (!_inst._uiPoolDictionary.TryGetValue(uiPoolObjectKey, out _)) return;
            _inst._uiPrefabDictionary[uiPoolObjectKey].Push(obj);
        }

        public static void ReturnToPool(GameObject obj, EnemyPoolObjectKey enemyPoolObjectKey)
        {
            if (!_inst._enemyPoolDictionary.TryGetValue(enemyPoolObjectKey, out _)) return;
            _inst._enemyPrefabDictionary[enemyPoolObjectKey].Push(obj);
        }

        public static async UniTaskVoid PoolCleaner()
        {
            foreach (var poolKey in _inst._prefabDictionary.Keys)
            {
                var outOfRange = _inst._prefabDictionary[poolKey].Count > _inst.poolMaxSize;

                while (outOfRange)
                {
                    Destroy(_inst._prefabDictionary[poolKey].Pop());
                    outOfRange = _inst._prefabDictionary[poolKey].Count > _inst.poolMaxSize;
                    await UniTask.Delay(100);
                }
            }

            foreach (var key in _inst._enemyPrefabDictionary.Keys)
            {
                var outOfRange = _inst._enemyPrefabDictionary[key].Count > _inst.poolMaxSize;

                while (outOfRange)
                {
                    Destroy(_inst._enemyPrefabDictionary[key].Pop());
                    outOfRange = _inst._enemyPrefabDictionary[key].Count > _inst.poolMaxSize;
                    await UniTask.Delay(100);
                }
            }
        }

        private GameObject Spawn(PoolObjectKey poolObjKey, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            if (!_prefabDictionary.TryGetValue(poolObjKey, out _))
                Debug.Log($"Pool doesn't exist {poolObjKey}");
#endif
            var poolStack = _prefabDictionary[poolObjKey];
            if (poolStack.Count <= 0)
            {
                _poolDictionary.TryGetValue(poolObjKey, out var pool);
#if UNITY_EDITOR
                if (pool == null) throw new Exception($"Pool with tag {poolObjKey} doesn't exist.");
#endif
                CreateNewObject(poolObjKey, pool.prefab);
            }

            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.SetActive(true);
            return poolObj;
        }

        private GameObject Spawn(UIPoolObjectKey uiPoolObjectKey, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            if (!_uiPrefabDictionary.TryGetValue(uiPoolObjectKey, out _))
                Debug.Log($"Pool doesn't exist {uiPoolObjectKey}");
#endif
            var uiPoolStack = _uiPrefabDictionary[uiPoolObjectKey];
            if (uiPoolStack.Count <= 0)
            {
                _uiPoolDictionary.TryGetValue(uiPoolObjectKey, out var uiPool);
#if UNITY_EDITOR
                if (uiPool == null) throw new Exception($"Pool with tag {uiPoolObjectKey} doesn't exist.");
#endif
                CreateUIObject(uiPoolObjectKey, uiPool.uiPrefab);
            }

            var uiPoolObj = uiPoolStack.Pop();
            uiPoolObj.transform.SetPositionAndRotation(position, rotation);
            uiPoolObj.SetActive(true);
            return uiPoolObj;
        }

        private GameObject Spawn(EnemyPoolObjectKey enemyPoolObjectKey, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            if (!_enemyPrefabDictionary.ContainsKey(enemyPoolObjectKey))
                throw new Exception($"Pool with tag {enemyPoolObjectKey} doesn't exist.");
#endif
            var enemyPoolStack = _enemyPrefabDictionary[enemyPoolObjectKey];
            if (enemyPoolStack.Count <= 0)
            {
                _enemyPoolDictionary.TryGetValue(enemyPoolObjectKey, out var enemyPool);
#if UNITY_EDITOR
                if (enemyPool == null) throw new Exception($"Pool with tag {enemyPoolObjectKey} doesn't exist.");
#endif
                CreateEnemyObject(enemyPoolObjectKey, enemyPool.enemyPrefab);
            }

            var enemyPoolObj = enemyPoolStack.Pop();
            enemyPoolObj.transform.SetPositionAndRotation(position, rotation);
            enemyPoolObj.SetActive(true);
            return enemyPoolObj;
        }

        private void CreateNewObject(PoolObjectKey poolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            if (!obj.TryGetComponent(out PoolObject poolObject))
                throw new Exception($"You have to attach PoolObject.cs in {prefab} prefab");
            poolObject.PoolObjKey = poolObjectKey;
            obj.name = poolObjectKey.ToString();
            obj.SetActive(false);
        }

        private void CreateUIObject(UIPoolObjectKey uiPoolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, canvas);
            if (!obj.TryGetComponent(out UIPoolObject uiPoolObject))
                throw new Exception($"You have to attach PoolObject.cs in {prefab} prefab");
            uiPoolObject.UIPoolObjKey = uiPoolObjectKey;
            obj.name = uiPoolObjectKey.ToString();
            obj.SetActive(false);
        }

        private void CreateEnemyObject(EnemyPoolObjectKey enemyPoolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            if (!obj.TryGetComponent(out EnemyPoolObject enemyPoolObject))
                throw new Exception($"You have to attach EnemyPoolObject.cs in {prefab} prefab");
            enemyPoolObject.enemyPoolObjKey = enemyPoolObjectKey;
            obj.name = enemyPoolObjectKey.ToString();
            obj.SetActive(false);
        }
#if UNITY_EDITOR
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
#endif
    }
}