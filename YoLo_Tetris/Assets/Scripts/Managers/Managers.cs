using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : Singleton <Managers>
{
    ResourceManager _resource = new ResourceManager();
    public static ResourceManager Resource { get { return Instance._resource; } }

    SceneManagerEx _scene = new SceneManagerEx();
    public static SceneManagerEx Scene { get { return Instance._scene; } }

    void Start()
    {
        Init();
        
    }

    void Update()
    {
        
    }

    protected override void Init()
    {
        throw new System.NotImplementedException();
    }
}
