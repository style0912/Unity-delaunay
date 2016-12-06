using UnityEngine;
using System.Collections;


namespace style0912 {

    /// <summary>
    /// 노드의 모서리 정보를 가지고 있다.
    /// d0, d1은 모서리를 중심으로 좌우 면의 센터.
    /// v0, v1은 모서리의 양 끝의 코너.
    /// river는 흐르는 강인지 아닌지를 나타낸다.
    /// </summary>
    public class Edge {
        public int index;
        public Center d0, d1;  // Delaunay edge
        public Corner v0, v1;  // Voronoi edge
        public Vector2 midpoint;  // halfway between v0,v1
        public int river;  // volume of water, or 0
    }
}
