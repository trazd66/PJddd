using UnityEngine;

using System.Collections.Generic;
using System;

namespace MapGen{
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

        public static Map generateRandomMap(int width, int height, int randomFillPercent, String seed=null){
            Map map = new Map(width, height, getRandomFilledMap(width, height, randomFillPercent, seed));
            map.SmoothMap();
            return map;
        }

        public static int[,] getRandomFilledMap(int width, int height, int randomFillPercent, String seed=null) {
            int[,] map = new int[width,height];

            if (seed == null) {
                seed = System.DateTime.UtcNow.ToString();
            }

            System.Random pseudoRandom = new System.Random(seed.GetHashCode());

            for (int x = 0; x < width; x ++) {
                for (int y = 0; y < height; y ++) {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                        map[x,y] = (int) tileType.border;
                    }else {
                        map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent)? (int)tileType.border: (int)tileType.floor;
                    }
                }
            }
            return map;
        }
        
        /*
            helper method for smoothing the maps,for example by turning
            * * *    * * *
            *   * => * * * and vice versa
            * * *    * * *
        */
        void SmoothMap() {
            for (int x = 0; x < width; x ++) {
                for (int y = 0; y < height; y ++) {
                    int neighbourWallTiles = GetSurroundingWallCount(x,y);

                    //below are parts that can be modified to get different desired rooms
                    if (neighbourWallTiles > 4)
                        tileMap[x,y] = (int) tileType.border;
                    else if (neighbourWallTiles < 4)
                        tileMap[x,y] = (int) tileType.floor;

                }
            }
        }

        /*
            helper method for detecing how many surrounding tiles are also walls
                                * * *
                                * * *            use this diagram for intuition
                                * * *
        */
        int GetSurroundingWallCount(int gridX, int gridY) {
            int wallCount = 0;
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                    if (IsInMapRange(neighbourX, neighbourY)) {
                        if (neighbourX != gridX || neighbourY != gridY) {
                            wallCount += tileMap[neighbourX, neighbourY];
                        }
                    }else {
                        wallCount ++;
                    }
                }
            }
            return wallCount;
        }
        
        
        bool IsInMapRange(int x, int y) {
            return x >= 0 && x < width && y >= 0 && y < height;
        }


        enum tileType{
            floor = 0,
            border = 1,
            passage = 2,
            roomIntersection = 3,
            
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
