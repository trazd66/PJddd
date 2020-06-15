using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using MapGen;
using Unity.Rendering;
using Unity.Physics;
using UnityEngine;
using Unity.Sample.Core;

[DisableAutoCreation]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(MapGenSystem))]
public class MapMeshGenSystem : SystemBase
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
        var ecb = ecbSystem.CreateCommandBuffer();
        BufferFromEntity<TileMapBufferElement> lookup = GetBufferFromEntity<TileMapBufferElement>();


        Entities.WithNone<RenderMesh>().ForEach(
            (Entity entity, int entityInQueryIndex, in MapGenRequirement mapGenReq) =>
            {
            DynamicBuffer<TileMapBufferElement> tileMapBuffers = lookup[entity];
            if (tileMapBuffers.Length > 0)
            {
                RenderMesh tileMapRenderMesh = generateRenderMesh(in mapGenReq, in tileMapBuffers);
                ecb.AddComponent(entity, new LocalToWorld { });
                ecb.AddSharedComponent(entity, tileMapRenderMesh);
                    ecb.AddComponent(entity, new RenderBounds
                    {
                        Value =
                        new AABB
                        {
                            Center = 0,
                            Extents = 0
                        }
                    });
                    ecb.AddComponent(entity, new Translation { });
                }

            }
        ).WithoutBurst().Run();

        ecbSystem.AddJobHandleForProducer(this.Dependency);

    }

    static RenderMesh generateRenderMesh(in MapGenRequirement mapGenReq, in DynamicBuffer<TileMapBufferElement> tileMapBuffer)
    {
        MapMeshGenerator mapMeshGenerator = new MapMeshGenerator(tileMapBuffer.Reinterpret<int>(), mapGenReq.width, mapGenReq.height, mapGenReq.squareSize);
        return mapMeshGenerator.generateRenderMesh(Resources.Load("testMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material);
    }
}