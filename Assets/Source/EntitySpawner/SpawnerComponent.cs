﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Adding this component to an entity will spawn Count entity Prefabs, and then destroy the spawner entity.
/// </summary>
[Serializable]
public struct SpawnerComponent : IComponentData
{
    public int Count;
    public Entity PrefabEntity;

}