using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
public class ProxyEntityCollection
{
    private Dictionary<string, Entity> entityNamePairs = new Dictionary<string, Entity>();

    public void Add(string name, Entity e)
    {
        entityNamePairs.Add(name, e);
    }
    public Entity Get(string name)
    {
        return entityNamePairs[name];
    }
}