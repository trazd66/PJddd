using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class EntityDeletionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<DeletionTag>().ForEach( (Entity e) =>
        {
            ECSWorldManager.CurrentWorld.EntityManager.DestroyEntity(e);
        });
    }
}
