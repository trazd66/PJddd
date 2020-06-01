using UnityEngine;

using System.Collections.Generic;
using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;

namespace MapGen{
    enum tileType
    {
        floor = 0,
        border = 1,
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
        public int randomFillPercent;

        public NativeString64 seed;
        //add others later
    }


    public class Map{

        public int width;
        public int height;
        public int[,] tileMap {get;}
        List<Room> rooms;
        
        public Map(int width,int height, int[,] tileMap){
            this.width = width;
            this.height = height;
            this.tileMap = tileMap;
        }

        struct Coord {
            public int x;
            public int y;
            public Coord(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        class Room {
            public List<Coord> tiles;
            public List<Coord> edgeTiles;

            public Room(List<Coord> roomTiles, int[,] map) {
                tiles = roomTiles;

                edgeTiles = new List<Coord>();
                //a tile is considered to be an edge tile when one of its neighbor is a border tile
                foreach (Coord tile in tiles) {
                    for (int x = tile.x - 1; x <= tile.x + 1; x++) {
                        for (int y = tile.y - 1; y <= tile.y + 1; y++) {
                            if (x == tile.x || y == tile.y) {
                                if (map[x,y] == (int) tileType.border) {
                                    edgeTiles.Add(tile);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
