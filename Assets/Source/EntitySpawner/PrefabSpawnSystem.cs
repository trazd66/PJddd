using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class PrefabSpawnSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct PrefabSpawnSystemJob : IJobForEachWithEntity<SpawnerComponent, Translation, Rotation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref SpawnerComponent spawner,
            [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation)
        {
            for (int i = 0; i < spawner.Count; i++)
            {
                var instance = CommandBuffer.Instantiate(index, spawner.PrefabEntity);
                // Place the instantiated in a grid with some noise
                
                CommandBuffer.SetComponent(index, instance, translation);
                CommandBuffer.SetComponent(index, instance, rotation);
            }

            CommandBuffer.DestroyEntity(index, entity);
        }

    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new PrefabSpawnSystemJob
        {
            CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()

        }.Schedule(this, inputDependencies);

        entityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}