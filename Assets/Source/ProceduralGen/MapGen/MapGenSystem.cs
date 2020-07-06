using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using static Unity.Mathematics.math;

using MapGen;
using System.Linq;
using System;
using Random = Unity.Mathematics.Random;
using System.Collections.Generic;
using UnityEngine;

[DisableAutoCreation]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class MapGenSystem : SystemBase
{

    private BeginInitializationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        // Acquire an ECB and convert it to a concurrent one to be able
        // to use it from a parallel job.
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();
        Entities.WithAny<TileMapGeneration.T_InitialGen>().ForEach(
            (Entity entity, int entityInQueryIndex, ref MapGenRequirement mapGenRequirement, ref MapInfo mapInfo) =>
        {
            var tileMapBuffer = ecb.AddBuffer<TileMapBufferElement>(entityInQueryIndex, entity);

            createTileMap(in mapGenRequirement, ref tileMapBuffer);
            SmoothMap(in mapGenRequirement, ref tileMapBuffer);
            mapAllIslands(in mapGenRequirement, ref tileMapBuffer);
            SmoothMap(in mapGenRequirement, ref tileMapBuffer, 1);

            ecb.RemoveComponent<TileMapGeneration.T_InitialGen>(entityInQueryIndex, entity);//TileMapGeneartion finished
            ecb.AddComponent<TileMapGeneration.T_MeshGen>(entityInQueryIndex, entity);//Request starting mesh generation
        }
        ).ScheduleParallel();
        ecbSystem.AddJobHandleForProducer(this.Dependency);

    }


    static void createTileMap(in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer)
    {
        int width = mapGenRequirement.width;
        int height = mapGenRequirement.height;


        //TODO: Better Random management
        Random pseudoRandom = new Random((uint)mapGenRequirement.seed.GetHashCode());

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    tileMapBuffer.Add((int)TileType.wall);
                }
                else
                {
                    tileMapBuffer.Add(((int)pseudoRandom.NextUInt(0, 100) < mapGenRequirement.randomFillPercent) ? (int)TileType.floor : (int)TileType.wall);
                }
            }
        }
    }

    static void SmoothMap(in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer, int iter = 5)
    {
        int width = mapGenRequirement.width;
        int height = mapGenRequirement.height;

        Random pseudoRandom = new Random((uint)mapGenRequirement.seed.GetHashCode());

        for (int i = 0; i < iter; i++)
        {
            for (int y = 0; y < height; y++)
            {
                int index_y = width * y;
                for (int x = 0; x < width; x++)
                {
                    int index = x + index_y;
                    int neighbourWallTiles_r1 = MapGenHelper.GetSurroundingWallCount(x, y, width, height, tileMapBuffer);
                    int neighbourWallTiles_r2 = MapGenHelper.GetSurroundingWallCount(x, y, width, height, tileMapBuffer, 2);


                    if (i < 3)
                    {
                        tileMapBuffer[index] = (neighbourWallTiles_r1 + tileMapBuffer[index] > 4) ? (int)TileType.wall : (int)TileType.floor;
                    }
                    else
                    {
                        if (neighbourWallTiles_r2 + tileMapBuffer[index] < 14)
                        {
                            tileMapBuffer[index] = (int)TileType.floor;
                        }
                    }


                }
            }
        }


        

    }

    class Island : IComparable<Island>, IEquatable<Island>
    {
        public int islandCentre;
        public NativeList<int> islandTiles;
        public List<Island> connectedIslands;
        public List<Island> directConnections;
        public bool isAccessibleFromMainIsland;

        public int CompareTo(Island other) => other.islandCentre.CompareTo(islandCentre);
        public bool Equals(Island other) => other.islandCentre == islandCentre;


        public Island(NativeList<int> islandTiles)
        {
            this.islandTiles = islandTiles;
            connectedIslands = new List<Island>();
            directConnections = new List<Island>();
            islandCentre = islandTiles[islandTiles.Length / 2];

        }
        public static void connectIsland(Island islandA, Island islandB)
        {
            islandA.connectIsland(islandB);
            islandB.connectIsland(islandA);

            islandB.directConnections.Add(islandA);
            islandA.directConnections.Add(islandB);
        }

        public void connectIsland(Island otherIsland)
        {
            if (this.IsConnected(otherIsland) || this == otherIsland) return;
            this.connectedIslands.Add(otherIsland);
            otherIsland.connectedIslands.Add(this);

            foreach (Island island in this.connectedIslands)
            {
                island.connectIsland(otherIsland);
            }
        }

        private void SetAccessibleFromMainIsland()
        {
            if (!isAccessibleFromMainIsland)
            {
                isAccessibleFromMainIsland = true;
                foreach (Island connectedIsland in connectedIslands)
                {
                    connectedIsland.SetAccessibleFromMainIsland();
                }
            }
        }

        public bool IsConnected(Island otherIsland)
        {
            return connectedIslands.Contains(otherIsland);
        }

        public bool isDirectConnection(Island otherIsland)
        {
            return directConnections.Contains(otherIsland);
        }
    }

    struct IslandTile
    {
        public NativeList<int> tiles;
        public TileType tileType;

    }
    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }





    [BurstDiscard]
    static void mapAllIslands(in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer)
    {
        NativeList<int> mapFlags = new NativeList<int>(mapGenRequirement.height * mapGenRequirement.width, Allocator.Temp)
        {
            Length = mapGenRequirement.height * mapGenRequirement.width//initialize to all 0s
        };
        List<Island> all_islands = new List<Island>();
        for (int i = 0; i < mapFlags.Length; i++)
        {
            if (mapFlags[i] == 0 && tileMapBuffer[i] == (int)TileType.floor)
            {
                Island island = new Island(GetRegionTiles(ref mapFlags, in mapGenRequirement, ref tileMapBuffer, i % mapGenRequirement.width, i / mapGenRequirement.width));
                all_islands.Add(island);
            }
        }




        //getting rid of small islands
        //that is, turning small islands into 1s
        foreach (Island island in all_islands.Reverse<Island>())
        {

            if (island.islandTiles.Length < 75) // change this threshold
            {
                foreach (int tile_idx in island.islandTiles)
                {
                    tileMapBuffer[tile_idx] = (int)TileType.wall;
                }

                island.islandTiles.Dispose();
                all_islands.Remove(island);
            }
        }

        connectedClosestIslands(ref tileMapBuffer, mapGenRequirement.width, mapGenRequirement.height, all_islands, mapGenRequirement.seed);

    }

    static void connectedClosestIslands(ref DynamicBuffer<TileMapBufferElement> tileMapBuffer, int width, int height, List<Island> allIslands, NativeString64 seed)
    {
        Queue<Island> islandQueue = new Queue<Island>();
        islandQueue.Enqueue(allIslands[0]);
        int test_stop = 0;
        while (islandQueue.Count > 0)
        {
            Island curr_island = islandQueue.Dequeue();
            if (curr_island.connectedIslands.Count != allIslands.Count - 1)
            {
                float closest_island_distance = pow(width * height, 2);
                Island closestIsland = curr_island;


                //find the closest island that curr_island hasn't connected
                foreach (Island islandB in allIslands)
                {
                    if (islandB == curr_island || curr_island.IsConnected(islandB)) continue;
                    Coord tileA = new Coord(curr_island.islandCentre % width, curr_island.islandCentre / width);
                    Coord tileB = new Coord(islandB.islandCentre % width, islandB.islandCentre / width);
                    float distanceBetweenIslands = pow(tileA.tileX - tileB.tileX, 2) + pow(tileA.tileY - tileB.tileY, 2);
                    if (closest_island_distance > distanceBetweenIslands)
                    {
                        closest_island_distance = distanceBetweenIslands;
                        closestIsland = islandB;
                    }
                }

                /*                string debugString = testing_count + "  :  curr_island: " + curr_island.islandCentre + "  closest_island" + closestIsland.islandCentre;
                                foreach(Island island in curr_island.connectedIslands)
                                {
                                    debugString += "  " + island.islandCentre;
                                }
                                debugString += curr_island.IsConnected(closestIsland);

                                Debug.Log(debugString);*/

                CreatePassage(curr_island,
                    closestIsland,
                    new Coord(curr_island.islandCentre % width, curr_island.islandCentre / width),
                    new Coord(closestIsland.islandCentre % width, closestIsland.islandCentre / width),
                    ref tileMapBuffer,
                    width, height,
                    seed);
                islandQueue.Enqueue(closestIsland);
                test_stop++;
            }
        }

    }


    static Vector3 CoordToWorldPoint(Coord tile, int width, int height)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 0, -height / 2 + .5f + tile.tileY);
    }

    static void CreatePassage(Island IslandA, Island IslandB, Coord tileA, Coord tileB, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer, int width, int height, NativeString64 seed)
    {
        Island.connectIsland(IslandA, IslandB);

        /*        Debug.DrawLine(CoordToWorldPoint(tileA, width, height), CoordToWorldPoint(tileB, width, height), Color.green, 100);
        */


        /*        List<Coord> line = getRandomLine(tileA, tileB,seed);
        */
        List<Coord> line = getRandomLine(tileA, tileB,seed,3);

        foreach (Coord c in line)
        {
            DrawCircle(ref tileMapBuffer, width, height, c, 3);
        }
    }

    static void DrawCircle(ref DynamicBuffer<TileMapBufferElement> tileMapBuffer, int width, int height, Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y < r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (MapGenHelper.IsInMapRange(drawX, drawY, width-1, height-1))
                    {
                        tileMapBuffer[drawX + drawY * width] = (int)TileType.floor;
                    }
                }
            }
        }
    }

    public static List<Coord> getRandomLine(Coord from, Coord to, NativeString64 seed, int step = 1)
    {
        
        int x_diff = abs(to.tileX - from.tileX);
        int y_diff = abs(to.tileY - from.tileY);


        Random pseudoRandom = new Random((uint)seed.GetHashCode());
        Coord cur_coord = from;
        List<Coord> line = new List<Coord>();

        int x_sign = (int)sign(to.tileX - from.tileX);
        int y_sign = (int)sign(to.tileY - from.tileY);
        int x_min = min(from.tileX, to.tileX);
        int x_max = max(from.tileX, to.tileX);
        int y_min = min(from.tileY, to.tileY);
        int y_max = max(from.tileY, to.tileY);


        if (x_diff > y_diff)
        {
            while (cur_coord.tileX != to.tileX)
            {

                cur_coord.tileX += x_sign;
                int curr_dx = abs(to.tileX - cur_coord.tileX);
                int curr_dy = abs(to.tileY - cur_coord.tileY);
                if (curr_dy > curr_dx)
                {
                    //at this point , if you keep inceasing x, you would need to increase y as well.
                    if (sign(to.tileY - cur_coord.tileY) == y_sign)
                    {
                        cur_coord.tileY += y_sign;
                    }
                    else
                    {
                        cur_coord.tileY -= y_sign;
                    }
                    //to lossen the requirement, let x have a chance to decrease, 
                    //the percentage decreases as x get's bigger

                    if (pseudoRandom.NextInt(0, 100) < curr_dx * 100 / (curr_dx + curr_dy))
                    {
                        cur_coord.tileX -= x_sign;
                        continue;
                    }
                }
                else
                {

                    var randInt = pseudoRandom.NextInt(-step, step);
                    int temp_y = cur_coord.tileY + randInt;
                    cur_coord.tileY = (y_min <= temp_y && temp_y <= y_max) ? temp_y : cur_coord.tileY - randInt;
                }
                
                line.Add(cur_coord);
            }
        }
        else
        {
            while (cur_coord.tileY != to.tileY)
            {
                cur_coord.tileY += y_sign;
                int curr_dx = abs(to.tileX - cur_coord.tileX);
                int curr_dy = abs(to.tileY - cur_coord.tileY);
                if (curr_dx > curr_dy)
                {
                    if (sign(to.tileX - cur_coord.tileX) == x_sign)
                    {
                        cur_coord.tileX += x_sign;
                    }
                    else
                    {
                        cur_coord.tileX -= x_sign;
                    }

                    if (pseudoRandom.NextInt(0, 100) < curr_dy * 100 / (curr_dx + curr_dy))
                    {
                        cur_coord.tileY -= y_sign;
                        continue;
                    }

                }
                else{
                    var randInt = pseudoRandom.NextInt(-step, step);
                    int temp_x = cur_coord.tileX + randInt;
                    cur_coord.tileX = (x_min <= temp_x && temp_x <= x_max) ? temp_x : cur_coord.tileX - randInt;

                }
                line.Add(cur_coord);
            }
        }


        return line;
    }


    /*
    BFS flooding to try to map the regions by their types
*/
    static NativeList<int> GetRegionTiles(ref NativeList<int> mapFlags, in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer, int startX, int startY)
    {
        int width = mapGenRequirement.width;
        int height = mapGenRequirement.height;

        NativeList<int> tiles = new NativeList<int>(Allocator.Temp);

        int starting_idx = startX + startY * mapGenRequirement.width;
        int tileType = tileMapBuffer[starting_idx];



        NativeQueue<int> queue = new NativeQueue<int>(Allocator.Temp);
        queue.Enqueue(starting_idx);
        mapFlags[starting_idx] = 1;//flags to check if a tile has been visited or not

        while (queue.Count > 0)
        {
            int tile_idx = queue.Dequeue();
            tiles.Add(tile_idx);
            int tile_x = tile_idx % mapGenRequirement.width;
            int tile_y = tile_idx / mapGenRequirement.width;

            for (int y = tile_y - 1; y <= tile_y + 1; y++)
            {
                int y_idx = y * width;
                for (int x = tile_x - 1; x <= tile_x + 1; x++)
                {
                    if (MapGenHelper.IsInMapRange(x, y, width, height) && (y == tile_y || x == tile_x))
                    {
                        //only the neighboring 4 tiles are considered
                        if (mapFlags[x + y_idx] == 0 && tileMapBuffer[x + y_idx] == tileType)
                        {
                            mapFlags[x + y_idx] = 1;//is now visited
                            queue.Enqueue(x + y_idx);
                        }
                    }
                }
            }
        }

        queue.Dispose();
        return tiles;
    }
}