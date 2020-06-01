﻿using UnityEngine;
using UnityEditor;
using Unity.Entities;
namespace MapGen
{
    public static class MapGenHelper
    {
        /*
        helper method for detecing how many surrounding tiles are also walls
                * * *
                * * *            use this diagram for intuition
                * * *
        */
        public static int GetSurroundingWallCount(int gridX, int gridY, int width, int height, DynamicBuffer<TileMapBufferElement> tileMap)
        {
            int wallCount = 0;
            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                {
                    if (IsInMapRange(neighbourX, neighbourY, width, height))
                    {
                        if (neighbourX != gridX || neighbourY != gridY)
                        {
                            wallCount += tileMap[neighbourX + neighbourY * width];
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
            return wallCount;
        }


        public static bool IsInMapRange(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

    }
}