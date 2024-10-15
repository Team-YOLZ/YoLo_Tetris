using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Managers : Singleton <Managers>
{
    ResourceManager _resource = new ResourceManager();
    public static ResourceManager Resource { get { return Instance._resource; } }

    SceneManagerEx _scene = new SceneManagerEx();
    public static SceneManagerEx Scene { get { return Instance._scene; } }

    //InputManager _input = new InputManager();
    //public static InputManager Input { get { return Instance._input; } }

    void Start()
    {
        
    }

    void Update()
    {
        //_input.OnUpdate();
    }

    protected override void Init()
    {
        //_input.Init();
        Debug.Log("Managers Init..");
    }
}
