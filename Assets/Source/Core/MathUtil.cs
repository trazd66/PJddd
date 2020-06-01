using UnityEngine;
using Unity.Mathematics;
using System.Collections;

namespace MathUtil { 
    public static class float3Helper
    {
        public static float3 left = new float3(-1, 0, 0);
        public static float3 right  = new float3(1, 0, 0);
        public static float3 up = new float3(0, 1, 0);
        public static float3 down = new float3(0, -1, 0);
        public static float3 forward = new float3(0, 0, 1);
        public static float3 back = new float3(0, 0, -1);

    }


}
