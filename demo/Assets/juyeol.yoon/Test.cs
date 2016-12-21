using UnityEngine;
using System.Collections.Generic;
using Delaunay.Geo;
using System.Linq;


namespace style0912 {
    [ExecuteInEditMode]
    public class Test : MonoBehaviour {

        [SerializeField]
        bool _generateGraph;

        [SerializeField]
        bool _generateMesh;

        [SerializeField]
        bool _generateWithTexture;

        [SerializeField]
        bool _generateWithMesh;

        [SerializeField]
        int _seed;
        [SerializeField]
        bool _useRandomSeed;

        [SerializeField]
        Delaunay.KruskalType _kruskalType;
        [SerializeField]
        bool _regenerateSpanningTree;
        [SerializeField]
        bool _spanningTreeTestFlag;
        [SerializeField]
        bool _regenerateTexture;

        [SerializeField]
        int _pointCount = 300;
        [SerializeField]
        float _mapWidth = 100;
        [SerializeField]
        float _mapHeight = 50;
        [SerializeField]
        float _lakeThreshold = 0.3f;

        [SerializeField]
        int _textureScale = 1;

        [SerializeField]
        Renderer _renderer;

        [SerializeField]
        int _lloydRelaxations;

        [SerializeField]
        List<Vector2> _points;
        [SerializeField]
        bool _drawPoints;

        [SerializeField]
        List<LineSegment> _edges = null;
        [SerializeField]
        bool _drawEdges;
        [SerializeField]
        bool _drawMesh;
        [SerializeField]
        List<LineSegment> _spanningTree;
        [SerializeField]
        bool _drawSpanningTree;
        [SerializeField]
        List<LineSegment> _delaunayTriangulation;
        [SerializeField]
        bool _drawDelaunayTriangulation;

        [SerializeField]
        bool _drawOutline;

        [SerializeField]
        Delaunay.Voronoi _voronoi;

        //[SerializeField]
        Graph _graph;

        [SerializeField]
        BiomeColors biomeColors;

        [SerializeField]
        MeshFilter meshfilter;

        [SerializeField]
        AnimationCurve heightCurve;

        [SerializeField]
        float heightOffset;

        void Update() {
            if (_generateGraph) {
                _generateGraph = false;
                Generate();
            }

            if (_regenerateSpanningTree && null != _voronoi && null != _graph) {
                _regenerateSpanningTree = false;
                GenerateSpanningTree(_spanningTreeTestFlag);
            }

            if(null != _graph && _regenerateTexture) {
                _regenerateTexture = false;
                GenerateTexture();
            }

            if(null != _graph && _generateMesh && null != meshfilter) {
                _generateMesh = false;
                GenerateFlatMesh(meshfilter, heightCurve, heightOffset);
            }
        }

        void Generate() {
            if (_useRandomSeed) {
                _seed = System.DateTime.Now.GetHashCode();
            }
            style0912.Random random = new style0912.Random(_seed);

            List<uint> colors = new List<uint>();
            _points = new List<Vector2>();

            for (int i = 0; i < _pointCount; i++) {
                colors.Add(0);
                _points.Add(new Vector2(random.Range(0, _mapWidth), random.Range(0, _mapHeight)));
            }

            for (int i = 0; i < _lloydRelaxations; i++)
                _points = RelaxPoints(random, _points, _mapWidth, _mapHeight).ToList();

            if (null != _voronoi)
                _voronoi.Dispose();
            _voronoi = new Delaunay.Voronoi(random, _points, colors, new Rect(0, 0, _mapWidth, _mapHeight));

            _graph = new Graph(random, _points, _voronoi, (int)_mapWidth, (int)_mapHeight, _lakeThreshold);

            _edges = _voronoi.VoronoiDiagram();
            _delaunayTriangulation = _voronoi.DelaunayTriangulation();

            GenerateSpanningTree(_spanningTreeTestFlag);

            if(_generateWithTexture)
                GenerateTexture();

            if(_generateWithMesh)
                GenerateFlatMesh(meshfilter, heightCurve, heightOffset);
        }

        private void GenerateSpanningTree(bool testFlag) {
            if (testFlag) {
                //List<Center> centers = _graph.centers.FindAll(x => x.ocean || x.water);
                //List<Edge> edges = new List<Edge>();
                //centers.ForEach(x => edges.AddRange(x.borders));
                //edges.AddRange(_graph.edges.FindAll(x => x.river > 0));
                //_spanningTree = _voronoi.SpanningTree(edges, _kruskalType);
                _spanningTree = _voronoi.SpanningTree(_kruskalType);
                List<Center> centers = _graph.centers.FindAll(x => x.ocean || x.water);
                
            }
            else {
                _spanningTree = _voronoi.SpanningTree(_kruskalType);
            }
        }

        private void GenerateTexture() {
            Texture2D texture = _graph.GenerateTexture(_textureScale, _drawEdges);
            if (null != _renderer)
                _renderer.sharedMaterial.mainTexture = texture;
        }

        public static IEnumerable<Vector2> RelaxPoints(style0912.Random seed, IEnumerable<Vector2> startingPoints, float width, float height) {
            Delaunay.Voronoi v = new Delaunay.Voronoi(seed, startingPoints.ToList(), null, new Rect(0, 0, width, height));
            foreach (var point in startingPoints) {
                var region = v.Region(point);
                point.Set(0, 0);
                foreach (var r in region)
                    point.Set(point.x + r.x, point.y + r.y);

                point.Set(point.x / region.Count, point.y / region.Count);
                yield return point;
            }
        }

        void OnDrawGizmos() {
            

            if (null != _graph && null != biomeColors && _drawMesh) {
                DrawMesh2();
            }

            if (_edges != null && _drawEdges) {
                Gizmos.color = Color.white;
                for (int i = 0; i < _edges.Count; i++) {
                    Vector2 left = (Vector2)_edges[i].p0;
                    Vector2 right = (Vector2)_edges[i].p1;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }

            Gizmos.color = Color.magenta;
            if (_delaunayTriangulation != null && _drawDelaunayTriangulation) {
                for (int i = 0; i < _delaunayTriangulation.Count; i++) {
                    Vector2 left = (Vector2)_delaunayTriangulation[i].p0;
                    Vector2 right = (Vector2)_delaunayTriangulation[i].p1;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }

            if (_spanningTree != null && _drawSpanningTree) {
                Gizmos.color = Color.green;
                for (int i = 0; i < _spanningTree.Count; i++) {
                    LineSegment seg = _spanningTree[i];
                    Vector2 left = (Vector2)seg.p0;
                    Vector2 right = (Vector2)seg.p1;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }

            
            if (null != _graph && _points != null && _drawPoints) {
                Gizmos.color = Color.red;
                for (int i = 0; i < _graph.centers.Count; ++i) {
                    Gizmos.DrawSphere(_graph.centers[i].point, 0.2f);
                }
                //for (int i = 0; i < _points.Count; i++) {
                //    Gizmos.DrawSphere(_points[i], 0.2f);
                //}
            }

            if (_drawOutline) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(new Vector2(0, 0), new Vector2(0, _mapHeight));
                Gizmos.DrawLine(new Vector2(0, 0), new Vector2(_mapWidth, 0));
                Gizmos.DrawLine(new Vector2(_mapWidth, 0), new Vector2(_mapWidth, _mapHeight));
                Gizmos.DrawLine(new Vector2(0, _mapHeight), new Vector2(_mapWidth, _mapHeight));
            }
        }

        private void DrawMesh() {
            for (int i = 0; i < _graph.centers.Count; ++i) {
                if (0 < _graph.centers[i].corners.Count) {
                    Mesh mesh = new Mesh();
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();
                    Gizmos.color = biomeColors.Colors[_graph.centers[i].biome].color;
                    vertices.Add(_graph.centers[i].corners[0].point);
                    for (int x = 0; x < _graph.centers[i].corners.Count; ++x) {

                        triangles.Add(0);
                        triangles.Add(x);
                        triangles.Add(x + 1);

                        vertices.Add(_graph.centers[i].corners[x].point);
                    }

                    vertices.Add(_graph.centers[i].corners[_graph.centers[i].corners.Count - 1].point);

                    mesh.vertices = vertices.ToArray();
                    //mesh.col
                    mesh.triangles = triangles.ToArray();
                    mesh.RecalculateNormals();
                    Gizmos.DrawMesh(mesh, Vector3.zero, Quaternion.identity);
                }
            }
        }

        private void DrawMesh2() {
            for (int i = 0; i < _graph.centers.Count; ++i) {
                if (0 < _graph.centers[i].corners.Count) {
                    Mesh mesh = new Mesh();
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();
                    Gizmos.color = biomeColors.Colors[_graph.centers[i].biome].color;
                    vertices.Add(_graph.centers[i].point);
                    for (int x = 0; x < _graph.centers[i].corners.Count; ++x) {
                        vertices.Add(_graph.centers[i].corners[x].point);
                    }

                    for (int x = 1; x < _graph.centers[i].corners.Count; ++x) {
                        triangles.Add(0);
                        triangles.Add(x);
                        triangles.Add(x + 1);
                    }

                    triangles.Add(0);
                    triangles.Add(_graph.centers[i].corners.Count);
                    triangles.Add(1);

                    mesh.vertices = vertices.ToArray();
                    //mesh.col
                    mesh.triangles = triangles.ToArray();
                    mesh.RecalculateNormals();
                    Gizmos.DrawMesh(mesh, Vector3.zero, Quaternion.identity);
                }
            }
        }

        private void GenerateMesh(MeshFilter meshfilter, AnimationCurve heightCurve, float heightOffset) {
            meshfilter.mesh.Clear();
            List<Vector3> totalVertices = new List<Vector3>();
            List<int> totalTriangles = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<Vector3> normals = new List<Vector3>();

            for (int i = 0; i < _graph.centers.Count; ++i) {
                if (0 < _graph.centers[i].corners.Count) {
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();
                    //Gizmos.color = biomeColors.Colors[_graph.centers[i].biome].color;

                    float height = GetAverage(_graph.centers[i]);
                    vertices.Add(new Vector3(_graph.centers[i].point.x, heightCurve.Evaluate(height) * heightOffset, _graph.centers[i].point.y));
                    colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);

                    vertices.Add(new Vector3(_graph.centers[i].corners[0].point.x, heightCurve.Evaluate(_graph.centers[i].corners[0].elevation) * heightOffset, _graph.centers[i].corners[0].point.y));
                    colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);

                    for (int x = 1; x < _graph.centers[i].corners.Count; ++x) {
                        vertices.Add(new Vector3(_graph.centers[i].corners[x].point.x, heightCurve.Evaluate(_graph.centers[i].corners[x].elevation) * heightOffset, _graph.centers[i].corners[x].point.y));
                        colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                        triangles.Add(0);
                        triangles.Add(x);
                        triangles.Add(x + 1);

                        //normals.Add()
                    }

                    triangles.Add(0);
                    triangles.Add(_graph.centers[i].corners.Count);
                    triangles.Add(1);

                    for (int x = 0; x < triangles.Count; ++x) {
                        triangles[x] += totalVertices.Count;
                    }

                    totalVertices.AddRange(vertices);
                    totalTriangles.AddRange(triangles);
                }
            }
            meshfilter.mesh.vertices = totalVertices.ToArray();
            meshfilter.mesh.colors = colors.ToArray();
            meshfilter.mesh.normals = normals.ToArray();
            meshfilter.mesh.triangles = totalTriangles.ToArray();
            //meshfilter.mesh.RecalculateNormals();
            meshfilter.mesh.RecalculateBounds();
        }

        private void GenerateFlatMesh(MeshFilter meshfilter, AnimationCurve heightCurve, float heightOffset) {
            meshfilter.mesh.Clear();
            List<Vector3> totalVertices = new List<Vector3>();
            List<int> totalTriangles = new List<int>();
            //List<Vector2> uv = new List<Vector2>();
            List<Color> colors = new List<Color>();

            for (int i = 0; i < _graph.centers.Count; ++i) {
                if (0 < _graph.centers[i].corners.Count && _graph.centers[i].biome != Biome.Ocean) {
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();

                    float height = _graph.centers[i].elevation;

                    for (int x = 0; x < _graph.centers[i].corners.Count - 1; ++x) {
                        vertices.Add(new Vector3(_graph.centers[i].point.x, heightCurve.Evaluate(height) * heightOffset, _graph.centers[i].point.y));
                        colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                        triangles.Add(vertices.Count-1);

                        vertices.Add(new Vector3(_graph.centers[i].corners[x].point.x, heightCurve.Evaluate(_graph.centers[i].corners[x].elevation) * heightOffset, _graph.centers[i].corners[x].point.y));
                        colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                        triangles.Add(vertices.Count - 1);

                        vertices.Add(new Vector3(_graph.centers[i].corners[x + 1].point.x, heightCurve.Evaluate(_graph.centers[i].corners[x + 1].elevation) * heightOffset, _graph.centers[i].corners[x + 1].point.y));
                        colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                        triangles.Add(vertices.Count - 1);
                    }

                    vertices.Add(new Vector3(_graph.centers[i].point.x, heightCurve.Evaluate(height) * heightOffset, _graph.centers[i].point.y));
                    colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                    triangles.Add(vertices.Count - 1);

                    vertices.Add(new Vector3(_graph.centers[i].corners[_graph.centers[i].corners.Count - 1].point.x, heightCurve.Evaluate(_graph.centers[i].corners[_graph.centers[i].corners.Count - 1].elevation) * heightOffset, _graph.centers[i].corners[_graph.centers[i].corners.Count - 1].point.y));
                    colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                    triangles.Add(vertices.Count - 1);

                    vertices.Add(new Vector3(_graph.centers[i].corners[0].point.x, heightCurve.Evaluate(_graph.centers[i].corners[0].elevation) * heightOffset, _graph.centers[i].corners[0].point.y));
                    colors.Add(biomeColors.Colors[_graph.centers[i].biome].color);
                    triangles.Add(vertices.Count - 1);

                    for (int x = 0; x < triangles.Count; ++x) {
                        triangles[x] += totalVertices.Count;
                    }

                    totalVertices.AddRange(vertices);
                    totalTriangles.AddRange(triangles);
                }
            }
            meshfilter.mesh.vertices = totalVertices.ToArray();
            meshfilter.mesh.colors = colors.ToArray();
            meshfilter.mesh.triangles = totalTriangles.ToArray();
            meshfilter.mesh.RecalculateNormals();
            meshfilter.mesh.RecalculateBounds();
        }

        float GetAverage(Center center) {
            //return center.elevation;
            float sum = 0;
            for(int i = 0; i < center.corners.Count; ++i) {
                sum += center.corners[i].elevation;
            }

            return Mathf.Lerp(center.elevation, sum / center.neighbors.Count, 0.5f);

            //return sum / center.neighbors.Count;
        }
    }
}