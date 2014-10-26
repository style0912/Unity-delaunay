using UnityEngine;
using System.Collections;

using UnityEditor;

/** Unity custom inspector lets us add a Button inside the Inspector */
[CustomEditor(typeof(VoronoiPolygonator))]
public class InspectorForPolygonator : Editor {

	
	public override void OnInspectorGUI()
	{
	VoronoiPolygonator vp = target as VoronoiPolygonator;
	
	if( vp.NeedsRegeneration() )
	{
	EditorGUILayout.HelpBox( "No Voronoi diagram present; please create one using the re-generate button below", MessageType.Warning );
	}
	if( GUILayout.Button ( "Re-generate Voronoi") )
	{
			vp.Demo ();
			EditorUtility.SetDirty( target );
	}
		if( GUILayout.Button ( "Clear Voronoi") )
		{
			vp.DeleteAllData();
			EditorUtility.SetDirty( target );
		}
	DrawDefaultInspector();
	}
}
