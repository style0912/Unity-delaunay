using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SkyIslandGeneratorCS))]
[System.Serializable]
public class SkyIslandGeneratorEditorCS : Editor {
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();

        SkyIslandGeneratorCS sig = target as SkyIslandGeneratorCS;
        
        sig.seed = EditorGUILayout.TextField("Seed", sig.seed);
        sig.useRandomSeed = GUILayout.Toggle(sig.useRandomSeed, "useRandomSeed");

        //Area size
        sig.areaSize = EditorGUILayout.FloatField("Area size", sig.areaSize);
        //Interval
        sig.interval = EditorGUILayout.FloatField("Interval", sig.interval);
        //Top height
        sig.topHeight = EditorGUILayout.FloatField("Top height", sig.topHeight);
        sig.topExponent = EditorGUILayout.FloatField("Top height exponent", sig.topExponent);
        //Bottom height
        sig.bottomHeight = EditorGUILayout.FloatField("Bottom height", sig.bottomHeight);
        sig.bottomExponent = EditorGUILayout.FloatField("Bottom height exponent", sig.bottomExponent);
        //Material
        sig.material = EditorGUILayout.ObjectField("Material", sig.material, typeof(Material), false) as Material;
        //Shape texture
        sig.shape = EditorGUILayout.ObjectField("Shape", sig.shape, typeof(Texture2D), false) as Texture2D;

        GUILayout.BeginHorizontal();
        {
            //Height maps
            sig.hmapFoldout = EditorGUILayout.Foldout(sig.hmapFoldout, "Height maps");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("add")) {
                if (sig.noiseScale.Count < 10) {
                    sig.noiseScale.Add(1);
                    sig.offset.Add(Vector2.zero);
                    sig.offsetRandom.Add(true);
                    sig.alpha.Add(1);
                    sig.blendMode.Add(0);
                    sig.hmapFoldout = true;
                }
            }
        }
        GUILayout.EndHorizontal();

        if (sig.hmapFoldout) {
            for (var hmaps = 0; hmaps < sig.noiseScale.Count; hmaps++) {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(hmaps + " element");
                    if (GUILayout.Button("remove")) {
                        sig.noiseScale.RemoveAt(hmaps);
                        sig.offset.RemoveAt(hmaps);
                        sig.offsetRandom.RemoveAt(hmaps);
                        sig.alpha.RemoveAt(hmaps);
                        sig.blendMode.RemoveAt(hmaps);
                    }
                }
                GUILayout.EndHorizontal();

                //Perlin noise scale
                GUILayout.BeginHorizontal();
                {
                    sig.noiseScale[hmaps] = EditorGUILayout.FloatField("Perlin noise scale", sig.noiseScale[hmaps]);
                }
                GUILayout.EndHorizontal();

                //Random offset
                GUILayout.BeginHorizontal();
                {
                    sig.offsetRandom[hmaps] = EditorGUILayout.Toggle("Random offset", sig.offsetRandom[hmaps]);
                    if (!sig.offsetRandom[hmaps])
                        sig.offset[hmaps] = EditorGUILayout.Vector2Field("", sig.offset[hmaps]);
                }
                GUILayout.EndHorizontal();
                    

                //Alpha
                sig.alpha[hmaps] = EditorGUILayout.FloatField("Alpha", sig.alpha[hmaps]);

                //Blend mode
                if (hmaps != 0) {
                    string[] blendModeNames = new string[4];
                    blendModeNames[0] = "multiply";
                    blendModeNames[1] = "darken";
                    blendModeNames[2] = "lighten";
                    blendModeNames[3] = "exclusion";
                    sig.blendMode[hmaps] = EditorGUILayout.Popup("Blend mode", sig.blendMode[hmaps], blendModeNames);
                }

                GUILayout.Space(10);
            }
        }
        
        //Colors
        GUILayout.BeginHorizontal();
        {
            sig.colorFoldout = EditorGUILayout.Foldout(sig.colorFoldout, "Colors");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("add")) {
                if (sig.colors.Count < 10) {
                    sig.colors.Add(new Color(1, 1, 1, 1));
                    sig.colorMinHeight.Add(0);
                    sig.colorMaxHeight.Add(1);
                    sig.colorTransValue.Add(1);
                    sig.colorFoldout = true;
                }
            }
        }
        GUILayout.EndHorizontal();

        if (sig.colorFoldout) {
            for (var clrs = 0; clrs < sig.colors.Count; clrs++) {

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(clrs + " element");
                    if (GUILayout.Button("remove")) {
                        sig.colors.RemoveAt(clrs);
                        sig.colorMinHeight.RemoveAt(clrs);
                        sig.colorMaxHeight.RemoveAt(clrs);
                        sig.colorTransValue.RemoveAt(clrs);
                    }
                }
                GUILayout.EndHorizontal();

                //Color
                sig.colors[clrs] = EditorGUILayout.ColorField("Color", sig.colors[clrs]);
                //ColorMinHeight
                sig.colorMinHeight[clrs] = EditorGUILayout.Slider("Min height", sig.colorMinHeight[clrs], 0, 1);
                sig.colorMinHeight[clrs] = Mathf.Min(sig.colorMinHeight[clrs], sig.colorMaxHeight[clrs]);
                //ColorMaxHeight
                sig.colorMaxHeight[clrs] = EditorGUILayout.Slider("Max height", sig.colorMaxHeight[clrs], 0, 1);
                sig.colorMaxHeight[clrs] = Mathf.Max(sig.colorMaxHeight[clrs], sig.colorMinHeight[clrs]);
                //ColorTransValue
                sig.colorTransValue[clrs] = EditorGUILayout.FloatField("Min transition value", sig.colorTransValue[clrs]);

                GUILayout.Space(10);
            }
        }

        sig.borderColor = EditorGUILayout.ColorField("Border color", sig.borderColor);
        sig.bottomColor = EditorGUILayout.ColorField("Bottom color", sig.bottomColor);

        //Objects
        GUILayout.BeginHorizontal();
        {
            sig.objectFoldout = EditorGUILayout.Foldout(sig.objectFoldout, "Objects");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("add")) {
                if (sig.objectPrefab.Count < 10) {
                    sig.objectPrefab.Add(null);
                    sig.objectMinHeight.Add(0);
                    sig.objectMaxHeight.Add(1);
                    sig.objectMaxAngle.Add(50);
                    sig.objectNoiseScale.Add(1);
                    sig.objectOffset.Add(Vector2.zero);
                    sig.objectOffsetRandom.Add(true);
                    sig.objectInterval.Add(1);
                    sig.objectFoldout = true;
                }
            }
        }
        GUILayout.EndHorizontal();

        if (sig.objectFoldout) {
            for (var obj = 0; obj < sig.objectPrefab.Count; obj++) {

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(obj + " element");
                    if (GUILayout.Button("remove")) {
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
                GUILayout.EndHorizontal();

                sig.objectPrefab[obj] = EditorGUILayout.ObjectField("Prefab", sig.objectPrefab[obj], typeof(Transform), true) as Transform;

                //ObjectMinHeight
                sig.objectMinHeight[obj] = EditorGUILayout.Slider("Min height", sig.objectMinHeight[obj], 0, 1);
                sig.objectMinHeight[obj] = Mathf.Min(sig.objectMinHeight[obj], sig.objectMaxHeight[obj]);
                //ObjectMaxHeight
                sig.objectMaxHeight[obj] = EditorGUILayout.Slider("Max height", sig.objectMaxHeight[obj], 0, 1);
                sig.objectMaxHeight[obj] = Mathf.Max(sig.objectMaxHeight[obj], sig.objectMinHeight[obj]);
                //Max angle
                sig.objectMaxAngle[obj] = EditorGUILayout.FloatField("Max angle", sig.objectMaxAngle[obj]);
                //Interval
                sig.objectInterval[obj] = EditorGUILayout.FloatField("Interval", sig.objectInterval[obj]);
                //Perlin noise scale
                sig.objectNoiseScale[obj] = EditorGUILayout.FloatField("Perlin noise scale", sig.objectNoiseScale[obj]);

                //Random offset
                sig.objectOffsetRandom[obj] = EditorGUILayout.Toggle("Random offset", sig.objectOffsetRandom[obj]);
                if (!sig.objectOffsetRandom[obj])
                    sig.objectOffset[obj] = EditorGUILayout.Vector2Field("", sig.objectOffset[obj]);
                //else
                //    EditorGUILayout.LabelField("Random offset");

                GUILayout.Space(10);
            }
        }
        sig.savePath = EditorGUILayout.TextField("Save path", sig.savePath);

        if (GUILayout.Button("ReGenerate")) {
            sig.GenerateIsland();
        }

        if (GUILayout.Button("Generate and save")) {
            sig.GenerateAndSave();
        }

        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
        Undo.RecordObject(target, "ChangedSettings");
    }

    //void ClearAll(SkyIslandGenerator sig) {
    //    sig.noiseScale.Clear();
    //    sig.offset.Clear();
    //    sig.offsetRandom.Clear();
    //    sig.alpha.Clear();
    //    sig.blendMode.Clear();

    //    sig.colors.Clear();
    //    sig.colorMinHeight.Clear();
    //    sig.colorMaxHeight.Clear();
    //    sig.colorTransValue.Clear();

    //    sig.objectPrefab.Clear();
    //    sig.objectMinHeight.Clear();
    //    sig.objectMaxHeight.Clear();
    //    sig.objectMaxAngle.Clear();
    //    sig.objectNoiseScale.Clear();
    //    sig.objectOffset.Clear();
    //    sig.objectOffsetRandom.Clear();
    //    sig.objectInterval.Clear();
    //}
}
