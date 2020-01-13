using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class StaticResourceManager : MonoBehaviour
{

    public static StaticResourceManager S { get; private set; }

    private ProxyEntityCollection entityCollection {get; set; }

    /*this must execute before mainWorld does its initialization, see script execution order in setting*/
    private void Awake()
    {
        if (S == null) {//init the static manager
            S = this;
            entityCollection = new ProxyEntityCollection();
            ECSWorldManager.CurrentWorld = World.Active;
        }
        entityCollection.Add("mc", GameObjectConversionUtility.ConvertGameObjectHierarchy(mainCharacterGameObject, ECSWorldManager.CurrentWorld));
        entityCollection.Add("test", GameObjectConversionUtility.ConvertGameObjectHierarchy(testGameObject, ECSWorldManager.CurrentWorld));

    }

    public Entity getPrefabEntity(string name)
    {
        return entityCollection.Get(name);
    }

    public GameObject testGameObject;

    public GameObject mainCharacterGameObject;



}
