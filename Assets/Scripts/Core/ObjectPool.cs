using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initial = 10;
    Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < initial; i++) CreateOne();
    }

    GameObject CreateOne()
    {
        var go = Instantiate(prefab, transform);
        go.SetActive(false);
        pool.Enqueue(go);
        return go;
    }

    public GameObject Get()
    {
        if (pool.Count == 0) CreateOne();
        var obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
