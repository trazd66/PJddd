using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using MapGen;
using Unity.Collections;
using System.Runtime.InteropServices;

public class MapRepo
{

    enum mapTheme {
    
    }

    const float DEFAULT_SQUARE_SIZE = 1f;

    Dictionary<NativeString64, Dictionary<World, Entity>> mapPool;

    World renderWorld;
    World mapGenWorld;

    public MapRepo(World renderWorld, World mapGenWorld)
    {
        mapPool = new Dictionary<NativeString64, Dictionary<World, Entity>>();
        this.renderWorld = renderWorld;
        this.mapGenWorld = mapGenWorld;
    }

    public void generateRandomMap(
        mapType mType,
        int width, int height, 
        [Optional] NativeString64 seed, 
        [Optional] float squareSize)
    {
        seed = (seed.LengthInBytes == 0) ? Time.time.ToString() : seed;
        squareSize = (squareSize == 0) ? DEFAULT_SQUARE_SIZE : squareSize;

        Entity map = mapGenWorld.EntityManager.CreateEntity();
        mapGenWorld.EntityManager.AddComponentData(map, new MapGenRequirement { 
            width = width, 
            height = height,
            mType = mType,
            randomFillPercent = 38, 
            seed = seed, 
            squareSize = squareSize});

        mapGenWorld.EntityManager.AddComponentData(map, new T_TileMapGen { });
    }

    public void moveMapToRenderWorld()
    {

    }

    public void destroyMap()
    {

    }



}
