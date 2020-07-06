using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

struct SpawnFlag : IComponentData
{
    WeakAssetReference asset;
    Parent parentMap;
    LocalToParent localToParent;
}