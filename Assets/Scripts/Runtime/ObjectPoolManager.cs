using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;


//Based on this video: https://www.youtube.com/watch?v=Ah3epb2HGCw 
public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private bool addToDontDestroyOnLoad = false;

    private GameObject emptyHolder;

    private static GameObject UI;
    private static GameObject gameObjects;

    private static Dictionary<GameObject, ObjectPool<GameObject>> objectPools;
    private static Dictionary<GameObject, GameObject> cloneToPrefabMap;

    public enum PoolType
    {
        UI,
        GameObjects
    }
    public static PoolType PoolingType;

    private void Awake()
    {
        objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetupEmpties();
    }

    private void SetupEmpties()
    {
        emptyHolder = new GameObject("Object pools");

        UI = new GameObject("UI");
        UI.transform.SetParent(emptyHolder.transform);

        gameObjects = new GameObject("Game Objects");
        gameObjects.transform.SetParent(emptyHolder.transform);

        if (addToDontDestroyOnLoad) DontDestroyOnLoad(UI.transform.root);
    }

    //For setting parent
    private static void CreatePool(GameObject prefab, Transform parent, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, parent, rot, pooltype),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject);

        objectPools.Add(prefab, pool);
    }

    
    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateObject(prefab, pos, rot, pooltype),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject);

        objectPools.Add(prefab, pool);
    }

    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, pos, rot);

        prefab.SetActive(true);

        GameObject parentObject = SetParentObject(pooltype);
        obj.transform.SetParent(parentObject.transform);

        return obj;
    }

    //For setting parent
    private static GameObject CreateObject(GameObject prefab, Transform parent, Quaternion rot, PoolType pooltype = PoolType.GameObjects)
    {
        prefab.SetActive(false);

        GameObject obj = Instantiate(prefab, parent);

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = rot;
        obj.transform.localScale = Vector3.one;

        prefab.SetActive(true);

        return obj;
    }

    private static void OnGetObject(GameObject obj)
    {
        //Optional logic
    }

    private static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    private static void OnDestroyObject(GameObject obj)
    {
        if(cloneToPrefabMap.ContainsKey(obj))
        {
            cloneToPrefabMap.Remove(obj);
        }
    }

    private static GameObject SetParentObject(PoolType pooltype)
    {
        switch (pooltype)
        {
            case PoolType.UI:
                return UI;

            case PoolType.GameObjects:

                return gameObjects;

            default:
                return null;
        }
    }

    private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation = new Quaternion(), PoolType poolType = PoolType.GameObjects) where T : Object
    {
        if (!objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
        }

        GameObject obj = objectPools[objectToSpawn].Get();

        if (obj != null)
        {
            if (!cloneToPrefabMap.ContainsKey(obj))
            {
                cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.position = spawnPos;
            obj.transform.rotation = spawnRotation;
            obj.SetActive(true);

            if(typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();
            if(component == null)
            {
                Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}");
                return null;
            }

            return component;
        }

        return null;
    }

    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos = new Vector3(), Quaternion spawnRotation = new Quaternion(), PoolType poolType = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos = new Vector3(), Quaternion spawnRotation = new Quaternion(), PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    }

    //For setting parent
    private static T SpawnObject<T>(GameObject objectToSpawn, Transform parent, Quaternion spawnRotation = new Quaternion(), PoolType poolType = PoolType.GameObjects) where T : Object
    {
        if (!objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, parent, spawnRotation, poolType);
        }

        GameObject obj = objectPools[objectToSpawn].Get();

        if (obj != null)
        {
            if (!cloneToPrefabMap.ContainsKey(obj))
            {
                cloneToPrefabMap.Add(obj, objectToSpawn);
            }

            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = spawnRotation;
            obj.SetActive(true);

            if (typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }

            T component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"Object {objectToSpawn.name} doesn't have component of type {typeof(T)}");
                return null;
            }

            return component;
        }

        return null;
    }

    public static T SpawnObject<T>(T typePrefab, Transform parent, Quaternion spawnRotation = new Quaternion(), PoolType poolType = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, parent, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Transform parent, Quaternion spawnRotation = new Quaternion(), PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, parent, spawnRotation, poolType);
    }

    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if (cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = SetParentObject(poolType);

            if (obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform);
            }

            if(objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name, obj);
        }

    }
}
