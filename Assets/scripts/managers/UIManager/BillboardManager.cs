using System.Collections.Generic;
using UnityEngine;

public class BillboardManager : MonoBehaviour
{
    public static BillboardManager Instance;
    public Transform test;
    private List<Transform> billboards = new List<Transform>();

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Register(test);
    }
    
    public void Register(Transform t)
    {
        if (!billboards.Contains(t))
            billboards.Add(t);
    }

    public void Unregister(Transform t)
    {
        billboards.Remove(t);
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 camPos = Camera.main.transform.position;

        for (int i = 0; i < billboards.Count; i++)
        {
            var obj = billboards[i];
            if (obj == null) continue;

            obj.LookAt(camPos);

            // 🔧 Girar 180° en Y para corregir texto invertido
            obj.Rotate(0f, 180f, 0f, Space.Self);
        }
    }
}