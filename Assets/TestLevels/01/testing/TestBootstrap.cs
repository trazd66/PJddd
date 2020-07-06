using UnityEngine;
using Unity.Entities;
using MapGen;
using Unity.Sample.Core;
using UnityEngine.InputSystem;
public class TestBootstrap : MonoBehaviour
{

    MapRepo mapRepo;

    public WeakAssetReference testAsset0;
    public WeakAssetReference testAsset1;


    public void Initialize()
    {
        mapRepo = new MapRepo(WorldManager.getOrCreateWorld("DefaultWorld"),WorldManager.getOrCreateWorld("MapGenWorld"));
        ConfigVar.Init();
        StaticInputController.init();
        World mapGenWorld = WorldManager.getOrCreateWorld("MapGenWorld");
    }

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.X].wasReleasedThisFrame)
        {
            mapRepo.testMap = mapRepo.generateRandomMap(mapTheme.test, mapType.Cave, 500, 500);
        }

        if (Keyboard.current[Key.M].wasReleasedThisFrame)
        {
            mapRepo.moveMapToRenderWorld(ref mapRepo.testMap);
        }
/*        World mapGenWorld = WorldManager.getOrCreateWorld("MapGenWorld");
        var testMap = mapRepo.generateRandomMap(mapType.Cave, 200, 100);

        var entityArr = new NativeArray<Entity>(1,Allocator.Temp);
        entityArr[0] = testMap;
        World.DefaultGameObjectInjectionWorld.EntityManager.CopyEntitiesFrom(mapGenWorld.EntityManager, entityArr);
*/


/*        if (Input.GetKeyDown(KeyCode.A))
                {
                    mapGenWorld.EntityManager.DestroyEntity(mapGenWorld.EntityManager.GetAllEntities());
                    mapRepo.generateRandomMap(mapType.Cave, 200, 100);
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    World.DefaultGameObjectInjectionWorld.EntityManager.CopyAndReplaceEntitiesFrom(mapGenWorld.EntityManager);
                }
        */
    }
}
