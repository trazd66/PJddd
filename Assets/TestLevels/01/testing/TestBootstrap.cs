using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using  MapGen;
using Unity.Sample.Core;
using Unity.Rendering;

public class TestBootstrap : MonoBehaviour
{

    MapRepo mapRepo;

    public void Initialize()
    {
        mapRepo = new MapRepo(WorldManager.getOrCreateWorld("DefaultWorld"),WorldManager.getOrCreateWorld("MapGenWorld"));
    }

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }


    // Update is called once per frame
    void Update()
    {
        World mapGenWorld = WorldManager.getOrCreateWorld("MapGenWorld");
        if (Input.GetKeyDown(KeyCode.A))
        {
            mapGenWorld.EntityManager.DestroyEntity(mapGenWorld.EntityManager.GetAllEntities());
            mapRepo.generateRandomMap(mapType.Cave, 200, 100);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.CopyAndReplaceEntitiesFrom(mapGenWorld.EntityManager);
        }
    }
}
