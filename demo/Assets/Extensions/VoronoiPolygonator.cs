using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;

public class VoronoiPolygonator : MonoBehaviour {
	
	private int
		m_pointCount = 20;
	
	private List<Vector2> m_points;
	private float m_mapWidth = 20;
	private float m_mapHeight = 10;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	
	void Awake ()
	{
		Demo ();
	}
	
	void Update ()
	{
		if (Input.anyKeyDown) {
			Demo ();
		}
	}
	
	private Delaunay.Voronoi v;
	private void Demo ()
	{
		
		List<uint> colors = new List<uint> ();
		m_points = new List<Vector2> ();
		
		for (int i = 0; i < m_pointCount; i++) {
			colors.Add (0);
			m_points.Add (new Vector2 (
				UnityEngine.Random.Range (0, m_mapWidth),
				UnityEngine.Random.Range (0, m_mapHeight))
			              );
		}
		v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, m_mapWidth, m_mapHeight));
		m_edges = v.VoronoiDiagram ();
		
		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();
	}
	
	public bool shouldDrawBounds = false;
	public bool shouldDrawAllEdges = false;
	public bool shouldDrawDelaunayTriangulation = false;
	public bool shouldDrawVoronoiPolygons = true;
	public bool shouldDrawVoronoiLinesToCenter = true;
	public bool shouldDrawSpanningTree  = false;
	void OnDrawGizmos ()
	{
		Gizmos.color = Color.red;
		if (m_points != null) {
			for (int i = 0; i < m_points.Count; i++) {
				Gizmos.DrawSphere (m_points [i], 0.2f);
			}
		}
		
		if( shouldDrawAllEdges)
		{
		if (m_edges != null) {
			Gizmos.color = Color.white;
			for (int i = 0; i< m_edges.Count; i++) {
				Vector2 left = (Vector2)m_edges [i].p0;
				Vector2 right = (Vector2)m_edges [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}
		}
		
		if( shouldDrawDelaunayTriangulation )
		{
		Gizmos.color = Color.magenta;
		if (m_delaunayTriangulation != null) {
			for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
				Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
				Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}
		}
		
		if( shouldDrawSpanningTree )
		{
		if (m_spanningTree != null) {
			Gizmos.color = Color.green;
			for (int i = 0; i< m_spanningTree.Count; i++) {
				LineSegment seg = m_spanningTree [i];				
				Vector2 left = (Vector2)seg.p0;
				Vector2 right = (Vector2)seg.p1;
				Gizmos.DrawLine ((Vector3)left, (Vector3)right);
			}
		}
		}
		
		if( shouldDrawVoronoiPolygons )
		{
		/** ADAM: note, the SiteCoords are identical to the raw Points array you passed-in when creating the Voronoi object */
		List<Vector2> ses = v.SiteCoords();
		foreach( Vector2 siteCoord in ses )
		{
			Gizmos.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
			
		List<LineSegment> outlineOfSite = v.VoronoiBoundaryForSite(  siteCoord );
		foreach( LineSegment seg in outlineOfSite )
		{
		Gizmos.DrawLine( (Vector2)seg.p0, (Vector2)seg.p1 );
		
		}
		
				if( shouldDrawVoronoiLinesToCenter )
				{
				foreach( LineSegment seg in outlineOfSite )
				{
						Gizmos.color = Color.gray;
						Gizmos.DrawLine( (Vector2)seg.p0, siteCoord );
					}
				}
		}
		}
		
		if( shouldDrawBounds )
		{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
		Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
		Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
		}
	}
}
