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
            var tileMapBuffer =  ecb.AddBuffer<TileMapBufferElement>(entityInQueryIndex, entity);
            
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


        /*        if (MapGenRequirement.seed.ToString() == null)
                {
                    MapGenRequirement.seed = Time.DeltaTime.ToString();
                }
        */

        //TODO: Better Random management
        Random pseudoRandom = new Random((uint)mapGenRequirement.seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    tileMapBuffer.Add((int)tileType.border);
                }
                else
                {
                    tileMapBuffer.Add(((int)pseudoRandom.NextUInt(0, 100) < mapGenRequirement.randomFillPercent) ? (int)tileType.floor : (int)tileType.border);
                }
            }
        }
    }

    static void SmoothMap(in MapGenRequirement mapGenRequirement, ref DynamicBuffer<TileMapBufferElement> tileMapBuffer)
    {
        int width = mapGenRequirement.width;
        int height = mapGenRequirement.height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + width * y;
                int neighbourWallTiles = MapGenHelper.GetSurroundingWallCount(x, y, width, height, tileMapBuffer);

                //below are parts that can be modified to get different desired rooms
                if (neighbourWallTiles > 4)
                    tileMapBuffer[index] = (int)tileType.border;
                else if (neighbourWallTiles <= 4)
                    tileMapBuffer[index] = (int)tileType.floor;

            }
        }
    }

    static void generateRenderMesh(in DynamicBuffer<TileMapBufferElement> tileMapBuffer)
    {
        MapMeshGenerator mapMeshGenerator = new MapMeshGenerator();

    }
}