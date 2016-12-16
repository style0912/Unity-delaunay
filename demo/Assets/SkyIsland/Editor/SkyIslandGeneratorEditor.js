#pragma strict

@CustomEditor(SkyIslandGenerator)

@System.Serializable
public class SkyIslandGeneratorEditor extends Editor{

	function OnInspectorGUI(){
		var sig : SkyIslandGenerator = target as SkyIslandGenerator;
		/*if(GUILayout.Button("ClearAll")){
			ClearAll(sig);
		}*/
		var width = 0.0;
		var rt : Rect;
		GUILayoutUtility.GetRect(0, 2);
		GUILayoutUtility.GetRect(0, 162);
		rt = GUILayoutUtility.GetLastRect();
		width = rt.width;
		var boxWidth = width-9;
		//Area size
		sig.areaSize = EditorGUI.FloatField(Rect(14, rt.y, width, 16), "Area size", sig.areaSize);
		//Interval
		sig.interval = EditorGUI.FloatField(Rect(14, rt.y+18, width, 16), "Interval", sig.interval);
		//Top height
		sig.topHeight = EditorGUI.FloatField(Rect(14, rt.y+36, width, 16), "Top height", sig.topHeight);
		sig.topExponent = EditorGUI.FloatField(Rect(14, rt.y+54, width, 16), "Top height exponent", sig.topExponent);
		//Bottom height
		sig.bottomHeight = EditorGUI.FloatField(Rect(14, rt.y+72, width, 16), "Bottom height", sig.bottomHeight);
		sig.bottomExponent = EditorGUI.FloatField(Rect(14, rt.y+90, width, 16), "Bottom height exponent", sig.bottomExponent);
		//Material
		sig.material = EditorGUI.ObjectField(Rect(14, rt.y+108, width, 16), "Material", sig.material, Material, false) as Material;
		//Shape texture
		sig.shape = EditorGUI.ObjectField(Rect(14, rt.y+126, width, 16), "Shape", sig.shape, Texture2D, false) as Texture2D;
		
		//Height maps
		sig.hmapFoldout = EditorGUI.Foldout(Rect(14, rt.y+144, width/2, 16), sig.hmapFoldout, "Height maps");
		if(GUI.Button(Rect(14+width-40, rt.y+144, 40, 16), "add")){
			if(sig.noiseScale.Count < 10){
				sig.noiseScale.Add(1);
				sig.offset.Add(Vector2.zero);
				sig.offsetRandom.Add(true);
				sig.alpha.Add(1);
				sig.blendMode.Add(0);
			}
		}
		if(sig.hmapFoldout){
			for(var hmaps = 0; hmaps < sig.noiseScale.Count; hmaps++){
				GUILayoutUtility.GetRect(0, 100);
				rt = GUILayoutUtility.GetLastRect();
				GUI.Box(Rect(14, rt.y, width, 96), "");
				rt.y = GUILayoutUtility.GetLastRect().y + 3;
				
				//Perlin noise scale
				sig.noiseScale[hmaps] = EditorGUI.FloatField(Rect(18, rt.y+18, boxWidth, 16), "Perlin noise scale", sig.noiseScale[hmaps]);
				//Random offset
				sig.offsetRandom[hmaps] = EditorGUI.Toggle(Rect(18, rt.y+36, 16, 16), "", sig.offsetRandom[hmaps]);
				if(!sig.offsetRandom[hmaps])
					sig.offset[hmaps] = EditorGUI.Vector2Field(Rect(34, rt.y+36, boxWidth-16, 16), "", sig.offset[hmaps]);
				else
					EditorGUI.LabelField(Rect(34, rt.y+36, boxWidth-16, 16), "Random offset");
				
				//Alpha
				sig.alpha[hmaps] = EditorGUI.FloatField(Rect(18, rt.y+54, boxWidth, 16), "Alpha", sig.alpha[hmaps]);
				
				//Blend mode
				if(hmaps != 0){
					var blendModeNames : String[] = new String[4];
						blendModeNames[0] = "multiply";
						blendModeNames[1] = "darken";
						blendModeNames[2] = "lighten";
						blendModeNames[3] = "exclusion";
					sig.blendMode[hmaps] = EditorGUI.Popup(Rect(18, rt.y+72, boxWidth, 16), "Blend mode", sig.blendMode[hmaps], blendModeNames);
				}
				
				EditorGUI.LabelField(Rect(18, rt.y, boxWidth/2, 16), hmaps +" element");
				if(GUI.Button(Rect(14+width-65, rt.y, 60, 16), "remove")){
					sig.noiseScale.RemoveAt(hmaps);
					sig.offset.RemoveAt(hmaps);
					sig.offsetRandom.RemoveAt(hmaps);
					sig.alpha.RemoveAt(hmaps);
					sig.blendMode.RemoveAt(hmaps);
				}
			}
		}

		GUILayoutUtility.GetRect(0, 18);
		rt = GUILayoutUtility.GetLastRect();
		width = rt.width;
		
		//Colors
		sig.colorFoldout = EditorGUI.Foldout(Rect(14, rt.y, width/2, 16), sig.colorFoldout, "Colors");
		if(GUI.Button(Rect(14+width-40, rt.y, 40, 16), "add")){
			if(sig.colors.Count < 10){
				sig.colors.Add(Color(1,1,1,1));
				sig.colorMinHeight.Add(0);
				sig.colorMaxHeight.Add(1);
				sig.colorTransValue.Add(1);
			}
		}
		
		if(sig.colorFoldout){
			for(var clrs = 0; clrs < sig.colors.Count; clrs++){
				GUILayoutUtility.GetRect(0, 100);
				rt = GUILayoutUtility.GetLastRect();
				GUI.Box(Rect(14, rt.y, width, 96), "");
				rt.y = GUILayoutUtility.GetLastRect().y + 3;
				
				//Color
				sig.colors[clrs] = EditorGUI.ColorField(Rect(18, rt.y+18, boxWidth, 16), "Color", sig.colors[clrs]);
				//ColorMinHeight
				sig.colorMinHeight[clrs] = EditorGUI.Slider(Rect(18, rt.y+36, boxWidth, 16), "Min height", sig.colorMinHeight[clrs], 0, 1);
				sig.colorMinHeight[clrs] = Mathf.Min(sig.colorMinHeight[clrs], sig.colorMaxHeight[clrs]);
				//ColorMaxHeight
				sig.colorMaxHeight[clrs] = EditorGUI.Slider(Rect(18, rt.y+54, boxWidth, 16), "Max height", sig.colorMaxHeight[clrs], 0, 1);
				sig.colorMaxHeight[clrs] = Mathf.Max(sig.colorMaxHeight[clrs], sig.colorMinHeight[clrs]);
				//ColorTransValue
				sig.colorTransValue[clrs] = EditorGUI.FloatField(Rect(18, rt.y+72, boxWidth, 16), "Min transition value", sig.colorTransValue[clrs]);
				
				EditorGUI.LabelField(Rect(18, rt.y, boxWidth/2, 16), clrs +" element");
				if(GUI.Button(Rect(14+width-65, rt.y, 60, 16), "remove")){
					sig.colors.RemoveAt(clrs);
					sig.colorMinHeight.RemoveAt(clrs);
					sig.colorMaxHeight.RemoveAt(clrs);
					sig.colorTransValue.RemoveAt(clrs);
				}
			}
		}
		GUILayoutUtility.GetRect(0, 54);
		rt = GUILayoutUtility.GetLastRect();
		width = rt.width;
		
		sig.borderColor = EditorGUI.ColorField(Rect(14, rt.y, width, 16), "Border color", sig.borderColor);
		sig.bottomColor = EditorGUI.ColorField(Rect(14, rt.y+18, width, 16), "Bottom color", sig.bottomColor);
		
		//Objects
		sig.objectFoldout = EditorGUI.Foldout(Rect(14, rt.y+36, width/2, 16), sig.objectFoldout, "Objects");
		if(GUI.Button(Rect(14+width-40, rt.y+36, 40, 16), "add")){
			if(sig.objectPrefab.Count < 10){
				sig.objectPrefab.Add(null);
				sig.objectMinHeight.Add(0);
				sig.objectMaxHeight.Add(1);
				sig.objectMaxAngle.Add(50);
				sig.objectNoiseScale.Add(1);
				sig.objectOffset.Add(Vector2.zero);
				sig.objectOffsetRandom.Add(true);
				sig.objectInterval.Add(1);
			}
		}
		
		if(sig.objectFoldout){
			for(var obj = 0; obj < sig.objectPrefab.Count; obj++){
				GUILayoutUtility.GetRect(0, 154);
				rt = GUILayoutUtility.GetLastRect();
				GUI.Box(Rect(14, rt.y, width, 150), "");
				rt.y = GUILayoutUtility.GetLastRect().y + 3;
				sig.objectPrefab[obj] = EditorGUI.ObjectField(Rect(18, rt.y+18, boxWidth, 16), "Prefab", sig.objectPrefab[obj], Transform, true) as Transform;
				
				//ObjectMinHeight
				sig.objectMinHeight[obj] = EditorGUI.Slider(Rect(18, rt.y+36, boxWidth, 16), "Min height", sig.objectMinHeight[obj], 0, 1);
				sig.objectMinHeight[obj] = Mathf.Min(sig.objectMinHeight[obj], sig.objectMaxHeight[obj]);
				//ObjectMaxHeight
				sig.objectMaxHeight[obj] = EditorGUI.Slider(Rect(18, rt.y+54, boxWidth, 16), "Max height", sig.objectMaxHeight[obj], 0, 1);
				sig.objectMaxHeight[obj] = Mathf.Max(sig.objectMaxHeight[obj], sig.objectMinHeight[obj]);
				//Max angle
				sig.objectMaxAngle[obj] = EditorGUI.FloatField(Rect(18, rt.y+72, boxWidth, 16), "Max angle", sig.objectMaxAngle[obj]);
				//Interval
				sig.objectInterval[obj] = EditorGUI.FloatField(Rect(18, rt.y+90, boxWidth, 16), "Interval", sig.objectInterval[obj]);
				//Perlin noise scale
				sig.objectNoiseScale[obj] = EditorGUI.FloatField(Rect(18, rt.y+108, boxWidth, 16), "Perlin noise scale", sig.objectNoiseScale[obj]);
				
				//Random offset
				sig.objectOffsetRandom[obj] = EditorGUI.Toggle(Rect(18, rt.y+126, 16, 16), "", sig.objectOffsetRandom[obj]);
				if(!sig.objectOffsetRandom[obj])
					sig.objectOffset[obj] = EditorGUI.Vector2Field(Rect(34, rt.y+126, boxWidth-16, 16), "", sig.objectOffset[obj]);
				else
					EditorGUI.LabelField(Rect(34, rt.y+126, boxWidth-16, 16), "Random offset");
				
				EditorGUI.LabelField(Rect(18, rt.y, boxWidth/2, 16), obj +" element");
				if(GUI.Button(Rect(14+width-65, rt.y, 60, 16), "remove")){
					sig.objectPrefab.RemoveAt(obj);
					sig.objectMinHeight.RemoveAt(obj);
					sig.objectMaxHeight.RemoveAt(obj);
					sig.objectMaxAngle.RemoveAt(obj);
					sig.objectNoiseScale.RemoveAt(obj);
					sig.objectOffset.RemoveAt(obj);
					sig.objectOffsetRandom.RemoveAt(obj);
					sig.objectInterval.RemoveAt(obj);
				}
			}
		}
		sig.savePath = EditorGUILayout.TextField("Save path", sig.savePath);
		if(GUILayout.Button("Generate and save"))
        {
            sig.GenerateAndSave();
        }
		
		if(GUI.changed){
			EditorUtility.SetDirty(target);
		}
		Undo.RecordObject(target, "ChangedSettings");
	}
	
	function ClearAll(sig : SkyIslandGenerator){
		sig.noiseScale.Clear();
		sig.offset.Clear();
		sig.offsetRandom.Clear();
		sig.alpha.Clear();
		sig.blendMode.Clear();
		
		sig.colors.Clear();
		sig.colorMinHeight.Clear();
		sig.colorMaxHeight.Clear();
		sig.colorTransValue.Clear();
		
		sig.objectPrefab.Clear();
		sig.objectMinHeight.Clear();
		sig.objectMaxHeight.Clear();
		sig.objectMaxAngle.Clear();
		sig.objectNoiseScale.Clear();
		sig.objectOffset.Clear();
		sig.objectOffsetRandom.Clear();
		sig.objectInterval.Clear();
	}
}