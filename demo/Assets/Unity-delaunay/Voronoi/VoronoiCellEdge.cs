using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** Unity-friendly Output from runnning the ported AS3Delaunay library

 "VoronoiNS" is a silly namespace name, but C# is buggy and can't cope with a class and namespace with same
name, and the original port (ab)uses Voronoi as a class-name */
namespace VoronoiNS
{
[System.Serializable]
public class VoronoiCellEdge {

public VoronoiCell cell1, cell2;
public Vector2 edgeStart, edgeEnd;

public void AddCellJoiningIfBothFilled( VoronoiCell c )
{
if( cell1 == null )
cell1 = c;
else if( cell2 == null )
cell2 = c;
else
{
Debug.LogError("Cannot add a third cell to an edge!");
}

if( cell1 != null && cell2 != null )
{
/* now we can join the two cells via this edge */
if( cell1.neighbours == null )
					cell1.neighbours = new Dictionary<VoronoiCellEdge, VoronoiCell>();
cell1.neighbours.Add( this, cell2 );

if( cell2.neighbours == null )
					cell2.neighbours = new Dictionary<VoronoiCellEdge, VoronoiCell>();
			cell2.neighbours.Add( this, cell1 );
}
}
}
}