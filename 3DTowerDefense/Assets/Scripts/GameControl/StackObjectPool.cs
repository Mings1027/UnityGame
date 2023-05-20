using System;
using System.Collections.Generic;
using TowerControl;
using UnityEngine;

namespace GameControl
{
    public class StackObjectPool : Singleton<StackObjectPool>
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

        private static StackObjectPool _inst;
        private Dictionary<string, Stack<GameObject>> _poolDictionary;

        private readonly string _info = " 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
                                        "    ObjectPooling.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
                                        "    CancelInvoke();    // Mono behaviour에 Invoke가 있다면 \n}";

        [SerializeField] private Pool[] pools;

        private void Awake()
        {
            Array.Sort(pools);

            _inst = Instance;
            _poolDictionary = new Dictionary<string, Stack<GameObject>>();
            //미리 생성
            for (var i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];
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

        private GameObject CreateNewObject(string objTag, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
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

        public static void ReturnToPool(GameObject obj)
        {
            if (!_inst._poolDictionary.ContainsKey(obj.name))
                throw new Exception($"Pool with tag {obj.name} doesn't exist.");
            _inst._poolDictionary[obj.name].Push(obj);
        }
    }
}