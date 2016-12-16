using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class SkyIslandGeneratorCS : MonoBehaviour {

    public string seed;
    public bool useRandomSeed;

    //island dimensions
    public float areaSize = 100f;
    //the distance between the vertices
    public float interval = 1.0f;
    //the height of top of the island
    public float topHeight = 1.0f;
    public float topExponent = 1.0f;
    //the height of bottom of the island
    public float bottomHeight = 1.0f;
    public float bottomExponent = 1.0f;

    public Material material;
    //shape of the island
    public Texture2D shape;

    //Height maps. Maximum 10 layers
    //Scale of Perlin noise. From it depends density of the mountains. The higher the value the denser the mountains.
    public List<float> noiseScale = new List<float>();
    //Offset of Perlin noise on coordinate. You can set random value.
    public List<Vector2> offset = new List<Vector2>();
    public List<bool> offsetRandom = new List<bool>();
    //The influence of this layer on the previous ones.
    public List<float> alpha = new List<float>();
    //Blend mode of height map layers(multiply, darken, lighten, exclusion).
    public List<int> blendMode = new List<int>();
    //for foldout in editor
    public bool hmapFoldout;

    //Colors. Maximum 10 layers
    //Color of this layer.
    public List<Color> colors = new List<Color>();
    //The minimum height to which can be given color.
    public List<float> colorMinHeight = new List<float>();
    //The maximum height of which can be given color.
    public List<float> colorMaxHeight = new List<float>();
    //The minimum value of the transition. The higher the value, the greater must be the angle at which the surface can be given color.
    //It is intended to determine the vertical surfaces.
    public List<float> colorTransValue = new List<float>();
    //for foldout in editor
    public bool colorFoldout;

    //The color of the boundary surfaces of the island.
    public Color borderColor;
    //position on UV, bound to the variable "colorsOnUv"
    public int borderColorUv = 0;
    //Color of bottom of the island.
    public Color bottomColor;
    //position on UV, bound to the variable "colorsOnUv"
    public int bottomColorUv = 0;

    //Create objects. Maximum 10 layers
    //Object prefab.
    public List<Transform> objectPrefab = new List<Transform>();
    //The minimum height at which the object may be.
    public List<float> objectMinHeight = new List<float>();
    //The maximum height at which the object may be.
    public List<float> objectMaxHeight = new List<float>();
    //The maximum angle of the surface on which the object can be located.
    public List<float> objectMaxAngle = new List<float>();
    //Scale of Perlin noise. It determines what objects are in what areas of the island.
    public List<float> objectNoiseScale = new List<float>();
    //Offset of Perlin noise on coordinate. You can set random value.
    public List<Vector2> objectOffset = new List<Vector2>();
    public List<bool> objectOffsetRandom = new List<bool>();
    //Distance between objects.
    public List<float> objectInterval = new List<float>();
    //for foldout in editor
    public bool objectFoldout;
    //generated mesh and prefab save path
    public string savePath = "Assets/SkyIsland/IslandPrefabs/NewIsland";

    //4x4 texture that has all the colors that you specified
    Texture2D texture;
    //automatically calculated coordinates on UV
    Vector2[] colorsOnUv = new Vector2[16];

    //array of heights
    float[] heights;
    Mesh mesh;
    Mesh btmMesh;
    MeshFilter filter;
    MeshRenderer meshRenderer;

    //fixed size of the island. Calculated from size of the island and interval between vertices.
    private Vector2 size;

    //array of boundary vertices
    private List<int> extremeVertices = new List<int>();

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uv = new List<Vector2>();
    private List<int> triangles = new List<int>();

    void Start() {
        GenerateIsland();
    }

    public void GenerateAndSave() {
#if UNITY_EDITOR
        if (savePath.Length > 0) {
            GenerateIsland();

            UnityEditor.AssetDatabase.CreateAsset(mesh, savePath + "/" + gameObject.name + "_mesh.asset");
            UnityEditor.AssetDatabase.CreateAsset(btmMesh, savePath + "/" + gameObject.name + "_bottomMesh.asset");
            UnityEditor.AssetDatabase.CreateAsset(texture, savePath + "/" + gameObject.name + "_colors.asset");

            var prefab = UnityEditor.PrefabUtility.CreateEmptyPrefab(savePath + "/" + gameObject.name + ".prefab");
            UnityEditor.PrefabUtility.ReplacePrefab(gameObject, prefab, UnityEditor.ReplacePrefabOptions.ReplaceNameBased);

            Debug.Log("<color=blue>Saved to: </color>" + savePath);
        }
        else {
            if (savePath.Length == 0)
                Debug.Log("<color=red>'Save path' is empty\nExample: Assets/SkyIsland/IslandPrefabs/NewIsland</color>");
        }
#endif
    }

    void ClearIsland() {
        heights = new float[(int)size.x * (int)size.y];
        mesh = new Mesh();
        extremeVertices = new List<int>();
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uv = new List<Vector2>();
        triangles = new List<int>();

        //Created objects become children of the island. They need to be destroyed when re-creating island
        for (var i = transform.childCount - 1; i >= 0; --i) {
            var child = transform.GetChild(i).gameObject;
            DestroyImmediate(child);
        }
    }

    public void GenerateIsland() {
        filter = GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        //Height maps. If offset of Perlin noise is random.
        if (useRandomSeed)
            seed = System.DateTime.Now.GetHashCode().ToString();
        style0912.Random rand = new style0912.Random(seed.GetHashCode());
        //Random.seed = 0;
        for (var offRnd = 0; offRnd < offsetRandom.Count; offRnd++) {
            if (offsetRandom[offRnd]) {
                offset[offRnd] = new Vector2(rand.Range(-1000000.0f, 1000000.0f), rand.Range(-1000000.0f, 1000000.0f));
            }
        }
        //Objects. If offset of Perlin noise is random.
        for (var objOffRnd = 0; objOffRnd < objectOffsetRandom.Count; objOffRnd++) {
            if (objectOffsetRandom[objOffRnd]) {
                objectOffset[objOffRnd] = new Vector2(rand.Range(-1000000.0f, 1000000.0f), rand.Range(-1000000.0f, 1000000.0f));
            }
        }

        //fixed size of the island. Calculated from size of the island and interval between vertices.
        size = new Vector2(Mathf.Round(areaSize / interval), Mathf.Round(areaSize / interval));


        ClearIsland();
        //Generate height maps
        CalcNoise();

        //Create Top
        for (var zt = 0; zt < size.y - 1; zt++) {
            for (var xt = 0; xt < size.x - 1; xt++) {
                //array of numbers which will take the form "0000"
                var biQuad = new int[4];

                //taken 4 vertices
                int vertA = (int)(zt * size.x + xt);
                int vertB = (int)((zt + 1) * size.x + xt);
                int vertC = (int)((zt + 1) * size.x + xt + 1);
                int vertD = (int)(zt * size.x + xt + 1);

                //if height of vertices > 0, then character of "biQuad" equals 1. "1111" or "0101" etc.
                if (heights[vertA] > 0) biQuad[0] = 1;
                if (heights[vertB] > 0) biQuad[1] = 1;
                if (heights[vertC] > 0) biQuad[2] = 1;
                if (heights[vertD] > 0) biQuad[3] = 1;

                MakeTriangles(vertA, vertB, vertC, vertD, "" + biQuad[0] + biQuad[1] + biQuad[2] + biQuad[3]);
            }
        }

        //bottom part of island is another object
        CopyToBottom();

        //assigned to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();

        filter.sharedMesh = mesh;
        meshRenderer.material = material;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //create collider
        GetComponent<MeshCollider>().sharedMesh = mesh;
        //set texture
        meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);

        //add objects
        CreateObject(rand);
    }

    void CopyToBottom() {
        //create empty game object
        var bottom = new GameObject();
        bottom.transform.position = transform.position;
        bottom.name = "Bottom";

        //add scripts
        btmMesh = new Mesh();
        var btmfilter = bottom.AddComponent<MeshFilter>();
        var btmMeshRenderer = bottom.AddComponent<MeshRenderer>();
        var btmMeshCollider = bottom.AddComponent<MeshCollider>();

        List<int> btmExtremeVertices = new List<int>();
        List<Vector3> btmVertices = new List<Vector3>();
        List<Vector3> btmNormals = new List<Vector3>();
        List<Vector2> btmUv = new List<Vector2>();
        List<int> btmTriangles = new List<int>();

        //copy vertices, normals, UVs from top
        var vCount = vertices.Count;
        for (var cpv = 0; cpv < vCount; cpv++) {
            btmVertices.Add(new Vector3(vertices[cpv].x, Mathf.Pow(vertices[cpv].y, bottomExponent) * -bottomHeight / topHeight, vertices[cpv].z) + new Vector3(0, -interval / 2, 0));
            btmNormals.Add(normals[cpv]);
            btmUv.Add(colorsOnUv[bottomColorUv]);
        }

        //copy triangles from top
        var tCount = triangles.Count / 3;
        for (var cpt = 0; cpt < tCount; cpt++) {
            btmTriangles.Add(triangles[cpt * 3 + 1]);
            btmTriangles.Add(triangles[cpt * 3]);
            btmTriangles.Add(triangles[cpt * 3 + 2]);
        }

        //copy border vertices
        var evCount = extremeVertices.Count;
        for (var ev = 0; ev < evCount; ev++) {
            btmExtremeVertices.Add(extremeVertices[ev] + vCount);
            btmVertices[extremeVertices[ev]] = vertices[extremeVertices[ev]];
        }

        //assigned to the mesh
        btmMesh.vertices = btmVertices.ToArray(); ;
        btmMesh.normals = btmNormals.ToArray(); ;
        btmMesh.uv = btmUv.ToArray(); ;
        btmMesh.triangles = btmTriangles.ToArray(); ;

        btmfilter.sharedMesh = btmMesh;
        btmMeshRenderer.material = material;
        btmMesh.RecalculateNormals();
        btmMesh.RecalculateBounds();
        //create collider
        btmMeshCollider.sharedMesh = btmMesh;
        //set texture
        btmMeshRenderer.sharedMaterial.SetTexture("_MainTex", texture);

        //make parrent
        bottom.transform.parent = transform;
    }

    void CreateObject(style0912.Random rand) {
        RaycastHit hit;
        for (int obj = 0; obj < objectPrefab.Count; obj++) {
            //fixed size of the island with objects. Calculated from size of the island and interval between objects.
            var objSize = Mathf.Round(areaSize / objectInterval[obj]);
            //to make center of mesh like a center of object
            var delayPos = new Vector3((areaSize - objectInterval[obj]) / 2, 0, (areaSize - objectInterval[obj]) / 2);
            //random scatter from grid
            var randomDelay = objectInterval[obj] / 3;

            for (int zobj = 0; zobj < objSize; zobj++) {
                for (int xobj = 0; xobj < objSize; xobj++) {
                    float xCoord = xobj / objSize * objectNoiseScale[obj];
                    float yCoord = zobj / objSize * objectNoiseScale[obj];
                    //area in which the object can be
                    var objHeight = Mathf.PerlinNoise(objectOffset[obj].x + xCoord, objectOffset[obj].y + yCoord);
                    if (objHeight > 0.4) {
                        var xpos = xobj * objectInterval[obj] + transform.position.x;
                        var zpos = zobj * objectInterval[obj] + transform.position.z;
                        if (Physics.Raycast(new Vector3(xpos + rand.Range(-randomDelay, randomDelay), topHeight, zpos + rand.Range(-randomDelay, randomDelay)) - delayPos, -Vector3.up, out hit) && hit.collider.gameObject == gameObject) {
                            if (hit.point.y > objectMinHeight[obj] * topHeight && hit.point.y <= objectMaxHeight[obj] * topHeight) {
                                if (Vector3.Angle(hit.normal, Vector3.up) < objectMaxAngle[obj]) {
                                    //create object on hit point
                                    var newObj = Instantiate(objectPrefab[obj], hit.point, Quaternion.Euler(0, rand.Range(0.0f, 360.0f), 0)) as Transform;
                                    //make parrent
                                    newObj.parent = transform;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    int SetVertices(int a, int vertCol) {
        var x = a % size.x;
        var y = (int)(((float)a - (x + 1)) / size.y) + 1;
        var h = Mathf.Pow(heights[a], topExponent);

        vertices.Add(new Vector3(x / size.x * areaSize, h * topHeight, y / size.y * areaSize) - new Vector3((areaSize - interval) / 2, 0, (areaSize - interval) / 2));
        normals.Add(Vector3.up);
        uv.Add(colorsOnUv[vertCol]);

        //it's a number of vertices of triangle
        var verticesA = vertices.Count - 1;
        return verticesA;
    }

    int SetExtremeVertices(int a, int b) {
        var ab = 0;

        var x = a % size.x;
        var y = Mathf.Round(((float)a - (x + 1)) / size.y);
        var h = Mathf.Pow(heights[(int)(y * size.x + x)], 2);
        //position of "a" vertices
        var aVert = new Vector3(x / size.x * areaSize, h * topHeight, y / size.y * areaSize) - new Vector3((areaSize - interval) / 2, interval / 4, (areaSize - interval) / 2);

        x = b % size.x;
        y = Mathf.Round(((float)b - (x + 1)) / size.y);
        h = Mathf.Pow(heights[b], 2);
        //position of "b" vertices
        var bVert = new Vector3(x / size.x * areaSize, h * topHeight, y / size.y * areaSize) - new Vector3((areaSize - interval) / 2, interval / 4, (areaSize - interval) / 2);

        //position between "a" and "b" vertices
        var abCenter = (aVert + bVert) / 2;

        vertices.Add(abCenter);
        normals.Add(Vector3.up);
        uv.Add(colorsOnUv[borderColorUv]);
        //number of vertices of triangle
        ab = vertices.Count - 1;
        extremeVertices.Add(ab);

        return ab;
    }

    //make triangles from "biQuad"
    void MakeTriangles(int a, int b, int c, int d, string biQuadStr) {
        if (biQuadStr == "0001") {
            triangles.Add(SetExtremeVertices(d, a));
            triangles.Add(SetExtremeVertices(c, d));
            triangles.Add(SetVertices(d, borderColorUv));
        }

        if (biQuadStr == "0010") {
            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetExtremeVertices(c, d));
        }

        if (biQuadStr == "0011") {
            triangles.Add(SetExtremeVertices(d, a));
            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetVertices(d, borderColorUv));

            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetVertices(d, borderColorUv));
        }

        if (biQuadStr == "0100") {
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetExtremeVertices(b, c));
        }

        if (biQuadStr == "0101") {
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetExtremeVertices(d, a));

            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetVertices(d, borderColorUv));
            triangles.Add(SetExtremeVertices(d, a));

            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetExtremeVertices(c, d));
            triangles.Add(SetVertices(d, borderColorUv));

            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetExtremeVertices(c, d));
        }

        if (biQuadStr == "0110") {
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetVertices(c, borderColorUv));

            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetExtremeVertices(c, d));
        }

        if (biQuadStr == "0111") {
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetVertices(d, borderColorUv));

            triangles.Add(SetVertices(d, borderColorUv));
            triangles.Add(SetExtremeVertices(d, a));
            triangles.Add(SetExtremeVertices(a, b));

            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetVertices(d, borderColorUv));
        }

        if (biQuadStr == "1000") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetExtremeVertices(d, a));
        }

        if (biQuadStr == "1001") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(d, borderColorUv));

            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetExtremeVertices(c, d));
            triangles.Add(SetVertices(d, borderColorUv));
        }

        if (biQuadStr == "1010") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetVertices(c, borderColorUv));

            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetVertices(c, borderColorUv));

            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetExtremeVertices(c, d));
            triangles.Add(SetExtremeVertices(d, a));

            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetExtremeVertices(d, a));
            triangles.Add(SetVertices(a, borderColorUv));
        }

        if (biQuadStr == "1011") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetExtremeVertices(a, b));
            triangles.Add(SetExtremeVertices(b, c));

            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetVertices(a, borderColorUv));

            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetVertices(d, borderColorUv));
            triangles.Add(SetVertices(a, borderColorUv));
        }

        if (biQuadStr == "1100") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetExtremeVertices(b, c));

            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetExtremeVertices(d, a));
        }

        if (biQuadStr == "1101") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetVertices(d, borderColorUv));

            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetExtremeVertices(b, c));
            triangles.Add(SetExtremeVertices(c, d));

            triangles.Add(SetExtremeVertices(c, d));
            triangles.Add(SetVertices(d, borderColorUv));
            triangles.Add(SetVertices(b, borderColorUv));
        }

        if (biQuadStr == "1110") {
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetVertices(b, borderColorUv));
            triangles.Add(SetVertices(c, borderColorUv));

            triangles.Add(SetVertices(c, borderColorUv));
            triangles.Add(SetExtremeVertices(c, d));
            triangles.Add(SetExtremeVertices(d, a));

            triangles.Add(SetExtremeVertices(d, a));
            triangles.Add(SetVertices(a, borderColorUv));
            triangles.Add(SetVertices(c, borderColorUv));
        }

        if (biQuadStr == "1111") {
            int newCol = 0;
            float triH = 0.0f;
            float transAngle = 0.0f;

            //fixed heights of vertices, with exponent
            var fHeightA = Mathf.Pow(heights[a], topExponent);
            var fHeightB = Mathf.Pow(heights[b], topExponent);
            var fHeightC = Mathf.Pow(heights[c], topExponent);
            var fHeightD = Mathf.Pow(heights[d], topExponent);

            //int clrs = 0;
            //int obj = 0;

            //choose diagonal to connect the square
            if (Mathf.Abs(fHeightA - fHeightC) < Mathf.Abs(fHeightB - fHeightD)) {
                //tringle ABC and CDA
                //Color ABC
                for (int clrs = 0; clrs < colors.Count; clrs++) {
                    triH = Mathf.Max(fHeightA, fHeightB, fHeightC);
                    transAngle = colorTransValue[clrs] / topHeight * interval;
                    if (triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]) {
                        triH = Mathf.Max(fHeightA, fHeightB, fHeightC) - Mathf.Min(fHeightA, fHeightB, fHeightC);
                        if (triH > transAngle) newCol = clrs;
                    }
                }
                triangles.Add(SetVertices(a, newCol));
                triangles.Add(SetVertices(b, newCol));
                triangles.Add(SetVertices(c, newCol));

                //Color CDA
                for (int clrs = 0; clrs < colors.Count; clrs++) {
                    triH = Mathf.Max(fHeightC, fHeightD, fHeightA);
                    transAngle = colorTransValue[clrs] / topHeight * interval;
                    if (triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]) {
                        triH = Mathf.Max(fHeightC, fHeightD, fHeightA) - Mathf.Min(fHeightC, fHeightD, fHeightA);
                        if (triH > transAngle) newCol = clrs;
                    }
                }
                triangles.Add(SetVertices(c, newCol));
                triangles.Add(SetVertices(d, newCol));
                triangles.Add(SetVertices(a, newCol));
            }
            else {
                //tringle ABD and BCD
                //Color ABD
                for (int clrs = 0; clrs < colors.Count; clrs++) {
                    triH = Mathf.Max(fHeightA, fHeightB, fHeightD);
                    transAngle = colorTransValue[clrs] / topHeight * interval;
                    if (triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]) {
                        triH = Mathf.Max(fHeightA, fHeightB, fHeightD) - Mathf.Min(fHeightA, fHeightB, fHeightD);
                        if (triH > transAngle) newCol = clrs;
                    }
                }
                triangles.Add(SetVertices(a, newCol));
                triangles.Add(SetVertices(b, newCol));
                triangles.Add(SetVertices(d, newCol));

                //Color BCD
                for (int clrs = 0; clrs < colors.Count; clrs++) {
                    triH = Mathf.Max(fHeightB, fHeightC, fHeightD);
                    transAngle = colorTransValue[clrs] / topHeight * interval;
                    if (triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]) {
                        triH = Mathf.Max(fHeightB, fHeightC, fHeightD) - Mathf.Min(fHeightB, fHeightC, fHeightD);
                        if (triH > transAngle) newCol = clrs;
                    }
                }
                triangles.Add(SetVertices(b, newCol));
                triangles.Add(SetVertices(c, newCol));
                triangles.Add(SetVertices(d, newCol));
            }
        }
    }

    void CalcNoise() {
        for (var y = 0; y < size.y; y++) {
            for (var x = 0; x < size.x; x++) {
                int id = (int)(y * size.x + x);
                for (var hmC = 0; hmC < noiseScale.Count; hmC++) {
                    if (x < size.x - 1 && y < size.y - 1) {
                        var xCoord = (float)x / size.x * noiseScale[hmC];
                        var yCoord = (float)y / size.y * noiseScale[hmC];
                        if (hmC == 0) {
                            heights[id] = Mathf.PerlinNoise(offset[hmC].x + xCoord, offset[hmC].y + yCoord) * alpha[hmC];
                        }
                        else {
                            var a = heights[id];
                            var b = Mathf.PerlinNoise(offset[hmC].x + xCoord, offset[hmC].y + yCoord) * alpha[hmC];
                            //blending height maps
                            heights[id] = BlendHeightMaps(a, b, blendMode[hmC]);
                        }
                    }
                }

                //blend height maps with shape("linear burn" blend mode)
                heights[id] = BlendHeightMaps(heights[id], shape.GetPixel((int)(x / size.x * shape.width), (int)(y / size.y * shape.height)).grayscale, 4);

                if (x == 0 || y == 0 || x == size.x - 1 || y == size.y - 1) {
                    heights[id] = 0;
                }
            }
        }

        //create texture
        Color[] newColors = new Color[16];
        texture = new Texture2D(4, 4);
        for (var newClrs = 0; newClrs < 16; newClrs++) {
            if (newClrs < colors.Count) {
                newColors[newClrs] = colors[newClrs];
            }
            else {
                newColors[newClrs] = new Color(1, 1, 1, 1);
            }

            var yuv = Mathf.Floor(newClrs / 4f);
            var xuv = newClrs - yuv * 4;

            //automatically settings "colorsOnUv" 
            colorsOnUv[newClrs] = new Vector2(0.25f * xuv + 0.125f, 0.25f * yuv + 0.125f);
        }

        newColors[colors.Count] = borderColor;
        newColors[colors.Count + 1] = bottomColor;
        borderColorUv = colors.Count;
        bottomColorUv = colors.Count + 1;

        //apply texture
        texture.SetPixels(newColors);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
    }

    float BlendHeightMaps(float a, float b , int mode ) {
        float result = 0;
        //multiply
        if (mode == 0) {
            result = a * b;
        }
        //darken
        if (mode == 1) {
            result = Mathf.Min(a, b);
        }
        //lighten
        if (mode == 2) {
            result = Mathf.Max(a, b);
        }
        //exclusion
        if (mode == 3) {
            result = 0.5f - 2 * (a - 0.5f) * (b - 0.5f);
        }
        //linear burn
        if (mode == 4) {
            result = a - (1 - b);
        }

        return result;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            GenerateIsland();
        }
    }
}
