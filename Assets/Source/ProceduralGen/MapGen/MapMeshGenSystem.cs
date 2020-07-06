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


        Entities.WithAny<TileMapGeneration.T_MeshGen>().ForEach(
            (Entity entity, int entityInQueryIndex, in MapGenRequirement mapGenReq) =>
            {
            DynamicBuffer<TileMapBufferElement> tileMapBuffers = lookup[entity];
                if (tileMapBuffers.Length > 0)
                {
                    MapMeshGenerator mapMeshGenerator = GetMapMeshGenerator(in mapGenReq, in tileMapBuffers, TileType.wall);

                    RenderMesh tileMapRenderMesh = mapMeshGenerator.generateRenderMesh(
                        Resources.Load("testMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material,
                        mapMeshGenerator.createTileMesh());
                    //TODO: Make it read the materail from a material registry instead
                    
                    var meshCoillider = MapGenPhysics.createMeshCollider(mapMeshGenerator.vertices.ToArray(), mapMeshGenerator.triangles.ToArray());

                    var tileMeshEntity = ecb.CreateEntity();
                    ecb.AddComponent(tileMeshEntity, new PhysicsCollider { Value = meshCoillider });
/*                    ecb.AddComponent(tileMeshEntity, new Parent {Value=entity});
*/
                    ecb.AddComponent(tileMeshEntity, new LocalToWorld { });
                    ecb.AddSharedComponent(tileMeshEntity, tileMapRenderMesh);
                    ecb.AddComponent(tileMeshEntity, new RenderBounds
                    {
                        Value =
                        new AABB
                        {
                            Center = 0,
                            Extents = 10000000//for testing purpose
                        }
                    });
                    ecb.AddComponent(tileMeshEntity, new Translation { });



                    RenderMesh wallRenderMesh = mapMeshGenerator.generateRenderMesh(
    Resources.Load("testMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material,
    mapMeshGenerator.CreateWallMesh(5));
                    //TODO: Make it read the materail from a material registry instead

                    var wallMeshCoillider = MapGenPhysics.createMeshCollider(mapMeshGenerator.wallVertices.ToArray(), mapMeshGenerator.wallTriangles.ToArray());

                    var wallMeshEntity = ecb.CreateEntity();
                    ecb.AddComponent(wallMeshEntity, new PhysicsCollider { Value = wallMeshCoillider });
                    /*                    ecb.AddComponent(tileMeshEntity, new Parent {Value=entity});
                    */
                    ecb.AddComponent(wallMeshEntity, new LocalToWorld { });
                    ecb.AddSharedComponent(wallMeshEntity, wallRenderMesh);
                    ecb.AddComponent(wallMeshEntity, new RenderBounds
                    {
                        Value =
                        new AABB
                        {
                            Center = 0,
                            Extents = 10000000//for testing purpose
                        }
                    });
                    ecb.AddComponent(wallMeshEntity, new Translation { });
                    //-----------------------------------------------------------------

/*                    MapMeshGenerator floorMeshGenerator = GetMapMeshGenerator(in mapGenReq, in tileMapBuffers, TileType.floor);
                    RenderMesh floorRenderMesh = floorMeshGenerator.generateRenderMesh(
    Resources.Load("testMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material,
    floorMeshGenerator.createTileMesh());
                    //TODO: Make it read the materail from a material registry instead

                    var floorMeshCoillider = MapGenPhysics.createMeshCollider(floorMeshGenerator.vertices.ToArray(), floorMeshGenerator.triangles.ToArray());

                    var floorMeshEntity = ecb.CreateEntity();
                    ecb.AddComponent(floorMeshEntity, new PhysicsCollider { Value = floorMeshCoillider });
                    *//*                    ecb.AddComponent(tileMeshEntity, new Parent {Value=entity});
                    *//*
                    ecb.AddComponent(floorMeshEntity, new LocalToWorld { });
                    ecb.AddSharedComponent(floorMeshEntity, floorRenderMesh);
                    ecb.AddComponent(floorMeshEntity, new RenderBounds
                    {
                        Value =
                        new AABB
                        {
                            Center = 0,
                            Extents = 10000000//for testing purpose
                        }
                    });
                    ecb.AddComponent(floorMeshEntity, new Translation { });

*/
                    ecb.RemoveComponent<TileMapGeneration.T_MeshGen>(entity);//mesh Generation finished
                    ecb.AddComponent<TileMapGeneration.T_SpawnFlagGen>(entity);//Request starting spawn flag generation
                }
            }
        ).WithoutBurst().Run();//TODO : make it parrallel one day

        ecbSystem.AddJobHandleForProducer(this.Dependency);

    }


    static MapMeshGenerator GetMapMeshGenerator(in MapGenRequirement mapGenReq, in DynamicBuffer<TileMapBufferElement> tileMapBuffer, TileType tType)
    {
        return new MapMeshGenerator(tileMapBuffer.Reinterpret<int>(), mapGenReq.width, mapGenReq.height, mapGenReq.squareSize, tType);
    }
}