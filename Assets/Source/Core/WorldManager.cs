using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public static class WorldManager
{
    private static Dictionary<string, World> worldDict = new Dictionary<string, World>();


    public static World getOrCreateWorld(string worldName)
    {
        if (!worldDict.ContainsKey(worldName))
        {
            worldDict.Add(worldName, new World(worldName));
        }
            
        return worldDict[worldName];
    }
    
    public static void destroyWorld(string worldName)
    {
        worldDict[worldName].Dispose();
        worldDict.Remove(worldName);
    }
}
