using UnityEngine;
using System.Collections.Generic;
using Delaunay.Geo;
using System.Linq;


namespace style0912 {
    [ExecuteInEditMode]
    public class Test : MonoBehaviour {

        [SerializeField]
        bool _generate;

        [SerializeField]
        int seed;
        [SerializeField]
        bool useRandomSeed;

        [SerializeField]
        Delaunay.KruskalType kruskalType;
        [SerializeField]
        bool regenerateSpanningTree;

        [SerializeField]
        int _pointCount = 300;
        [SerializeField]
        float _mapWidth = 100;
        [SerializeField]
        float _mapHeight = 50;

        [SerializeField]
        int lloydRelaxations;

        [SerializeField]
        List<Vector2> _points;
        [SerializeField]
        bool drawPoints;

        [SerializeField]
        List<LineSegment> m_edges = null;
        [SerializeField]
        bool drawEdges;
        [SerializeField]
        List<LineSegment> m_spanningTree;
        [SerializeField]
        bool drawSpanningTree;
        [SerializeField]
        List<LineSegment> m_delaunayTriangulation;
        [SerializeField]
        bool drawDelaunayTriangulation;

        [SerializeField]
        Delaunay.Voronoi voronoi;

        void Update() {
            if (_generate) {
                _generate = false;
                Generate();
            }

            if (regenerateSpanningTree && null != voronoi) {
                regenerateSpanningTree = false;
                m_spanningTree = voronoi.SpanningTree(kruskalType);
            }
        }

        void Generate() {
            if (useRandomSeed) {
                seed = System.DateTime.Now.GetHashCode();
            }
            style0912.Random random = new style0912.Random(seed);

            List<uint> colors = new List<uint>();
            _points = new List<Vector2>();

            for (int i = 0; i < _pointCount; i++) {
                colors.Add(0);
                _points.Add(new Vector2(random.Next(0, _mapWidth), random.Next(0, _mapHeight)));
            }

            for (int i = 0; i < lloydRelaxations; i++)
                _points = RelaxPoints(random, _points, _mapWidth, _mapHeight).ToList();

            if (null != voronoi)
                voronoi.Dispose();
            voronoi = new Delaunay.Voronoi(random, _points, colors, new Rect(0, 0, _mapWidth, _mapHeight));
            m_edges = voronoi.VoronoiDiagram();
            m_spanningTree = voronoi.SpanningTree(kruskalType);
            m_delaunayTriangulation = voronoi.DelaunayTriangulation();
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
            Gizmos.color = Color.red;
            if (_points != null && drawPoints) {
                for (int i = 0; i < _points.Count; i++) {
                    Gizmos.DrawSphere(_points[i], 0.2f);
                }
            }

            if (m_edges != null && drawEdges) {
                Gizmos.color = Color.white;
                for (int i = 0; i < m_edges.Count; i++) {
                    Vector2 left = (Vector2)m_edges[i].p0;
                    Vector2 right = (Vector2)m_edges[i].p1;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }

            Gizmos.color = Color.magenta;
            if (m_delaunayTriangulation != null && drawDelaunayTriangulation) {
                for (int i = 0; i < m_delaunayTriangulation.Count; i++) {
                    Vector2 left = (Vector2)m_delaunayTriangulation[i].p0;
                    Vector2 right = (Vector2)m_delaunayTriangulation[i].p1;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }

            if (m_spanningTree != null && drawSpanningTree) {
                Gizmos.color = Color.green;
                for (int i = 0; i < m_spanningTree.Count; i++) {
                    LineSegment seg = m_spanningTree[i];
                    Vector2 left = (Vector2)seg.p0;
                    Vector2 right = (Vector2)seg.p1;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector2(0, 0), new Vector2(0, _mapHeight));
            Gizmos.DrawLine(new Vector2(0, 0), new Vector2(_mapWidth, 0));
            Gizmos.DrawLine(new Vector2(_mapWidth, 0), new Vector2(_mapWidth, _mapHeight));
            Gizmos.DrawLine(new Vector2(0, _mapHeight), new Vector2(_mapWidth, _mapHeight));
        }
    }
}