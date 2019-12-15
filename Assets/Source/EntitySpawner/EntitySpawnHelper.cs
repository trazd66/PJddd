using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public static class EntitySpawnHelper{

    public static void spawnPrefabSingle(Entity prefabToSpawn, EntityManager entityManager, Translation translation , Rotation rotation){
        SpawnerComponent MCspawner = new SpawnerComponent { Count = 1, PrefabEntity = prefabToSpawn };
        Entity spawner = entityManager.CreateEntity();
        entityManager.AddComponentData(spawner, MCspawner);
        entityManager.AddComponentData(spawner, translation);
        entityManager.AddComponentData(spawner, rotation);
    }


}
