﻿#define UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP

#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Scripting;
 
namespace DOTS.Bootstrap
{
    public class WorldBootStrap : ICustomBootstrap
    {
 
        static MethodInfo insertManagerIntoSubsystemListMethod = typeof(ScriptBehaviourUpdateOrder).GetMethod("InsertManagerIntoSubsystemList", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
       
 
        public bool Initialize(string defaultWorldName)
        {
            // setting up default world
            defaultWorldInit();

            mapGenWorldInit();


            return true;
        }

        public static void defaultWorldInit()
        {
            var default_world = WorldManager.getOrCreateWorld("DefaultWorld");
            World.DefaultGameObjectInjectionWorld = default_world;

            var default_systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default) as List<Type>;
            
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(default_world, default_systems);
            var charConSystem = default_world.AddSystem(new CharacterControlSystem());
            var simSysGroup = default_world.GetOrCreateSystem<SimulationSystemGroup>();
            simSysGroup.AddSystemToUpdateList(charConSystem);
            simSysGroup.SortSystems();

            UpdatePlayerLoop(default_world);

        }

        public static void mapGenWorldInit()
        {
            //Create world
            World mapGenWorld = WorldManager.getOrCreateWorld("MapGenWorld");
            //Create system groups
            var initSysGroup = mapGenWorld.GetOrCreateSystem<InitializationSystemGroup>();

            var ecbSystem = mapGenWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            var mapGenSys = mapGenWorld.CreateSystem<MapGenSystem>();
            var mapMeshGenSys = mapGenWorld.CreateSystem<MapMeshGenSystem>();
            initSysGroup.AddSystemToUpdateList(mapGenSys);
            initSysGroup.AddSystemToUpdateList(ecbSystem);
            initSysGroup.AddSystemToUpdateList(mapMeshGenSys);

            initSysGroup.SortSystems();

            UpdatePlayerLoop(mapGenWorld);
        }
        public static void UpdatePlayerLoop(World world)
        {
            var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
            if (PlayerLoop.GetCurrentPlayerLoop().subSystemList != null)
                playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            if (world != null)
            {
                for (var i = 0; i < playerLoop.subSystemList.Length; i++)
                {
                    int subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;
 
                    if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
                    {
                        Debug.Log("FixedUpdate");
/*                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; j++)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        var mgr = world.GetOrCreateSystem<SimulationSystemGroup>();
                        var genericMethod = insertManagerIntoSubsystemListMethod.MakeGenericMethod(mgr.GetType());
                        genericMethod.Invoke(null, new object[] { newSubsystemList, subsystemListLength + 0, mgr });
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;*/
                    }

                    else if (playerLoop.subSystemList[i].type == typeof(Update))
                    {
                        Debug.Log("Update");
                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; j++)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        var SimulationSystemGroup = world.GetOrCreateSystem<SimulationSystemGroup>();
                        
                        var transformSystemGroup = world.GetOrCreateSystem<TransformSystemGroup>();
                        SimulationSystemGroup.AddSystemToUpdateList(transformSystemGroup);

                        var companionGameObjectUpdateTransformSystem = world.GetOrCreateSystem<CompanionGameObjectUpdateTransformSystem>();
                        SimulationSystemGroup.AddSystemToUpdateList(companionGameObjectUpdateTransformSystem);

                        var genericMethod = insertManagerIntoSubsystemListMethod.MakeGenericMethod(SimulationSystemGroup.GetType() );
                        genericMethod.Invoke(null, new object[] { newSubsystemList, subsystemListLength + 0, SimulationSystemGroup });
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;
                    }
                    else if (playerLoop.subSystemList[i].type == typeof(PreLateUpdate))
                    {
                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; j++)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        var mgr = world.GetOrCreateSystem<PresentationSystemGroup>();
                        var genericMethod = insertManagerIntoSubsystemListMethod.MakeGenericMethod(mgr.GetType());
                        genericMethod.Invoke(null, new object[] { newSubsystemList, subsystemListLength + 0, mgr });
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;
                    }
                    else if (playerLoop.subSystemList[i].type == typeof(Initialization))
                    {
                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; j++)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        var mgr = world.GetOrCreateSystem<InitializationSystemGroup>();
                        var genericMethod = insertManagerIntoSubsystemListMethod.MakeGenericMethod(mgr.GetType());
                        genericMethod.Invoke(null, new object[] { newSubsystemList, subsystemListLength + 0, mgr });
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;
                    }
                }
            }
            else
            {
                Debug.Log("world not initialized");
            }
            PlayerLoop.SetPlayerLoop(playerLoop);
        }



    }
   
 
}
#endif