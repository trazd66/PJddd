using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;



/// <summary>
/// Adding this component to an entity will cause the entity to be destroyed.
/// </summary>
[Serializable]
public struct DeletionTag : IComponentData{}


public static class EntityDeleteHelper
{
    public static void deleteEntitySingle(Entity entityToDelete, EntityManager entityManager){
        entityManager.AddComponentData(entityToDelete, new DeletionTag());
    }
}
