using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pool. Adapted from Infinity Square Space ChunkCubesPool concept.
/// Pre-instantiates GameObjects and recycles them to avoid runtime allocation.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private Transform poolParent;

    private Queue<GameObject> available = new Queue<GameObject>();

    void Awake()
    {
        if (poolParent == null)
        {
            poolParent = new GameObject($"_Pool_{(prefab != null ? prefab.name : "null")}").transform;
            poolParent.SetParent(transform);
        }

        for (int i = 0; i < initialSize; i++)
            CreateInstance();
    }

    void CreateInstance()
    {
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, poolParent);
        obj.SetActive(false);
        available.Enqueue(obj);
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        if (available.Count == 0)
            CreateInstance();

        if (available.Count == 0) return null;

        GameObject obj = available.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        available.Enqueue(obj);
    }

    public void ReturnAll()
    {
        // Return all active children
        foreach (Transform child in poolParent)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                available.Enqueue(child.gameObject);
            }
        }
    }
}
