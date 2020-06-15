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

    enum TileType
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

    public struct T_TileMapGen : IComponentData
    {

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


}
