using System;
using System.Collections.Generic;
using UnityEngine;

// #if UNITY_EDITOR
// using UnityEditor;

namespace GameControl
{
//     [CustomEditor(typeof(StackObjectPool))]
//     public class StackObjectPoolingEditor : Editor
//     {
//         private const string Info = "풀링한 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
//                                     "    StackObjectPooler.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
//                                     "    CancelInvoke();    // Monobehaviour에 Invoke가 있다면 \n}";
//
//         public override void OnInspectorGUI()
//         {
//             EditorGUILayout.HelpBox(Info, MessageType.Info);
//             base.OnInspectorGUI();
//         }
//     }
// #endif
    public class StackObjectPool : MonoBehaviour
    {
        [Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        private static StackObjectPool inst;
        [SerializeField] private Pool[] pools;
        private Dictionary<string, Stack<GameObject>> _poolDictionary;

        private readonly string _info = " 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
                                        "    ObjectPooling.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
                                        "    CancelInvoke();    // Mono behaviour에 Invoke가 있다면 \n}";

        private void Awake()
        {
            inst = this;
            _poolDictionary = new Dictionary<string, Stack<GameObject>>();
            //미리 생성
            foreach (var pool in pools)
            {
                _poolDictionary.Add(pool.tag, new Stack<GameObject>());
                for (var i = 0; i < pool.size; i++)
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

        public static GameObject Get(string tag, Vector3 position) =>
            inst.Spawn(tag, position, Quaternion.identity);

        public static GameObject Get(string tag, Vector3 position, Quaternion rotation) =>
            inst.Spawn(tag, position, rotation);

        public static T Get<T>(string tag, Vector3 position) where T : Component
        {
            var obj = inst.Spawn(tag, position, Quaternion.identity);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception("Component not found");
        }

        public static T Get<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = inst.Spawn(tag, position, rotation);
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"Component not found");
        }

        private GameObject Spawn(string objTag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(objTag))
                throw new Exception($"Pool with tag {objTag} doesn't exist.");

            //스택에 없으면 새로 추가
            var poolStack = _poolDictionary[objTag];
            if (poolStack.Count <= 0)
            {
                var pool = Array.Find(pools, x => x.tag == objTag);
                var obj = CreateNewObject(pool.tag, pool.prefab);
                SortObject(obj);
            }

            //스택에서 꺼내 사용
            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.SetActive(true);
            return poolObj;
        }

        public static void ReturnToPool(GameObject obj)
        {
            if (!inst._poolDictionary.ContainsKey(obj.name))
                throw new Exception($"Pool with tag {obj.name} doesn't exist.");
            inst._poolDictionary[obj.name].Push(obj);
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