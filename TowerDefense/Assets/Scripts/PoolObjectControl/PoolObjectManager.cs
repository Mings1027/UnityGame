using System;
using System.Collections.Generic;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.MonsterDataControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace PoolObjectControl
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
    public class MonsterPool : IComparable<MonsterPool>
    {
        public MonsterPoolObjectKey monsterPoolObjKey;
        public GameObject monsterPrefab;
        public byte initSize;

        public int CompareTo(MonsterPool other)
        {
            return string.CompareOrdinal(monsterPoolObjKey.ToString(), other.monsterPoolObjKey.ToString());
        }
    }

    public class PoolObjectManager : MonoBehaviour
    {
        private static PoolObjectManager _inst;
        private Camera _cam;
        private Dictionary<PoolObjectKey, Stack<GameObject>> _prefabDictionary;
        private Dictionary<PoolObjectKey, Pool> _poolDictionary;
        private Dictionary<UIPoolObjectKey, Stack<GameObject>> _uiPrefabDictionary;
        private Dictionary<UIPoolObjectKey, UIPool> _uiPoolDictionary;
        private Dictionary<MonsterPoolObjectKey, Stack<GameObject>> _monsterPrefabDictionary;
        private Dictionary<MonsterPoolObjectKey, MonsterPool> _monsterPoolDictionary;

        [SerializeField] private Transform canvas;
        [SerializeField] private byte poolMaxSize;
        [SerializeField] private Pool[] pools;
        [SerializeField] private UIPool[] uiPools;
        [SerializeField] private MonsterPool[] monsterPools;

        private void Start()
        {
            _inst = this;
            _cam = Camera.main;
            _prefabDictionary = new Dictionary<PoolObjectKey, Stack<GameObject>>();
            _poolDictionary = new Dictionary<PoolObjectKey, Pool>();
            _uiPrefabDictionary = new Dictionary<UIPoolObjectKey, Stack<GameObject>>();
            _uiPoolDictionary = new Dictionary<UIPoolObjectKey, UIPool>();
            _monsterPrefabDictionary = new Dictionary<MonsterPoolObjectKey, Stack<GameObject>>();
            _monsterPoolDictionary = new Dictionary<MonsterPoolObjectKey, MonsterPool>();
            PoolInit();
            // MatchPoolKeyToPrefabKey();
#if UNITY_EDITOR
            SortPool();
#endif
        }
#if UNITY_EDITOR
        [ContextMenu("Sort Pool")]
        private void SortPool() => Array.Sort(pools);

        [ContextMenu("Set Prefab key from Pool")]
        private void MatchPoolKeyToPrefabKey()
        {
            foreach (var t in pools)
            {
                t.prefab.GetComponent<PoolObject>().PoolObjKey = t.poolObjectKey;
            }

            foreach (var t in uiPools)
            {
                t.uiPrefab.GetComponent<UIPoolObject>().UIPoolObjKey = t.uiPoolObjectKey;
            }

            foreach (var t in monsterPools)
            {
                t.monsterPrefab.GetComponent<MonsterPoolObject>().MonsterPoolObjKey =
                    t.monsterPoolObjKey;
            }
        }
#endif
        private void PoolInit()
        {
            foreach (var t in pools)
            {
                _poolDictionary.Add(t.poolObjectKey, t);
                t.prefab.GetComponent<PoolObject>().PoolObjKey = t.poolObjectKey;

                if (t.prefab == null) throw new Exception($"{t.poolObjectKey} doesn't exist");
                _prefabDictionary.Add(t.poolObjectKey, new Stack<GameObject>());
                for (var j = 0; j < t.initSize; j++)
                {
                    CreateNewObject(t.poolObjectKey, t.prefab);
                }
            }

            foreach (var t in uiPools)
            {
                _uiPoolDictionary.Add(t.uiPoolObjectKey, t);
                t.uiPrefab.GetComponent<UIPoolObject>().UIPoolObjKey = t.uiPoolObjectKey;

                if (t.uiPrefab == null) throw new Exception($"{t.uiPoolObjectKey} doesn't exist");
                _uiPrefabDictionary.Add(t.uiPoolObjectKey, new Stack<GameObject>());
                for (var j = 0; j < t.initSize; j++)
                {
                    CreateUIObject(t.uiPoolObjectKey, t.uiPrefab);
                }
            }

            foreach (var t in monsterPools)
            {
                _monsterPoolDictionary.Add(t.monsterPoolObjKey, t);
                t.monsterPrefab.GetComponent<MonsterPoolObject>().MonsterPoolObjKey = t.monsterPoolObjKey;

                if (t.monsterPrefab == null) throw new Exception($"{t.monsterPoolObjKey} doesn't exist");
                _monsterPrefabDictionary.Add(t.monsterPoolObjKey, new Stack<GameObject>());
                for (var j = 0; j < t.initSize; j++)
                {
                    CreateMonsterObject(t.monsterPoolObjKey, t.monsterPrefab);
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
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, Quaternion.identity);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static T Get<T>(PoolObjectKey poolObjectKey, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = _inst.Spawn(poolObjectKey, position, rotation);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static void Get(UIPoolObjectKey uiPoolObjectKey, Vector3 position) =>
            _inst.Spawn(uiPoolObjectKey, position, Quaternion.identity);

        public static T Get<T>(UIPoolObjectKey uiPoolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(uiPoolObjectKey, _inst._cam.WorldToScreenPoint(position), Quaternion.identity);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{uiPoolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static T Get<T>(MonsterPoolObjectKey monsterPoolObjectKey, in Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(monsterPoolObjectKey, position, Quaternion.identity);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{monsterPoolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
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

        public static void ReturnToPool(GameObject obj, MonsterPoolObjectKey monsterPoolObjectKey)
        {
            if (!_inst._monsterPoolDictionary.TryGetValue(monsterPoolObjectKey, out _)) return;
            _inst._monsterPrefabDictionary[monsterPoolObjectKey].Push(obj);
        }

        public static async UniTaskVoid PoolCleaner()
        {
            foreach (var poolKey in _inst._prefabDictionary.Keys)
            {
                var outOfRange = _inst._prefabDictionary[poolKey].Count > _inst.poolMaxSize;

                while (outOfRange)
                {
                    print(_inst._prefabDictionary[poolKey].Peek());
                    Destroy(_inst._prefabDictionary[poolKey].Pop());
                    outOfRange = _inst._prefabDictionary[poolKey].Count > _inst.poolMaxSize;
                    await UniTask.Delay(100);
                }
            }

            // foreach (var key in _inst._monsterPrefabDictionary.Keys)
            // {
            //     var outOfRange = _inst._monsterPrefabDictionary[key].Count > _inst.poolMaxSize;
            //
            //     while (outOfRange)
            //     {
            //         print(_inst._monsterPrefabDictionary[key].Peek());
            //         Destroy(_inst._monsterPrefabDictionary[key].Pop());
            //         outOfRange = _inst._monsterPrefabDictionary[key].Count > _inst.poolMaxSize;
            //         await UniTask.Delay(100);
            //     }
            // }
        }

        public static async UniTaskVoid MonsterPoolCleaner(IEnumerable<NormalMonsterData> monsterData)
        {
            foreach (var t in monsterData)
            {
                var monsterKey = t.MonsterPoolObjectKey;
                var outOfRange = _inst._monsterPrefabDictionary[monsterKey].Count > 0;
                while (outOfRange)
                {
                    Destroy(_inst._monsterPrefabDictionary[monsterKey].Pop());
                    outOfRange = _inst._monsterPrefabDictionary[monsterKey].Count > 0;
                    await UniTask.Delay(100);
                }
            }
        }

        private GameObject Spawn(PoolObjectKey poolObjKey, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            // if (!_prefabDictionary.TryGetValue(poolObjKey, out _))
            //     Debug.Log($"Pool doesn't exist {poolObjKey}");
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
            // if (!_uiPrefabDictionary.TryGetValue(uiPoolObjectKey, out _))
            //     Debug.Log($"Pool doesn't exist {uiPoolObjectKey}");
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

        private GameObject Spawn(MonsterPoolObjectKey monsterPoolObjectKey, Vector3 position, Quaternion rotation)
        {
#if UNITY_EDITOR
            if (!_monsterPrefabDictionary.ContainsKey(monsterPoolObjectKey))
                throw new Exception($"Pool with tag {monsterPoolObjectKey} doesn't exist.");
#endif
            var monsterPoolStack = _monsterPrefabDictionary[monsterPoolObjectKey];
            if (monsterPoolStack.Count <= 0)
            {
                _monsterPoolDictionary.TryGetValue(monsterPoolObjectKey, out var monsterPool);
#if UNITY_EDITOR
                if (monsterPool == null) throw new Exception($"Pool with tag {monsterPoolObjectKey} doesn't exist.");
#endif
                CreateMonsterObject(monsterPoolObjectKey, monsterPool.monsterPrefab);
            }

            var monsterPoolObj = monsterPoolStack.Pop();
            monsterPoolObj.transform.SetPositionAndRotation(position, rotation);
            monsterPoolObj.SetActive(true);
            return monsterPoolObj;
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

        private void CreateMonsterObject(MonsterPoolObjectKey monsterPoolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            if (!obj.TryGetComponent(out MonsterPoolObject monsterPoolObject))
                throw new Exception($"You have to attach MonsterPoolObject.cs in {prefab} prefab");
            monsterPoolObject.MonsterPoolObjKey = monsterPoolObjectKey;
            obj.name = monsterPoolObjectKey.ToString();
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