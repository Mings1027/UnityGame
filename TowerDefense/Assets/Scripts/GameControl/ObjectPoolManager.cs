using System;
using System.Collections.Generic;
using TowerControl;
using UnityEngine;

namespace GameControl
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [Serializable]
        public class Pool : IComparable<Pool>
        {
            public string tag;
            public GameObject prefab;
            public int size;

            public int CompareTo(Pool other)
            {
                return string.Compare(tag, other.tag, StringComparison.Ordinal);
            }
        }

        [Serializable]
        public class UIPool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        private static ObjectPoolManager _inst;
        private Dictionary<string, Stack<GameObject>> _poolDictionary;
        private Transform canvasForUIPool;


        private readonly string _info = " 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
                                        "    ObjectPooling.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
                                        "    CancelInvoke();    // Mono behaviour에 Invoke가 있다면 \n}";

        [SerializeField] private Pool[] pools;
        [SerializeField] private UIPool[] uiPools;

        private void Awake()
        {
            Array.Sort(pools);

            _inst = this;
            _poolDictionary = new Dictionary<string, Stack<GameObject>>();
            canvasForUIPool = transform.GetChild(0);
            PoolInit();
            UIPoolInit();
        }

        private void PoolInit()
        {
            //미리 생성
            for (var i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];
                if (pool.prefab == null)
                    throw new Exception($"{pool.tag} doesn't exist");
                _poolDictionary.Add(pool.tag, new Stack<GameObject>());
                for (var j = 0; j < pool.size; j++)
                {
                    var obj = CreateNewObject(pool.tag, pool.prefab);
                    SortObject(obj);
                }

                if (_poolDictionary[pool.tag].Count <= 0)
                    print($"{pool.tag}{_info}");
                else if (_poolDictionary[pool.tag].Count != pool.size)
                    print($"{pool.tag}에 ReturnToPool이 중복됩니다.");
            }
        }

        private void UIPoolInit()
        {
            for (int i = 0; i < uiPools.Length; i++)
            {
                var uiPool = uiPools[i];
                if (uiPool.prefab == null)
                    throw new Exception($"{uiPool.tag} doesn't exist");
                _poolDictionary.Add(uiPool.tag, new Stack<GameObject>());
                for (int j = 0; j < uiPool.size; j++)
                {
                    CreateUIObject(uiPool.tag, uiPool.prefab);
                }

                if (_poolDictionary[uiPool.tag].Count <= 0)
                    print($"{uiPool.tag}{_info}");
                else if (_poolDictionary[uiPool.tag].Count != uiPool.size)
                    print($"{uiPool.tag}에 ReturnToPool이 중복됩니다.");
            }
        }

        public static GameObject Get(string tag, Transform t) =>
            _inst.Spawn(tag, t.position, t.rotation);

        public static GameObject Get(string tag, Vector3 position) =>
            _inst.Spawn(tag, position, Quaternion.identity);

        public static GameObject Get(string tag, Vector3 position, Quaternion rotation) =>
            _inst.Spawn(tag, position, rotation);

        public static T Get<T>(string tag, Transform t) where T : Component
        {
            var obj = _inst.Spawn(tag, t.position, t.rotation);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(string tag, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(tag, position, Quaternion.identity);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = _inst.Spawn(tag, position, rotation);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static GameObject GetUI(string tag) => _inst.UISpawn(tag);

        public static T GetUI<T>(string tag)
        {
            var obj = _inst.UISpawn(tag);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static void ReturnToPool(GameObject obj)
        {
            if (!_inst._poolDictionary.ContainsKey(obj.name))
                throw new Exception($"Pool with tag {obj.name} doesn't exist.");
            _inst._poolDictionary[obj.name].Push(obj);
        }

        private GameObject Spawn(string objTag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(objTag))
                throw new Exception($"Pool with tag {objTag} doesn't exist.");

            //스택에 없으면 새로 추가
            var poolStack = _poolDictionary[objTag];
            if (poolStack.Count <= 0)
            {
                var pool = GetPoolWithTag(objTag);
                if (pool == null)
                    throw new Exception($"Pool with tag {objTag} doesn't exist.");

                var obj = CreateNewObject(pool.tag, pool.prefab);
#if UNITY_EDITOR
                SortObject(obj);
#endif
            }

            //스택에서 꺼내 사용
            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.SetActive(true);
            return poolObj;
        }

        private GameObject UISpawn(string objTag)
        {
            if (!_poolDictionary.ContainsKey(objTag))
                throw new Exception($"Pool with tag {objTag} doesn't exist.");

            //스택에 없으면 새로 추가
            var poolStack = _poolDictionary[objTag];
            if (poolStack.Count <= 0)
            {
                var uiPool = GetUIPoolWithTag(objTag);
                if (uiPool == null)
                    throw new Exception($"Pool with tag {objTag} doesn't exist.");
                var obj = CreateUIObject(uiPool.tag, uiPool.prefab);
#if UNITY_EDITOR
                SortObject(obj);
#endif
            }

            var uiPoolObj = poolStack.Pop();
            uiPoolObj.SetActive(true);
            return uiPoolObj;
        }

        private Pool GetPoolWithTag(string objTag)
        {
            for (var i = 0; i < pools.Length; i++)
            {
                if (pools[i].tag == objTag)
                {
                    return pools[i];
                }
            }

            return null;
        }

        private UIPool GetUIPoolWithTag(string objTag)
        {
            for (var i = 0; i < uiPools.Length; i++)
            {
                if (uiPools[i].tag == objTag)
                {
                    return uiPools[i];
                }
            }

            return null;
        }

        private GameObject CreateNewObject(string objTag, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            obj.name = objTag;
            obj.SetActive(false);
            return obj;
        }

        private GameObject CreateUIObject(string objTag, GameObject prefab)
        {
            var obj = Instantiate(prefab, canvasForUIPool);
            obj.name = objTag;
            obj.SetActive(false);
            return obj;
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

                if (transform.GetChild(i).name == obj.name)
                    isFind = true;
                else if (isFind)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }
            }
        }
    }
}