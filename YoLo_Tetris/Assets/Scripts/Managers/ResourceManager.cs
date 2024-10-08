using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed To Load Prefab : {path}");
            return null;
        }
        return Object.Instantiate(original, parent);
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
        {
            Debug.Log("Can not be Destroy. this is null.");
            return;
        }
        else
        {
            Object.Destroy(go);
        }
    }
}
