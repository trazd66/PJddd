using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

/// <summary>
/// Adding this component to an entity will spawn Count entity Prefabs, and then destroy the spawner entity.
/// </summary>
[Serializable]
public struct SpawnerComponent : IComponentData
{
    public int Count;
    public Entity PrefabEntity;

}

public struct MassSpawnerComponent : IComponentData
{
    public int Count;
    public Entity PrefabEntity;

}


public static class EntitySpawnHelper{

    public static void spawnPrefabSingle(Entity prefabToSpawn, EntityManager entityManager, Translation translation , Rotation rotation){
        SpawnerComponent MCspawner = new SpawnerComponent { Count = 1, PrefabEntity = prefabToSpawn };
        Entity spawner = entityManager.CreateEntity();
        entityManager.AddComponentData(spawner, MCspawner);
        entityManager.AddComponentData(spawner, translation);
        entityManager.AddComponentData(spawner, rotation);
    }
}
