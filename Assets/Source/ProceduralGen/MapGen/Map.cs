using UnityEngine;

using System.Collections.Generic;
using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;

namespace MapGen{

    public enum mapType
    {
        Cave,
        Dungeon,

    }

    public enum TileType
    {
        floor = 0,
        wall = 1,
        passage = 2,
        roomIntersection = 3,

    }


    public struct TileMapBufferElement : IBufferElementData
    {
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator int(TileMapBufferElement e) { return e.value; }
        public static implicit operator TileMapBufferElement(int e) { return new TileMapBufferElement { value = e }; }

        // Actual value each buffer element will store.
        public int value;
    }

    public class TileMapGeneration
    {
        public struct T_InitialGen : IComponentData
        {

        }

        public struct T_MeshGen : IComponentData
        {

        }

        public struct T_SpawnFlagGen : IComponentData
        {

        }

    }

    public struct MapGenRequirement : IComponentData
    {
        public int width;
        public int height;


        public Translation spawnlocation;

        public int randomFillPercent;
        public mapType mType;

        public float squareSize;

        public NativeString64 seed;
        //add others later
    }


    public enum mapTheme
    {
        test
    }

    public enum generationStage
    {
        tileGen,
        meshGen,
        meshPhysicsGen,
        spawnGen,
        ready,
        transferd,
    }

    public struct MapInfo : IComponentData
    {
        public Entity entity;
        public mapTheme theme;
        public NativeString64 seed;
        public generationStage genStage;
        public NativeString64 currWorldName;

        public void nextGenStage()
        {
            this.genStage++;
        }

    }



}
