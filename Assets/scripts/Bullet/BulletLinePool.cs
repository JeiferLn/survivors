using System.Collections.Generic;
using UnityEngine;

public class BulletLinePool : MonoBehaviour
{
    [SerializeField] private GameObject bulletLinePrefab;
    [SerializeField] private int poolSize = 15;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletLinePrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            GameObject extra = Instantiate(bulletLinePrefab, transform);
            extra.SetActive(false);
            return extra;
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
