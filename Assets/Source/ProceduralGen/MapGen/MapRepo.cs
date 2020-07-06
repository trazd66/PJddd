using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using MapGen;
using Unity.Collections;
using System.Runtime.InteropServices;
using Unity.Assertions;
using Unity.Transforms;

public class MapRepo
{

   public MapInfo testMap;




    public class MapCollection
    {
        
    }

    const float DEFAULT_SQUARE_SIZE = 1f;


    World renderWorld;
    World mapGenWorld;

    public MapRepo(World renderWorld, World mapGenWorld)
    {
        this.renderWorld = renderWorld;
        this.mapGenWorld = mapGenWorld;
    }

    public MapInfo generateRandomMap(
        mapTheme theme,
        mapType mType,
        int width, int height, 
        [Optional] NativeString64 seed, 
        [Optional] float squareSize)
    {
        seed = (seed.LengthInBytes == 0) ? Time.realtimeSinceStartup.ToString() : seed;
        squareSize = (squareSize == 0) ? DEFAULT_SQUARE_SIZE : squareSize;


        Entity map = mapGenWorld.EntityManager.CreateEntity();
        mapGenWorld.EntityManager.AddComponentData(map, new MapGenRequirement { 
            width = width, 
            height = height,
            mType = mType,
            randomFillPercent = 45, 
            seed = seed, 
            squareSize = squareSize});

        var mapInfo = new MapInfo { entity = map, theme = theme, seed = seed, genStage = generationStage.tileGen };
        mapGenWorld.EntityManager.AddComponentData(map, new TileMapGeneration.T_InitialGen { });
        mapGenWorld.EntityManager.AddComponentData(map, new MapInfo {entity=map, theme=theme,seed = seed,genStage = generationStage.tileGen});
        return mapInfo;
    }

    public void moveMapToRenderWorld(ref MapInfo map)
    {
        var newEntityArr = new List<Entity>();
        var entitiesList = new List<Entity>();
        
        renderWorld.EntityManager.CopyEntitiesFrom(mapGenWorld.EntityManager, mapGenWorld.EntityManager.GetAllEntities(Allocator.Temp));

    }

    public void destroyMap(ref MapInfo map)
    {
    }



}
