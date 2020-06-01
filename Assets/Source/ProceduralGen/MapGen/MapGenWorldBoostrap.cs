using MapGen;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Sample.Core;
using UnityEngine;

public class MapGenWorldBoostrap
{
   public static void initMapGenWorld()
    {
        //Create world
        World mapGenWorld = WorldManager.getOrCreateWorld("MapGenWorld");
        //Create system groups
        var initSysGroup = mapGenWorld.GetOrCreateSystem<InitializationSystemGroup>();

        var ecbSystem = mapGenWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        var mapGenSys = mapGenWorld.CreateSystem<MapGenSystem>();
        initSysGroup.AddSystemToUpdateList(mapGenSys);
        initSysGroup.AddSystemToUpdateList(ecbSystem);

        initSysGroup.SortSystemUpdateList();


        //some testing
        Entity testingEntity = mapGenWorld.EntityManager.CreateEntity();
        mapGenWorld.EntityManager.AddComponentData(testingEntity, new MapGenRequirement { width = 50, height = 50, randomFillPercent = 50 , seed = "1001"});
        mapGenWorld.EntityManager.AddComponentData(testingEntity, new T_TileMapGen { });

    }
}
