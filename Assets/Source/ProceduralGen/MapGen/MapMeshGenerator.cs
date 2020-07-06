using UnityEngine;
using System.Collections.Generic;
using Unity.Rendering;
using Unity.Entities;
using Unity.Collections;

namespace MapGen
{
    /*
    Code adpated from https://github.com/SebLague/Procedural-Cave-Generation
    */
    public class MapMeshGenerator{

        public SquareGrid squareGrid;

        public List<Vector3> vertices = new List<Vector3>();
        public List<int> triangles = new List<int>();


        public List<Vector3> wallVertices;
        public List<int> wallTriangles;



        Dictionary<int,List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
        List<List<int>> outlines = new List<List<int>> ();
        HashSet<int> checkedVertices = new HashSet<int>();

        int width;
        int height;


        public MapMeshGenerator(DynamicBuffer<int> tilemap, int width, int height, float sqaureSize,TileType meshType)
        {
           this.width = width;
           this.height = height;
           squareGrid = new SquareGrid(tilemap.AsNativeArray(), width, height, sqaureSize, meshType);

            for (int x = 0; x < squareGrid.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.GetLength(1); y++)
                {
                    Square top = (y != squareGrid.GetLength(1) - 1)? squareGrid.GetSquare(x, y + 1) : null;
                    Square left = (x != 0) ? squareGrid.GetSquare(x - 1, y) : null;
                    Square bot = (y != 0) ? squareGrid.GetSquare(x, y - 1) : null;
                    Square right = (x != squareGrid.GetLength(0) - 1) ? squareGrid.GetSquare(x + 1, y) : null;

                    TriangulateSquare(squareGrid.GetSquare(x, y), top, left, bot, right);
                }
            }

        }

        public RenderMesh generateRenderMesh(Material material,Mesh mesh)
        {

            RenderMesh rend = new RenderMesh();
            rend.mesh = mesh;
            rend.material = material;
            return rend;
        }

        public Mesh createTileMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            /*            mesh.uv = generateUVs();
*/          mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();
            /*            rend.receiveShadows = true;
            */
            return mesh;
        }

        public Vector2[] generateUVs()
        {
            var uvs = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count /3; i++)
            {
                uvs[i*3] = new Vector2(0, 0);
                uvs[i * 3 + 1] = new Vector2(0, 1);
                uvs[i * 3 + 2] = new Vector2(1, 0);
            }
            return uvs;
        }

        void TriangulateSquare(Square square) {
            switch (square.configuration) {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
            }

        }


        void TriangulateSquare(Square square, Square top, Square left, Square bot, Square right)
        {
            List<Vector3> spline = !((square.configuration & ~square.configuration) == 0) ? null : InterpolateSpine(square, top, left, bot, right);
            switch (square.configuration)
            {
                case 0:
                    break;

                // 1 points:

                case 0b0001:
                    if (spline != null)
                        MeshFromSpline(square.bottomLeft, square.centreLeft, square.centreBottom, spline);
                    else
                        MeshFromPoints(square.bottomLeft, square.centreLeft, square.centreBottom);

                    break;

                case 0b0010:
                    if (spline != null)
                    {
                        spline.Reverse();
                        MeshFromSpline(square.bottomRight, square.centreBottom, square.centreRight, spline);
                    }

                    else
                        MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                    break;

                case 0b0100:
                    if (spline != null)
                    {
                        spline.Reverse();
                        MeshFromSpline(square.topRight, square.centreRight, square.centreTop, spline);
                    }
                    else
                        MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                    break;
                case 0b1000:
                    if (spline != null)
                        MeshFromSpline(square.topLeft, square.centreTop, square.centreLeft, spline);
                    else
                        MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                    break;



                // 2 points:
                case 0b0011:
                    MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                    break;
                case 0b0110:
                    MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                    break;
                case 0b1001:
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                    break;
                case 0b1100:
                    MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                    break;
                case 0b0101:
                    MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                    break;
                case 0b1010:
                    MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                    break;


                // 3 point:
                case 0b0111:
                    MeshFromPoints(square.bottomRight, square.centreTop, square.topRight);
                    if (spline != null)
                    {
                        spline.Reverse();
                        MeshFromSpline(square.bottomRight, square.centreLeft, square.centreTop, spline);
                    }
                    else
                        MeshFromPoints(square.bottomRight, square.centreLeft, square.centreTop);

                    MeshFromPoints(square.bottomRight, square.bottomLeft, square.centreLeft);

                    break;

                case 0b1011:
                    MeshFromPoints(square.bottomLeft, square.topLeft, square.centreTop);
                    if (spline != null)
                    {
                        MeshFromSpline(square.bottomLeft, square.centreTop, square.centreRight, spline);
                    }
                    else
                        MeshFromPoints(square.bottomLeft, square.centreTop, square.centreRight);

                    MeshFromPoints(square.bottomLeft, square.centreRight, square.bottomRight);
                    break;

                case 0b1101:
                    MeshFromPoints(square.topLeft, square.topRight, square.centreRight);
                    if (spline != null)
                        MeshFromSpline(square.topLeft, square.centreRight, square.centreBottom, spline);
                    else
                        MeshFromPoints(square.topLeft, square.centreRight, square.centreBottom);

                    MeshFromPoints(square.topLeft, square.centreBottom, square.bottomLeft);
                    break;

                case 0b1110:
                    MeshFromPoints(square.topRight, square.bottomRight, square.centreBottom);
                    if (spline != null)
                    {
                        spline.Reverse();
                        MeshFromSpline(square.topRight, square.centreBottom, square.centreLeft, spline);
                    }
                    else
                        MeshFromPoints(square.topRight, square.centreBottom, square.centreLeft);

                    MeshFromPoints(square.topRight, square.centreLeft, square.topLeft);
                    break;





                // 4 point:

                case 0b1111:
                    MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                    checkedVertices.Add(square.topLeft.vertexIndex);
                    checkedVertices.Add(square.topRight.vertexIndex);
                    checkedVertices.Add(square.bottomRight.vertexIndex);
                    checkedVertices.Add(square.bottomLeft.vertexIndex);
                    break;

            }

        }

        /***
         * 1   2
         * 3   4
         ***/
        List<Vector3> InterpolateSpine(Square square, Square top, Square left, Square bot, Square right)
        {
            Node p0,p1,p2,p3;
            p0 = square.centreBottom;
            p1 = square.centreBottom;
            p2 = square.centreBottom;
            p3 = square.centreBottom;
            //Config
            bool neighborStraight0 = false;
            bool neighborStraight1 = false;
            switch (square.configuration)
            {
                case 0:
                    break;


                //1 point
                case 0b0001:
                    switch (left.configuration)
                    {
                        case 0b0011:
                            p0 = left.centreLeft;
                            p1 = left.centreRight;
                            break;

                        case 0b0010:
                            p0 = left.centreBottom;
                            p1 = left.centreRight;
                            break;

                        case 0b1011:
                            p0 = left.centreTop;
                            p1 = left.centreRight;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (bot.configuration)
                    {
                        case 0b1000:
                            p2 = bot.centreTop;
                            p3 = bot.centreLeft;
                            break;
                        case 0b1001:
                            p2 = bot.centreTop;
                            p3 = bot.centreBottom;
                            break;
                        case 0b1011:
                            p2 = bot.centreTop;
                            p3 = bot.centreRight;
                            neighborStraight1 = true;
                            break;

                    }
                    break;

                case 0b0010:
                    switch (right.configuration)
                    {
                        case 0b0011:
                            p0 = right.centreRight;
                            p1 = right.centreLeft;
                            break;
                        case 0b0001:
                            p0 = right.centreBottom;
                            p1 = right.centreLeft;
                            break;
                        case 0b0111:
                            p0 = right.centreTop;
                            p1 = right.centreLeft;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (bot.configuration)
                    {
                        case 0b0110:
                            p2 = bot.centreTop;
                            p3 = bot.centreBottom;
                            break;
                        case 0b0100:
                            p2 = bot.centreTop;
                            p3 = bot.centreRight;
                            break;
                        case 0b0111:
                            p2 = bot.centreTop;
                            p3 = bot.centreLeft;
                            neighborStraight1 = true;
                            break;
                    }
                    break;

                case 0b0100:
                    switch (top.configuration)
                    {
                        case 0b0110:
                            p0 = top.centreTop;
                            p1 = top.centreBottom;
                            break;
                        case 0b0010:
                            p0 = top.centreRight;
                            p1 = top.centreBottom;
                            break;
                        case 0b1110:
                            p0 = top.centreLeft;
                            p1 = top.centreBottom;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (right.configuration)
                    {
                        case 0b1100:
                            p2 = right.centreLeft;
                            p3 = right.centreRight;
                            break;
                        case 0b1000:
                            p2 = right.centreLeft;
                            p3 = right.centreTop;
                            break;
                        case 0b1110:
                            p2 = right.centreLeft;
                            p3 = right.centreBottom;
                            neighborStraight1 = true;
                            break;
                    }
                    break;

                case 0b1000:
                    switch (top.configuration)
                    {
                        case 0b1001:
                            p0 = top.centreTop;
                            p1 = top.centreBottom;
                            break;
                        case 0b0001:
                            p0 = top.centreLeft;
                            p1 = top.centreBottom;
                            break;
                        case 0b1101:
                            p0 = top.centreRight;
                            p1 = top.centreBottom;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (left.configuration)
                    {
                        case 0b1100:
                            p2 = left.centreRight;
                            p3 = left.centreLeft;
                            break;
                        case 0b0100:
                            p2 = left.centreRight;
                            p3 = left.centreTop;

                            break;
                        case 0b1101:
                            p2 = left.centreRight;
                            p3 = left.centreBottom;
                            neighborStraight1 = true;
                            break;
                    }
                    break;


                //3 points
                case 0b0111:
                    switch (top.configuration)
                    {
                        case 0b1110:
                            p0 = top.centreLeft;
                            p1 = top.centreBottom;
                            break;
                        case 0b0110:
                            p0 = top.centreTop;
                            p1 = top.centreBottom;
                            break;
                        case 0b0010:
                            p0 = top.centreRight;
                            p1 = top.centreBottom;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (left.configuration)
                    {
                        case 0b1011:
                            p2 = left.centreRight;
                            p3 = left.centreTop;
                            break;
                        case 0b0011:
                            p2 = left.centreRight;
                            p3 = left.centreLeft;
                            break;
                        case 0b0010:
                            p2 = left.centreRight;
                            p3 = left.centreBottom;
                            neighborStraight1 = true;
                            break;
                    }
                    break;
                case 0b1011:
                    switch (top.configuration)
                    {
                        case 0b1001:
                            p0 = top.centreTop;
                            p1 = top.centreBottom;
                            break;
                        case 0b1101:
                            p0 = top.centreRight;
                            p1 = top.centreBottom;
                            break;
                        case 0b0001:
                            p0 = top.centreLeft;
                            p1 = top.centreBottom;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (right.configuration)
                    {
                        case 0b0011:
                            p2 = right.centreLeft;
                            p3 = right.centreRight;
                            break;
                        case 0b0111:
                            p2 = right.centreLeft;
                            p3 = right.centreTop;
                            break;
                        case 0b0001:
                            p2 = right.centreLeft;
                            p3 = right.centreBottom;
                            neighborStraight1 = true;
                            break;
                    }
                    break;
                case 0b1101:
                    switch (right.configuration)
                    {
                        case 0b1100:
                            p0 = right.centreRight;
                            p1 = right.centreLeft;
                            break;
                        case 0b1110:
                            p0 = right.centreBottom;
                            p1 = right.centreLeft;
                            break;
                        case 0b1000:
                            p0 = right.centreTop;
                            p1 = right.centreLeft;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (bot.configuration)
                    {
                        case 0b1001:
                            p2 = bot.centreTop;
                            p3 = bot.centreBottom;
                            break;
                        case 0b1011:
                            p2 = bot.centreTop;
                            p3 = bot.centreRight;
                            break;
                        case 0b1000:
                            p2 = bot.centreTop;
                            p3 = bot.centreLeft;
                            neighborStraight1 = true;
                            break;
                    }
                    break;
                case 0b1110:
                    switch (left.configuration)
                    {
                        case 0b1100:
                            p0 = left.centreLeft;
                            p1 = left.centreRight;
                            break;

                        case 0b1101:
                            p0 = left.centreBottom;
                            p1 = left.centreRight;
                            break;

                        case 0b0100:
                            p0 = left.centreTop;
                            p1 = left.centreRight;
                            neighborStraight0 = true;
                            break;
                    }
                    switch (bot.configuration)
                    {
                        case 0b0111:
                            p2 = bot.centreTop;
                            p3 = bot.centreLeft;
                            break;
                        case 0b0110:
                            p2 = bot.centreTop;
                            p3 = bot.centreBottom;
                            break;
                        case 0b0100:
                            p2 = bot.centreTop;
                            p3 = bot.centreRight;
                            neighborStraight1 = true;
                            break;

                    }
                    break;

            }
            if (neighborStraight0 && neighborStraight1)
            {
                return null;
            }
            var retval = CatmullRomSpline.getPoints(p0.position, p1.position, p2.position, p3.position);

            return retval;
        }


        void MeshFromSpline(Node n0, Node n1, Node n2, List<Vector3> splineVertecies)
        {
            Node[] splineNodes = getSplineNodes(splineVertecies);
            AssignVertices(n0, n1, n2);
            AssignVertices(splineNodes);
            CreateTriangle(n0, n1, splineNodes[0]);
            for (int i = 0; i< splineNodes.Length - 1; i++)
            {
                CreateTriangle(n0, splineNodes[i], splineNodes[i+1]);
            }

            CreateTriangle(n0, splineNodes[splineNodes.Length - 1], n2);


        }

        private Node[] getSplineNodes(List<Vector3> splineVertecies)
        {
            List<Node> nodes = new List<Node>();
            foreach (Vector3 pos in splineVertecies)
            {
                nodes.Add(new Node(pos));
            }
            return nodes.ToArray();
        }

        void MeshFromPoints(params Node[] points) {
            AssignVertices(points);

            if (points.Length >= 3)
                CreateTriangle(points[0], points[1], points[2]);
            if (points.Length >= 4)
                CreateTriangle(points[0], points[2], points[3]);
            if (points.Length >= 5) 
                CreateTriangle(points[0], points[3], points[4]);
            if (points.Length >= 6)
                CreateTriangle(points[0], points[4], points[5]);

        }

        void AssignVertices(params Node[] points) {
            for (int i = 0; i < points.Length; i ++) {
                if (points[i].vertexIndex == -1) {
                    points[i].vertexIndex = vertices.Count;
                    vertices.Add(points[i].position);
                }
            }
        }

        void CreateTriangle(Node a, Node b, Node c) {
            triangles.Add(a.vertexIndex);
            triangles.Add(b.vertexIndex);
            triangles.Add(c.vertexIndex);

            Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
            AddTriangleToDictionary (triangle.vertexIndexA, triangle);
            AddTriangleToDictionary (triangle.vertexIndexB, triangle);
            AddTriangleToDictionary (triangle.vertexIndexC, triangle);
        }

        void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
            if (triangleDictionary.ContainsKey (vertexIndexKey)) {
                triangleDictionary [vertexIndexKey].Add (triangle);
            } else {
                List<Triangle> triangleList = new List<Triangle>();
                triangleList.Add(triangle);
                triangleDictionary.Add(vertexIndexKey, triangleList);
            }
        }

        public Mesh CreateWallMesh(int wallHeight)
        {

            CalculateMeshOutlines();

            wallVertices = new List<Vector3>();
            wallTriangles = new List<int>();
            Mesh wallMesh = new Mesh();

            foreach (List<int> outline in outlines)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    int startIndex = wallVertices.Count;
                    wallVertices.Add(vertices[outline[i]]); // left
                    wallVertices.Add(vertices[outline[i + 1]]); // right
                    wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                    wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex + 0);
                }
            }
            wallMesh.vertices = wallVertices.ToArray();
            wallMesh.triangles = wallTriangles.ToArray();
            return wallMesh;
        }
        void CalculateMeshOutlines() {

            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex ++) {
                if (!checkedVertices.Contains(vertexIndex)) {
                    int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                    if (newOutlineVertex != -1) {
                        checkedVertices.Add(vertexIndex);

                        List<int> newOutline = new List<int>();
                        newOutline.Add(vertexIndex);
                        outlines.Add(newOutline);
                        FollowOutline(newOutlineVertex, outlines.Count - 1);
                        outlines[outlines.Count - 1].Add(vertexIndex);
                    }
                }
            }
        }

        void FollowOutline(int vertexIndex, int outlineIndex) {
            outlines [outlineIndex].Add (vertexIndex);
            checkedVertices.Add (vertexIndex);
            int nextVertexIndex = GetConnectedOutlineVertex (vertexIndex);

            if (nextVertexIndex != -1) {
                FollowOutline(nextVertexIndex, outlineIndex);
            }
        }

        private int GetConnectedOutlineVertex(int vertexIndex) {
            List<Triangle> trianglesContainingVertex = triangleDictionary [vertexIndex];

            for (int i = 0; i < trianglesContainingVertex.Count; i ++) {
                Triangle triangle = trianglesContainingVertex[i];

                for (int j = 0; j < 3; j ++) {
                    int vertexB = triangle[j];
                    if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB)) {
                        if (IsOutlineEdge(vertexIndex, vertexB)) {
                            return vertexB;
                        }
                    }
                }
            }

            return -1;
        }

        private bool IsOutlineEdge(int vertexA, int vertexB) {
            List<Triangle> trianglesContainingVertexA = triangleDictionary [vertexA];
            int sharedTriangleCount = 0;

            for (int i = 0; i < trianglesContainingVertexA.Count; i ++) {
                if (trianglesContainingVertexA[i].Contains(vertexB)) {
                    sharedTriangleCount ++;
                    if (sharedTriangleCount > 1) {
                        break;
                    }
                }
            }

            return sharedTriangleCount == 1;
        }

        struct Triangle {
            public int vertexIndexA;
            public int vertexIndexB;
            public int vertexIndexC;
            int[] vertices;

            public Triangle (int a, int b, int c) {
                vertexIndexA = a;
                vertexIndexB = b;
                vertexIndexC = c;

                vertices = new int[3];
                vertices[0] = a;
                vertices[1] = b;
                vertices[2] = c;
            }

            public int this[int i] {
                get {
                    return vertices[i];
                }
            }


            public bool Contains(int vertexIndex) {
                return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
            }
        }

        public class SquareGrid {
            public Square[,] squares;

            public SquareGrid(NativeArray<int> map, int width, int height, float squareSize, TileType meshType)
            {

                float mapWidth = width * squareSize;
                float mapHeight = height * squareSize;

                ControlNode[,] controlNodes = new ControlNode[width, height];

                for (int y = 0; y < height; y++)
                {
                    int y_index = width * y;
                    for (int x = 0; x < width; x++)
                    {
                        Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                        controlNodes[x, y] = new ControlNode(pos, map[x + y_index] == (int)meshType, squareSize);
                    }
                }

                squares = new Square[width - 1, height - 1];
                for (int x = 0; x < width - 1; x++)
                {
                    for (int y = 0; y < height - 1; y++)
                    {
                        squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                    }
                }
            }

            public int GetLength(int dim){
                return squares.GetLength(dim);
            }

            public Square GetSquare(int x, int y){
                return squares[x,y];
            }
        }
        
        public class Square {

            public ControlNode topLeft, topRight, bottomRight, bottomLeft;
            public Node centreTop, centreRight, centreBottom, centreLeft;
            public uint configuration;

            public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
                topLeft = _topLeft;
                topRight = _topRight;
                bottomRight = _bottomRight;
                bottomLeft = _bottomLeft;

                centreTop = topLeft.right;
                centreRight = bottomRight.above;
                centreBottom = bottomLeft.right;
                centreLeft = bottomLeft.above;

                if (topLeft.active)
                    configuration += 8;
                if (topRight.active)
                    configuration += 4;
                if (bottomRight.active)
                    configuration += 2;
                if (bottomLeft.active)
                    configuration += 1;
            }

        }

        public class Node {
            public Vector3 position;
            public int vertexIndex = -1;

            public Node(Vector3 _pos) {
                position = _pos;
            }
        }

        public class ControlNode : Node {

            public bool active;
            public Node above, right;

            public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
                active = _active;
                above = new Node(position + Vector3.forward * squareSize/2f);
                right = new Node(position + Vector3.right * squareSize/2f);
            }

        }
    }

}
