using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ObjectPooler))]
public class ObjectPoolerEditor : Editor
{
    private const string Info = "풀링한 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" +
                                "    ObjectPooler.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" +
                                "    CancelInvoke();    // Monobehaviour에 Invoke가 있다면 \n}";

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(Info, MessageType.Info);
        base.OnInspectorGUI();
    }
}
#endif

public class ObjectPooler : MonoBehaviour
{
    private static ObjectPooler _inst;
    private void Awake() => _inst = this;

    [Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] private Pool[] pools;
    private List<GameObject> spawnObjects;
    Dictionary<string, Queue<GameObject>> poolDictionary;

    private const string Info = " 오브젝트에 다음을 적으세요 \nvoid OnDisable()\n{\n" + "    ObjectPooler.ReturnToPool(gameObject);    // 한 객체에 한번만 \n" + "    CancelInvoke();    // Monobehaviour에 Invoke가 있다면 \n}";


    public static GameObject SpawnFromPool(string tag, Vector3 position) =>
        _inst._SpawnFromPool(tag, position, Quaternion.identity);

    public static GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) =>
        _inst._SpawnFromPool(tag, position, rotation);

    public static T SpawnFromPool<T>(string tag, Vector3 position) where T : Component
    {
        GameObject obj = _inst._SpawnFromPool(tag, position, Quaternion.identity);
        if (obj.TryGetComponent(out T component))
            return component;
        else
        {
            obj.SetActive(false);
            throw new Exception($"Component not found");
        }
    }

    public static T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject obj = _inst._SpawnFromPool(tag, position, rotation);
        if (obj.TryGetComponent(out T component))
            return component;
        else
        {
            obj.SetActive(false);
            throw new Exception($"Component not found");
        }
    }

    public static List<GameObject> GetAllPools(string tag)
    {
        if (!_inst.poolDictionary.ContainsKey(tag))
            throw new Exception($"Pool with tag {tag} doesn't exist.");

        return _inst.spawnObjects.FindAll(x => x.name == tag);
    }

    public static List<T> GetAllPools<T>(string tag) where T : Component
    {
        var objects = GetAllPools(tag);

        // if (!objects[0].TryGetComponent(out T component))
        //     throw new Exception("Component not found");

        return objects.ConvertAll(x => x.GetComponent<T>());
    }

    public static void ReturnToPool(GameObject obj)
    {
        if (!_inst.poolDictionary.ContainsKey(obj.name))
            throw new Exception($"Pool with tag {obj.name} doesn't exist.");

        _inst.poolDictionary[obj.name].Enqueue(obj);
    }

    [ContextMenu("GetSpawnObjectsInfo")]
    void GetSpawnObjectsInfo()
    {
        var poolsLength = pools.Length;
        for (var i = 0; i < poolsLength; i++)
        {
            var pool = pools[i];
            var count = spawnObjects.FindAll(x => x.name == pool.tag).Count;
            Debug.Log($"{pool.tag} count : {count}");
        }
    }

    private GameObject _SpawnFromPool(string nameTag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(nameTag))
            throw new Exception($"Pool with tag {nameTag} doesn't exist.");

        // 큐에 없으면 새로 추가
        Queue<GameObject> poolQueue = poolDictionary[nameTag];
        if (poolQueue.Count <= 0)
        {
            Pool pool = Array.Find(pools, x => x.tag == nameTag);
            var obj = CreateNewObject(pool.tag, pool.prefab);
            ArrangePool(obj);
        }

        // 큐에서 꺼내서 사용
        GameObject objectToSpawn = poolQueue.Dequeue();
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    void Start()
    {
        spawnObjects = new List<GameObject>();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // 미리 생성
        var poolsLength = pools.Length;
        for (int i1 = 0; i1 < poolsLength; i1++)
        {
            Pool pool = pools[i1];
            poolDictionary.Add(pool.tag, new Queue<GameObject>());
            var poolSize = pool.size;
            for (int i = 0; i < poolSize; i++)
            {
                var obj = CreateNewObject(pool.tag, pool.prefab);
                ArrangePool(obj);
            }

            // OnDisable에 ReturnToPool 구현여부와 중복구현 검사
            if (poolDictionary[pool.tag].Count <= 0)
                Debug.LogError($"{pool.tag}{Info}");
            else if (poolDictionary[pool.tag].Count != pool.size)
                Debug.LogError($"{pool.tag}에 ReturnToPool이 중복됩니다");
        }
    }

    private GameObject CreateNewObject(string nameTag, GameObject prefab)
    {
        var obj = Instantiate(prefab, transform);
        obj.name = nameTag;
        obj.SetActive(false); // 비활성화시 ReturnToPool을 하므로 Enqueue가 됨
        return obj;
    }

    void ArrangePool(GameObject obj)
    {
        // 추가된 오브젝트 묶어서 정렬
        var isFind = false;
        var transformChildCount = transform.childCount;
        for (var i = 0; i < transformChildCount; i++)
        {
            if (i == transform.childCount - 1)
            {
                obj.transform.SetSiblingIndex(i);
                spawnObjects.Insert(i, obj);
                break;
            }
            else if (transform.GetChild(i).name == obj.name)
                isFind = true;
            else if (isFind)
            {
                obj.transform.SetSiblingIndex(i);
                spawnObjects.Insert(i, obj);
                break;
            }

        }
    }
}
