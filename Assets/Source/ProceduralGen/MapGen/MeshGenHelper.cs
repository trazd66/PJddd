using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace MapGen{
    public static class MeshGenHelper
    {
        /*
        helper method for setting up a quad,
        https://catlikecoding.com/unity/tutorials/rounded-cube/
        */    
        private static int SetQuad (int[] triangles, int i, int v00, int v10, int v01, int v11) {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

        public static Vector3[] genGridVertex(int xSize, int ySize){
            Vector3[] vertices = new Vector3[(xSize+1)*(ySize+1)];
            for (int i = 0, y = 0; y <= ySize; y++) {
                for (int x = 0; x <= xSize; x++, i++) {
                    vertices[i] = new Vector3(x, y);
                }
            }
            return vertices;
        }

       public static int[] genTrigFromBitmap(Bitmap bm){
            int [] triangles = new int[bm.getnumSetBits() * 6];
            // int curTrigDrawn = 0;
            for (int ti = 0, vi = 0, y = 0; y < bm.getySize(); y++, vi++) {
                for (int x = 0; x < bm.getxSize(); x++, ti += 6, vi++) {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + bm.getxSize() + 1;
                    triangles[ti + 5] = vi + bm.getxSize() + 2;
                }
            }
            return triangles;
        }



    }

}
