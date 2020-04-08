using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using  MapGen;
public class TestBootstrap : MonoBehaviour
{
    // int[,] map;
    // int width;
    // int height;
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
        // Entity convertedMC = StaticResourceManager.S.getPrefabEntity("mc");
        // SpawnerComponent MCspawner = new SpawnerComponent { Count = 1, PrefabEntity = convertedMC };
        // Entity spawner = World.Active.EntityManager.CreateEntity();
        // World.Active.EntityManager.AddComponentData(spawner, MCspawner);
        // World.Active.EntityManager.AddComponentData(spawner, new Translation());
        // World.Active.EntityManager.AddComponentData(spawner, new Rotation());

        int[,] tmap = Map.generateRandomMap(50,50,30).tileMap;

        MapMeshGenerator mGen = new MapMeshGenerator();
        Entity map = World.Active.EntityManager.CreateEntity();
        World.Active.EntityManager.AddSharedComponentData (map, mGen.generateRenderMesh(tmap, StaticResourceManager.S.testMaterial, 1));
        World.Active.EntityManager.AddComponentData(map, new Translation());
        World.Active.EntityManager.AddComponentData(map, new Rotation());
        World.Active.EntityManager.AddComponentData(map, new LocalToWorld());

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
