using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using static Unity.Mathematics.Random;

using MapGen;
using Unity.Sample.Core;
using System.Linq;
using System;
using Random = Unity.Mathematics.Random;
using Unity.Entities.UniversalDelegates;
using System.Collections.Generic;

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
        Entities.WithAny<T_TileMapGen>().ForEach(
            (Entity entity, int entityInQueryIndex, ref MapGenRequirement mapGenRequirement) =>
        {
            var tileMapBuffer = ecb.AddBuffer<TileMapBufferElement>(entityInQueryIndex, entity);

            createTileMap(in mapGenRequirement, ref tileMapBuffer);
            SmoothMap(in mapGenRequirement, ref tileMapBuffer);


            ecb.RemoveComponent<T_TileMapGen>(entityInQueryIndex, entity);//TileMapGeneartion finished
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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

    static void SmoothMap(in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer)
    {
        int width = mapGenRequirement.width;
        int height = mapGenRequirement.height;
        for (int y = 0; y < height; y++)
        {
            int index_y = width * y;
            for (int x = 0; x < width; x++)
            {
                int index = x + index_y;
                int neighbourWallTiles_r1 = MapGenHelper.GetSurroundingWallCount(x, y, width, height, tileMapBuffer);
                int neighbourWallTiles_r2 = MapGenHelper.GetSurroundingWallCount(x, y, width, height, tileMapBuffer, 2);


                if (neighbourWallTiles_r1 < 4){
                    tileMapBuffer[index] = (int)TileType.floor;
                }else if (neighbourWallTiles_r1 >= 5){
                    tileMapBuffer[index] = (int)TileType.wall;
                }

            }
        }


/*        mapAllIslands(in mapGenRequirement, ref tileMapBuffer);
*/    }

    struct Island
    {
        public int islandCentre;
        public IslandTile islandTile;
        public List<Island> adjIslands;
        public List<Island> connectedIslands;

    }

    struct IslandTile
    {
        public NativeList<int> tiles;
        public TileType tileType;

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
            if (mapFlags[i] == 0)
            {
                Island island = new Island
                {
                    islandTile = GetRegionTiles(ref mapFlags, in mapGenRequirement, ref tileMapBuffer, i % mapGenRequirement.width, i / mapGenRequirement.width),
                };
                island.islandCentre = island.islandTile.tiles[island.islandTile.tiles.Length / 2];
                all_islands.Add(island);
            }
        }

        Island wall = new Island();

        var newwallTiles = new List<int>();

        //getting rid of small islands
        //that is, turning small islands into 0s
        foreach (Island island in all_islands.Reverse<Island>())
        {

            if (island.islandTile.tileType == TileType.wall)
            {
                wall = island;
            }
            else if (island.islandTile.tiles.Length < 20) // change this threshold
            {
                foreach (int tile_idx in island.islandTile.tiles)
                {
                    tileMapBuffer[tile_idx] = (int)TileType.wall;
                    newwallTiles.Add(tile_idx);
                }

                island.islandTile.tiles.Dispose();
                all_islands.Remove(island);
            }
        }

        //add the new 0s to the wallIsland
        foreach (int tile_idx in newwallTiles)
        {
            wall.islandTile.tiles.Add(tile_idx);
        }





    }

    /*
    BFS flooding to try to map the regions by their types
*/
    static IslandTile GetRegionTiles(ref NativeList<int> mapFlags, in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer, int startX, int startY)
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

       var islandTile = new IslandTile
        {
            tiles = tiles,
            tileType = (TileType) tileType,
        };


        queue.Dispose();
        return islandTile;
    }
}