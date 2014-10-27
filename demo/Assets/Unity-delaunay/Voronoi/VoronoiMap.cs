using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Delaunay;

/** Unity-friendly Output from runnning the ported AS3Delaunay library

"VoronoiNS" is a silly namespace name, but C# is buggy and can't cope with a class and namespace with same
name, and the original port (ab)uses Voronoi as a class-name
  */
namespace VoronoiNS
{
		public class VoronoiMap : MonoBehaviour
		{

				public static VoronoiMap CreateMapFromVoronoiOutput (Voronoi voronoiGenerator)
				{
						GameObject go = new GameObject ("New VoronoiMap");
						VoronoiMap map = go.AddComponent<VoronoiMap> ();

						System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
						watch.Reset ();
						watch.Start ();

						Dictionary<Site,VoronoiCell> generatedCells = new Dictionary<Site, VoronoiCell> ();

						foreach (Edge edge in voronoiGenerator.Edges())
						{
								VoronoiCellEdge vEdge = new VoronoiCellEdge ();
								Site[] bothSites = new Site[] { edge.leftSite, edge.rightSite };
								foreach (Site site in bothSites)
								{
										VoronoiCell newCell = null; // C# is rubbish. Crashes if Dictionary lacks the key. Very bad design.
										if (generatedCells.ContainsKey (site))
												newCell = generatedCells [site];
										GameObject goCell;
					
										if (newCell == null)
										{
												goCell = new GameObject ("Cell-#" + generatedCells.Count);
												goCell.transform.parent = go.transform;
												newCell = goCell.AddComponent<VoronoiCell> ();
												generatedCells.Add (site, newCell);
										}
										else
										{
												goCell = newCell.gameObject;
										}
					
										vEdge.AddCellJoiningIfBothFilled (newCell);
					
										List<Vector2> region = site.Region (voronoiGenerator.plotBounds);
										newCell.orderedPointsOnCircumference = region;
										Debug.Log ("Added region with num points:" + region.Count);
								}
						}
						watch.Stop ();
			
						return map;
				}

				// Use this for initialization
				void Start ()
				{
	
				}
	
				// Update is called once per frame
				void Update ()
				{
	
				}
		}
}