using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

namespace MapGen{
    public enum CellShape
    {
        balanced = 0,
        narrow = 1,
        random = 2

    }
    public class Bitmap {
        private int[] bmArray;

        private int[] rowLength;
        private int xSize;// maximum x dimension of this bitmap
        private int ySize;// maximum y dimension of this bitmap

        private int numSetBits;// number of set bits of this bitmap

        public Bitmap(int area, int xCons, int yCons){
            bmArray = new int[yCons + 1];
            rowLength = new int[yCons + 1];
            this.numSetBits = area;
            this.xSize = xCons;
            this.ySize = yCons;
            
            int balance = (int) Round(Sqrt(area));
            for(int i = 0; i < yCons; i++){
                bmArray[i] = (int) Pow(2,xCons);
                rowLength[i] = xCons;
            }
            // int currAreaCount = 0;
            // int curRow = 0;
            // while(currAreaCount < area){
            //     bmArray[curRow] = Random.Range((int)Pow(2,xCons),(int)Pow(2,balance+xCons));

            //     rowLength[curRow] = 0;

            //     int i = bmArray[curRow];
            //     while((i>>=1) != 0){
            //         rowLength[i]++;
            //     }
                
            //     currAreaCount+=countSetBits(i);
            //     curRow++;
            // }
        }

        public int getRowLength(int i){
            return rowLength[i];
        }

        public int[] getBm(){
            return bmArray;
        }

        public int getxSize(){
            return xSize;
        }

        public int getySize(){
            return ySize;
        }

        public int getnumSetBits(){
            return numSetBits;
        }

        static int countSetBits(int n) { 
            int count = 0;
            while (n != 0) { 
                n &= (n-1) ; 
                count++; 
            } 
            return count; 
        }


        static int[] getRandCellBitmap(int area,CellShape shape = 0){
            int [] bitmap;
            int balance = (int) Round(Sqrt(area));
            bitmap = new int[balance];
            int currAreaCount = 0;
            if(shape == CellShape.balanced){
                int curRow = 0;
                while(currAreaCount < area){
                    bitmap[curRow] = Random.Range(0,(int)Pow(2,balance));
                    currAreaCount+=countSetBits(bitmap[curRow]);
                    curRow++;
                }                
            }
            
            return bitmap;
        }
    }

}
