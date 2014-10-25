using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;

/**
Upgrade of VoronoiDemo class, with a clearer name, and some better
interactivity.

 */
public class VoronoiPolygonator : MonoBehaviour {
	
	private int
		m_pointCount = 20;
	
	private List<Vector2> m_points;
	private float m_mapWidth = 20;
	private float m_mapHeight = 10;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	
	private Delaunay.Voronoi v;
	public void Demo () /** Public so that the custom Inspector class can call it */
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
	
	public bool NeedsRegeneration()
	{
	return v == null;
	}
	
	public bool shouldDrawBounds = true;
	public bool shouldDrawAllEdges = true;
	public bool shouldDrawDelaunayTriangulation = true;
	public bool shouldDrawVoronoiPolygons = true;
	public bool shouldDrawVoronoiLinesToCenter = false;
	public bool shouldDrawSpanningTree  = false;
	public bool randomizeVoronoiColours = true;
	void OnDrawGizmos ()
	{
	if( v == null )
	return;
	
		Gizmos.color = Color.red;
		if (m_points != null) {
			for (int i = 0; i < m_points.Count; i++) {
				Gizmos.DrawSphere (m_points [i], 0.2f);
			}
		}
		
		if( shouldDrawAllEdges)
		{
		if (m_edges != null) {
			Gizmos.color = Color.gray;
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
		/** Note, the SiteCoords are identical to the raw Points array you passed-in when creating the Voronoi object,
		I think. So ... you could safely re-use that here instead of fetching it from the Voronoi object (maybe; could be
		some filteing happening? Dupes removed, etc?) */
		List<Vector2> ses = v.SiteCoords();
		foreach( Vector2 siteCoord in ses )
		{
		if( randomizeVoronoiColours ) /** Note: the Edges display above (in Gray) shows unconnected edges,
		but this section re-uses the actual semi-polygons created automatically by the Voronoi algorithm. To prove
		this you can optionally turn on colourization of the edges, so that that shared edges will show with same colours
		*/
			Gizmos.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
			else
			Gizmos.color = Color.white;
			
			/** NB: this is the reason we had to change VoronoiDemo class and save the Voronoi object: the boundaries
			are the Voronoi polygons, the most precious thing from the algorithm, but aren't directly saved when you
			run the algorithm
			*/
		List<LineSegment> outlineOfSite = v.VoronoiBoundaryForSite(  siteCoord );
		foreach( LineSegment seg in outlineOfSite )
		{
		Vector2 s = (Vector2) seg.p0;
		Vector2 e = (Vector2) seg.p1;
		
		if( randomizeVoronoiColours )
		{
		/** To make them easier to see, shift the vectors SLIGHTLY towards the center point.
		
		 This lets you see EXACTLY what poly / partial poly the algorithm is giving us "for free",
		 so that triangulating it will be easy in your own projects */
		s += (siteCoord - s) * 0.05f;
		e += (siteCoord - e) * 0.05f;
		}
		Gizmos.DrawLine( s, e );
		
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
