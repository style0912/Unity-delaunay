using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;

using VoronoiNS;

/**
Upgrade of VoronoiDemo class, with a clearer name, and some better
interactivity.

 */
public class VoronoiPolygonator : MonoBehaviour
{
	
		public int numSitesToGenerate = 20;
		private List<Vector2> m_points;
		private float m_mapWidth = 20;
		private float m_mapHeight = 10;
		private List<LineSegment> m_edges = null;
		private List<LineSegment> m_spanningTree;
		private List<LineSegment> m_delaunayTriangulation;
		private Delaunay.Voronoi v;

/** Useful when experimenting and you create too much for Gizmos to render, and CPU goes nuts and dies */
		public void DeleteAllData ()
		{
				v = null;
		}

		public void Demo () /** Public so that the custom Inspector class can call it */
		{
		
				List<uint> colors = new List<uint> ();
				m_points = new List<Vector2> ();
		
				for (int i = 0; i < numSitesToGenerate; i++) {
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
				
				Debug.Log ("Created a Voronoi object. But for Unity, it's recommend you convert it to a VoronoiMap (data structure using Unity GameObjects and MonoBehaviours)");
				//Example:
                VoronoiDiagram map = VoronoiDiagram.CreateDiagramFromVoronoiOutput( v, true );
		}
	
		public bool NeedsRegeneration ()
		{
				return v == null;
		}
	
		public bool DrawBounds = false;
		public bool DrawAllEdges = false;
		public bool DrawDelaunayTriangulation = false;
		public bool DrawManualVoronoiPolygons = false; // NOTE: this is super-slow, use DrawVoronoiRegions instead
		public bool DrawManualVoronoiLinesToCenter = false;
		public bool DrawSpanningTree = false;
		public bool randomizeVoronoiColours = true;
		public bool CloseExternalVoronoPolys = true;
		public bool DrawVoronoiRegions = true;

		void OnDrawGizmos ()
		{
				if (v == null)
						return;
	
				Gizmos.color = Color.red;
				if (m_points != null) {
						for (int i = 0; i < m_points.Count; i++) {
								Gizmos.DrawSphere (m_points [i], 0.2f);
						}
				}
		
				if (DrawAllEdges) {
						if (m_edges != null) {
								Gizmos.color = Color.gray;
								for (int i = 0; i< m_edges.Count; i++) {
										Vector2 left = (Vector2)m_edges [i].p0;
										Vector2 right = (Vector2)m_edges [i].p1;
										Gizmos.DrawLine ((Vector3)left, (Vector3)right);
								}
						}
				}
		
				if (DrawDelaunayTriangulation) {
						Gizmos.color = Color.magenta;
						if (m_delaunayTriangulation != null) {
								for (int i = 0; i< m_delaunayTriangulation.Count; i++) {
										Vector2 left = (Vector2)m_delaunayTriangulation [i].p0;
										Vector2 right = (Vector2)m_delaunayTriangulation [i].p1;
										Gizmos.DrawLine ((Vector3)left, (Vector3)right);
								}
						}
				}
		
				if (DrawSpanningTree) {
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
		
				/** This is the correct source of Voronoi polygons: the "Regions" data from the Voronoi object.
		Note that the list is points, not lines, and you usually have to manually CLOSE the final point to the first */
				if (DrawVoronoiRegions) {
						Debug.Log ("Found " + v.Regions ().Count + " regions to draw");
						foreach (List<Vector2> region in v.Regions()) {
								if (randomizeVoronoiColours) /** Note: the Edges display above (in Gray) shows unconnected edges,
		but this section re-uses the actual semi-polygons created automatically by the Voronoi algorithm. To prove
		this you can optionally turn on colourization of the edges, so that that shared edges will show with same colours
		*/
										Gizmos.color = new Color (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f));
								else
										Gizmos.color = Color.white;
				
								for (int i = 0; i+1<region.Count; i++) {
										Vector2 s = (Vector2)region [i];
										Vector2 e = (Vector2)region [i + 1];
					
										if (randomizeVoronoiColours) {
												/** To make them easier to see, shift the vectors SLIGHTLY towards the center point.
		
		REMoVED: WE CANT DO THAT WHEN USING THE REGIONS SHORTCUT, REGIONS DELETE THEIR POINTS, sadly :(
		
		 This lets you see EXACTLY what poly / partial poly the algorithm is giving us "for free",
		 so that triangulating it will be easy in your own projects */
												//	s += (siteCoord - s) * 0.05f;
												//		e += (siteCoord - e) * 0.05f;
										}
										Gizmos.DrawLine (s, e);
					
								}
				
								if (CloseExternalVoronoPolys) {
										Gizmos.DrawLine ((Vector2)region [region.Count - 1], (Vector2)region [0]);
								}
						}
				}
		
				/** This is the INcorrect way of getting polygons out; but it has an advantage: the Voronoi class deletes
		the Site at center of a Region when giving us Regions (bug: I'd like to fix that and have it return a data
		structure that includes the Site!).
		
		In the meantime, here's how to manually generate the polys by re-using the Boundaries code from Voronoi,
		much easier than trying to manually generate from raw edges.
		
		But ideally: use DrawVoronoiRegions instead
		*/
				if (DrawManualVoronoiPolygons) {
						/** Note, the SiteCoords are identical to the raw Points array you passed-in when creating the Voronoi object,
		I think. So ... you could safely re-use that here instead of fetching it from the Voronoi object (maybe; could be
		some filteing happening? Dupes removed, etc?) */
						List<Vector2> ses = m_points;// v.SiteCoords ();
						foreach (Vector2 siteCoord in ses) {
								if (randomizeVoronoiColours) /** Note: the Edges display above (in Gray) shows unconnected edges,
		but this section re-uses the actual semi-polygons created automatically by the Voronoi algorithm. To prove
		this you can optionally turn on colourization of the edges, so that that shared edges will show with same colours
		*/
										Gizmos.color = new Color (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f));
								else
										Gizmos.color = Color.white;
			
								/** NB: this is the reason we had to change VoronoiDemo class and save the Voronoi object: the boundaries
			are the Voronoi polygons, the most precious thing from the algorithm. They are saved as Regions data structure
			when you run the algorithm - but that data structure throws-away the Site/Point that generates each Region.
			*/
								List<LineSegment> outlineOfSite = v.VoronoiBoundaryForSite (siteCoord);
								List<Vector2> pointsOnPolygonOutline = null;
								if (CloseExternalVoronoPolys)
										pointsOnPolygonOutline = new List<Vector2> ();
								foreach (LineSegment seg in outlineOfSite) {
										Vector2 s = (Vector2)seg.p0;
										Vector2 e = (Vector2)seg.p1;
		
										if (randomizeVoronoiColours) {
												/** To make them easier to see, shift the vectors SLIGHTLY towards the center point.
		
		 This lets you see EXACTLY what poly / partial poly the algorithm is giving us "for free",
		 so that triangulating it will be easy in your own projects */
												s += (siteCoord - s) * 0.05f;
												e += (siteCoord - e) * 0.05f;
										}
										Gizmos.DrawLine (s, e);
		
										if (CloseExternalVoronoPolys) {
												pointsOnPolygonOutline.Add (s);
												pointsOnPolygonOutline.Add (e);
										}
								}
		
								if (CloseExternalVoronoPolys) {
										List<Vector2> unduplicatedPoints = new List<Vector2> ();
				
										//Debug.Log( "Closing outline; "+pointsOnPolygonOutline.Count+" points on outline, with "+outlineOfSite.Count+" lines between them");
										foreach (Vector2 point in pointsOnPolygonOutline) {
												Vector2 dupe;
												if ((dupe = ListContainsVectorCloseToVector (unduplicatedPoints, point)) != Vector2.zero) {
														//Debug.Log( " - point: "+point);
														unduplicatedPoints.Remove (dupe);
												} else {
														//Debug.Log( " + point: "+point);
														unduplicatedPoints.Add (point);
												}
										}
				
										if (unduplicatedPoints.Count == 2) {
												// two points that need connecting
												Gizmos.DrawLine (unduplicatedPoints [0], unduplicatedPoints [1]);
										} else if (unduplicatedPoints.Count > 1)
												Debug.LogError ("Should only have 0 or 2 unconnected points in a single polygon; had: " + unduplicatedPoints.Count);
								}
		
								if (DrawManualVoronoiLinesToCenter) {
										foreach (LineSegment seg in outlineOfSite) {
												Gizmos.color = Color.gray;
												Gizmos.DrawLine ((Vector2)seg.p0, siteCoord);
										}
								}
						}
				}
		
				if (DrawBounds) {
						Gizmos.color = Color.yellow;
						Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (0, m_mapHeight));
						Gizmos.DrawLine (new Vector2 (0, 0), new Vector2 (m_mapWidth, 0));
						Gizmos.DrawLine (new Vector2 (m_mapWidth, 0), new Vector2 (m_mapWidth, m_mapHeight));
						Gizmos.DrawLine (new Vector2 (0, m_mapHeight), new Vector2 (m_mapWidth, m_mapHeight));
				}
		}
	
		/** Returns the dupe item in list so you can remove it; Voronoi library is abusing Vector2 class and returning non-equivalent equivalent objects */
		private Vector2 ListContainsVectorCloseToVector (List<Vector2> l, Vector2 p)
		{
				foreach (Vector2 v in l) {
						if (DoesPointEqualPointEpsilon (v, p))
								return v;
				}
				return Vector2.zero;
		}

		private bool DoesPointEqualPointEpsilon (Vector2 p1, Vector2 p2, float eps = 0.0001f)
		{
				if (Mathf.Abs (p1.x - p2.x) > eps)
						return false;
				if (Mathf.Abs (p1.y - p2.y) > eps)
						return false;
				return true;
		}
}
