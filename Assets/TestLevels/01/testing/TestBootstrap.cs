using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using  MapGen;
using Unity.Sample.Core;
using Unity.Rendering;

public class TestBootstrap : MonoBehaviour
{
/*     int[,] map;
     int width;
     int height;*/
    // void OnDrawGizmos() {

    //     map = Map.generateRandomMap(100,100,40).tileMap;
    //     width = 100;
    //     height = 100;
	// 	if (map != null) {
	// 		for (int x = 0; x < width; x ++) {
	// 			for (int y = 0; y < height; y ++) {
	// 				Gizmos.color = (map[x,y] == 1)?Color.black:Color.white;
	// 				Vector3 pos = new Vector3(-width/2 + x + .5f,0, -height/2 + y+.5f);
	// 				Gizmos.DrawCube(pos,Vector3.one);
	// 			}
	// 		}
	// 	}
	// }

    public void Initialize()
    {
        GameDebug.Log("asd");
        MapGenWorldBoostrap.initMapGenWorld();
/*        TestingModuleSetting tSetting = Resources.Load<TestingModuleSetting>("t0");
        
        Entity e0 = PrefabAssetManager.CreateEntity(World.DefaultGameObjectInjectionWorld.EntityManager, tSetting.test0Prefab);
        Entity e1 = PrefabAssetManager.CreateEntity(World.DefaultGameObjectInjectionWorld.EntityManager, tSetting.test1Prefab);

        int[,] tmap = Map.generateRandomMap(50,50,30).tileMap;

        MapMeshGenerator mGen = new MapMeshGenerator();
        var pfab = PrefabAssetRegistry.FindEntityPrefab(World.DefaultGameObjectInjectionWorld.EntityManager, tSetting.test0Prefab);
        RenderMesh rm_testing = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentObject<RenderMesh>(pfab);
        RenderMesh rm = mGen.generateRenderMesh(tmap, rm_testing.material, 1);
        Entity map = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddSharedComponentData(map, rm);
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(map, new Translation());
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(map, new Rotation());
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(map, new LocalToWorld());*/

    }

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
