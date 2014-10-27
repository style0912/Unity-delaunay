using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** Unity-friendly Output from runnning the ported AS3Delaunay library

 "VoronoiNS" is a silly namespace name, but C# is buggy and can't cope with a class and namespace with same
name, and the original port (ab)uses Voronoi as a class-name */
namespace VoronoiNS
{
	public class VoronoiCell : MonoBehaviour, ISerializationCallbackReceiver /** workaround for ENORMOUS bugs in Unity Serialization, c.f. http://blogs.unity3d.com/2014/06/24/serialization-in-unity/ */
		{

				/** NOTE: Unity3D's Serialization is fundamentally broken, and CANNOT serialize Dictionary's, no matter what you do */
				public Dictionary<VoronoiCellEdge,VoronoiCell> neighbours;
				
				/** Exists ONLY to workaround Unity bugs in Serialization */
				public List<VoronoiCellEdge> serializedEdgeList;
		/** Exists ONLY to workaround Unity bugs in Serialization */
		public List<VoronoiCell> serializedCellList;
		
		public void OnBeforeSerialize()
		{
		serializedEdgeList = new List<VoronoiCellEdge>();
		serializedCellList = new List<VoronoiCell>();
		
		foreach( KeyValuePair<VoronoiCellEdge,VoronoiCell> item in neighbours )
		{
		serializedEdgeList.Add( item.Key );
		serializedCellList.Add( item.Value );
		}
		}
		
		public void OnAfterDeserialize()
		{
		neighbours = new Dictionary<VoronoiCellEdge, VoronoiCell>();
		for( int i=0; i<serializedEdgeList.Count; i++ )
		{
		neighbours.Add( serializedEdgeList[i], serializedCellList[i]);
		}
		}
		
				public VoronoiCell parent
				{
						get {
								if (gameObject.transform.parent == null)
										return null;
		
								return GetComponentInParent<VoronoiCell> ();
						}
				}
				
				/** AS3Delaunay library automatically gives us this, and its useless for most things, but great when
				trying to make a simple mesh.
				
				 Complex meshes would need to know the original Edge objects, so they can set vertex-colouring,
				 vertex-texture-blending etc (which this CANNOT provide), but we might as well make the simple
				 version easy while we can! */
		public List<Vector2> orderedPointsOnCircumference;
	
				void Start ()
				{
				}
	
				// Update is called once per frame
				void Update ()
				{
	
				}
		}
}
