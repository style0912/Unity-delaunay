#pragma strict
import System.Collections.Generic;
@script RequireComponent(MeshFilter)
@script RequireComponent(MeshRenderer)
@script RequireComponent(MeshCollider)

//island dimensions
var areaSize = 100.0;
//the distance between the vertices
var interval = 1.0;
//the height of top of the island
var topHeight = 1.0;
var topExponent = 1.0;
//the height of bottom of the island
var bottomHeight = 1.0;
var bottomExponent = 1.0;

var material : Material;
//shape of the island
var shape : Texture2D;

//Height maps. Maximum 10 layers
//Scale of Perlin noise. From it depends density of the mountains. The higher the value the denser the mountains.
var noiseScale : List.<float> = new List.<float>();
//Offset of Perlin noise on coordinate. You can set random value.
var offset : List.<Vector2> = new List.<Vector2>();
var offsetRandom : List.<boolean> = new List.<boolean>();
//The influence of this layer on the previous ones.
var alpha : List.<float> = new List.<float>();
//Blend mode of height map layers(multiply, darken, lighten, exclusion).
var blendMode : List.<int> = new List.<int>();
//for foldout in editor
var hmapFoldout : boolean;

//Colors. Maximum 10 layers
//Color of this layer.
var colors : List.<Color> = new List.<Color>();
//The minimum height to which can be given color.
var colorMinHeight : List.<float> = new List.<float>();
//The maximum height of which can be given color.
var colorMaxHeight : List.<float> = new List.<float>();
//The minimum value of the transition. The higher the value, the greater must be the angle at which the surface can be given color.
//It is intended to determine the vertical surfaces.
var colorTransValue : List.<float> = new List.<float>();
//for foldout in editor
var colorFoldout : boolean;

//The color of the boundary surfaces of the island.
var borderColor : Color;
//position on UV, bound to the variable "colorsOnUv"
var borderColorUv = 0;
//Color of bottom of the island.
var bottomColor : Color;
//position on UV, bound to the variable "colorsOnUv"
var bottomColorUv = 0;

//Create objects. Maximum 10 layers
//Object prefab.
var objectPrefab : List.<Transform> = new List.<Transform>();
//The minimum height at which the object may be.
var objectMinHeight : List.<float> = new List.<float>();
//The maximum height at which the object may be.
var objectMaxHeight : List.<float> = new List.<float>();
//The maximum angle of the surface on which the object can be located.
var objectMaxAngle : List.<float> = new List.<float>();
//Scale of Perlin noise. It determines what objects are in what areas of the island.
var objectNoiseScale : List.<float> = new List.<float>();
//Offset of Perlin noise on coordinate. You can set random value.
var objectOffset : List.<Vector2> = new List.<Vector2>();
var objectOffsetRandom : List.<boolean> = new List.<boolean>();
//Distance between objects.
var objectInterval : List.<float> = new List.<float>();
//for foldout in editor
var objectFoldout : boolean;
//generated mesh and prefab save path
var savePath : String = "Assets/SkyIsland/IslandPrefabs/NewIsland";

//4x4 texture that has all the colors that you specified
private var texture : Texture2D;
//automatically calculated coordinates on UV
private var colorsOnUv : Vector2[] = new Vector2[16];

//array of heights
private var heights : float[];
private var mesh : Mesh;
private var btmMesh : Mesh;
private var filter : MeshFilter;
private var meshRenderer : MeshRenderer;

//fixed size of the island. Calculated from size of the island and interval between vertices.
private var size : Vector2;

//array of boundary vertices
private var extremeVertices : List.<int> = new List.<int>();

private var vertices : List.<Vector3> = new List.<Vector3>();
private var normals : List.<Vector3> = new List.<Vector3>();
private var uv : List.<Vector2> = new List.<Vector2>();
private var triangles : List.<int> = new List.<int>();

function Start(){
	GenerateIsland();
}

function GenerateAndSave(){
	#if UNITY_EDITOR
	if(savePath.Length>0){
		GenerateIsland();
		AssetDatabase.CreateAsset(mesh, savePath +"/" +gameObject.name +"_mesh.asset");
		AssetDatabase.CreateAsset(btmMesh, savePath +"/" +gameObject.name +"_bottomMesh.asset");
		AssetDatabase.CreateAsset(texture, savePath +"/" +gameObject.name +"_colors.asset");

		var prefab = EditorUtility.CreateEmptyPrefab(savePath +"/" +gameObject.name +".prefab");
		EditorUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ReplaceNameBased);
		
		Debug.Log("<color=blue>Saved to: </color>" +savePath);
	}else{
		if(savePath.Length == 0)
			Debug.Log("<color=red>'Save path' is empty\nExample: Assets/SkyIsland/IslandPrefabs/NewIsland</color>");
	}
	#endif
}

function ClearIsland(){
	heights = new float[size.x * size.y];
	mesh = new Mesh();
	extremeVertices = new List.<int>();
	vertices = new List.<Vector3>();
	normals = new List.<Vector3>();
	uv = new List.<Vector2>();
	triangles = new List.<int>();
	
	//Created objects become children of the island. They need to be destroyed when re-creating island
	for(var i = transform.childCount-1; i>=0; --i){
		var child = transform.GetChild(i).gameObject;
		DestroyImmediate(child);
	}
}

function GenerateIsland(){
	filter = GetComponent(MeshFilter);
	meshRenderer = gameObject.GetComponent(MeshRenderer);
    //Height maps. If offset of Perlin noise is random.
	Random.seed = 0;
	for(var offRnd = 0; offRnd < offsetRandom.Count; offRnd++){
		if(offsetRandom[offRnd]){
			offset[offRnd] = Vector2(Random.Range(-1000000.0, 1000000.0), Random.Range(-1000000.0, 1000000.0));
		}
	}
	//Objects. If offset of Perlin noise is random.
	for(var objOffRnd = 0; objOffRnd < objectOffsetRandom.Count; objOffRnd++){
		if(objectOffsetRandom[objOffRnd]){
			objectOffset[objOffRnd] = Vector2(Random.Range(-1000000.0, 1000000.0), Random.Range(-1000000.0, 1000000.0));
		}
	}
	
	//fixed size of the island. Calculated from size of the island and interval between vertices.
	size = Vector2(Mathf.Round(areaSize/interval), Mathf.Round(areaSize/interval));
	
	
	ClearIsland();
	//Generate height maps
	CalcNoise();
	
	//Create Top
	for(var zt = 0; zt < size.y-1; zt++){
		for(var xt = 0; xt < size.x-1; xt++){
			//array of numbers which will take the form "0000"
			var biQuad = new int[4];
			
			//taken 4 vertices
			var vertA = zt * size.x + xt;
			var vertB = (zt+1) * size.x + xt;
			var vertC = (zt+1) * size.x + xt+1;
			var vertD = zt * size.x + xt+1;
			
			//if height of vertices > 0, then character of "biQuad" equals 1. "1111" or "0101" etc.
			if(heights[vertA] > 0) biQuad[0] = 1;
			if(heights[vertB] > 0) biQuad[1] = 1;
			if(heights[vertC] > 0) biQuad[2] = 1;
			if(heights[vertD] > 0) biQuad[3] = 1;
			
			MakeTriangles(vertA, vertB, vertC, vertD,"" +biQuad[0] +biQuad[1] +biQuad[2] +biQuad[3]);
		}
	}
	
	//bottom part of island is another object
	CopyToBottom();

	//assigned to the mesh
	mesh.vertices = vertices.ToArray();
	mesh.normals = normals.ToArray();
	mesh.uv = uv.ToArray();
	mesh.triangles = triangles.ToArray();
	
	filter.sharedMesh = mesh;
	meshRenderer.material = material;
	mesh.RecalculateNormals();
	mesh.RecalculateBounds();
	//create collider
    GetComponent(MeshCollider).sharedMesh = mesh;
	//set texture
	meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
	
	//add objects
	CreateObject();
}

function CopyToBottom(){
	//create empty game object
	var bottom = new GameObject();
	bottom.transform.position = transform.position;
	bottom.name = "Bottom";
	
	//add scripts
	btmMesh = new Mesh();
	var btmfilter = bottom.AddComponent(MeshFilter);
	var btmMeshRenderer = bottom.AddComponent(MeshRenderer);
	var btmMeshCollider = bottom.AddComponent(MeshCollider);
	
	var btmExtremeVertices : List.<int> = new List.<int>();
	var btmVertices : List.<Vector3> = new List.<Vector3>();
	var btmNormals : List.<Vector3> = new List.<Vector3>();
	var btmUv : List.<Vector2> = new List.<Vector2>();
	var btmTriangles : List.<int> = new List.<int>();
	
	//copy vertices, normals, UVs from top
	var vCount = vertices.Count;
	for(var cpv = 0; cpv < vCount; cpv++){
		btmVertices.Add(Vector3(vertices[cpv].x, Mathf.Pow(vertices[cpv].y, bottomExponent)*-bottomHeight/topHeight, vertices[cpv].z) + Vector3(0, -interval/2, 0));
		btmNormals.Add(normals[cpv]);
		btmUv.Add(colorsOnUv[bottomColorUv]);
	}
	
	//copy triangles from top
	var tCount = triangles.Count/3;
	for(var cpt = 0; cpt < tCount; cpt++){
		btmTriangles.Add(triangles[cpt*3+1]);
		btmTriangles.Add(triangles[cpt*3]);
		btmTriangles.Add(triangles[cpt*3+2]);
	}
	
	//copy border vertices
	var evCount = extremeVertices.Count;
	for(var ev = 0; ev < evCount; ev++){
		btmExtremeVertices.Add(extremeVertices[ev]+vCount);
		btmVertices[extremeVertices[ev]] = vertices[extremeVertices[ev]];
	}
	
	//assigned to the mesh
	btmMesh.vertices = btmVertices.ToArray();;
	btmMesh.normals = btmNormals.ToArray();;
	btmMesh.uv = btmUv.ToArray();;
	btmMesh.triangles = btmTriangles.ToArray();;
	
	btmfilter.sharedMesh = btmMesh;
	btmMeshRenderer.material = material;
	btmMesh.RecalculateNormals();
	btmMesh.RecalculateBounds();
	//create collider
    btmMeshCollider.sharedMesh = btmMesh;
	//set texture
	btmMeshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
	
	//make parrent
	bottom.transform.parent = transform;
}

function CreateObject(){
	var hit : RaycastHit;
	for(var obj = 0; obj < objectPrefab.Count; obj++){
		//fixed size of the island with objects. Calculated from size of the island and interval between objects.
		var objSize = Mathf.Round(areaSize/objectInterval[obj]);
		//to make center of mesh like a center of object
		var delayPos = Vector3((areaSize-objectInterval[obj])/2, 0, (areaSize-objectInterval[obj])/2);
		//random scatter from grid
		var randomDelay = objectInterval[obj]/3;
		
		for(var zobj = 0; zobj < objSize; zobj++){
			for(var xobj = 0; xobj < objSize; xobj++){
				var xCoord = parseFloat(xobj) / objSize * objectNoiseScale[obj];
				var yCoord = parseFloat(zobj) / objSize * objectNoiseScale[obj];
				//area in which the object can be
				var objHeight = Mathf.PerlinNoise(objectOffset[obj].x + xCoord, objectOffset[obj].y + yCoord);
				if(objHeight > 0.4){
					var xpos = xobj * objectInterval[obj] + transform.position.x;
					var zpos = zobj * objectInterval[obj] + transform.position.z;
					if(Physics.Raycast(Vector3(xpos + Random.Range(-randomDelay, randomDelay), topHeight, zpos + Random.Range(-randomDelay, randomDelay))-delayPos, -Vector3.up, hit) && hit.collider.gameObject == gameObject){
						if(hit.point.y > objectMinHeight[obj] * topHeight && hit.point.y <= objectMaxHeight[obj] * topHeight){
							if(Vector3.Angle(hit.normal, Vector3.up) < objectMaxAngle[obj]){
								//create object on hit point
								var newObj = Instantiate(objectPrefab[obj], hit.point, Quaternion.Euler(0, Random.Range(0.0, 360.0), 0));
								//make parrent
								newObj.parent = transform;
							}
						}
					}
				}
			}
		}
	}
}

function SetVertices(a : int, vertCol : int){
	var x = a%size.x;
	var y = parseInt((parseFloat(a)-(x+1))/size.y)+1;
	var h = Mathf.Pow(heights[a], topExponent);

	vertices.Add(Vector3(x/size.x*areaSize, h * topHeight, y/size.y*areaSize) - Vector3((areaSize-interval)/2, 0, (areaSize-interval)/2));
	normals.Add(Vector3.up);
	uv.Add(colorsOnUv[vertCol]);
	
	//it's a number of vertices of triangle
	var verticesA = vertices.Count-1;
	return verticesA;
}

function SetExtremeVertices(a : int, b : int){
	var ab = 0;
	
	var x = a%size.x;
	var y = Mathf.Round((parseFloat(a)-(x+1))/size.y);
	var h = Mathf.Pow(heights[y * size.x + x], 2);
	//position of "a" vertices
	var aVert = Vector3(x/size.x*areaSize, h * topHeight, y/size.y*areaSize) - Vector3((areaSize-interval)/2, interval/4, (areaSize-interval)/2);
	
	x = b%size.x;
	y = Mathf.Round((parseFloat(b)-(x+1))/size.y);
	h = Mathf.Pow(heights[b], 2);
	//position of "b" vertices
	var bVert = Vector3(x/size.x*areaSize, h * topHeight, y/size.y*areaSize) - Vector3((areaSize-interval)/2, interval/4, (areaSize-interval)/2);
	
	//position between "a" and "b" vertices
	var abCenter = (aVert+bVert)/2;

	vertices.Add(abCenter);
	normals.Add(Vector3.up);
	uv.Add(colorsOnUv[borderColorUv]);
	//number of vertices of triangle
	ab = vertices.Count-1;
	extremeVertices.Add(ab);
	
	return ab;
}

//make triangles from "biQuad"
function MakeTriangles(a : int, b : int, c : int, d : int, biQuadStr : String){
	if(biQuadStr == "0001"){
		triangles.Add(SetExtremeVertices(d, a));
		triangles.Add(SetExtremeVertices(c, d));
		triangles.Add(SetVertices(d, borderColorUv));
	}

	if(biQuadStr == "0010"){
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetExtremeVertices(c, d));
	}
	
	if(biQuadStr == "0011"){
		triangles.Add(SetExtremeVertices(d, a));
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetVertices(d, borderColorUv));
		
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetVertices(d, borderColorUv));
	}
	
	if(biQuadStr == "0100"){
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetExtremeVertices(b, c));
	}
	
	if(biQuadStr == "0101"){
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetExtremeVertices(d, a));
		
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetVertices(d, borderColorUv));
		triangles.Add(SetExtremeVertices(d, a));
		
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetExtremeVertices(c, d));
		triangles.Add(SetVertices(d, borderColorUv));
		
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetExtremeVertices(c, d));
	}
	
	if(biQuadStr == "0110"){
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetVertices(c, borderColorUv));
		
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetExtremeVertices(c, d));
	}
	
	if(biQuadStr == "0111"){
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetVertices(d, borderColorUv));
		
		triangles.Add(SetVertices(d, borderColorUv));
		triangles.Add(SetExtremeVertices(d, a));
		triangles.Add(SetExtremeVertices(a, b));
		
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetVertices(d, borderColorUv));
	}
	
	if(biQuadStr == "1000"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetExtremeVertices(d, a));
	}
	
	if(biQuadStr == "1001"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(d, borderColorUv));
		
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetExtremeVertices(c, d));
		triangles.Add(SetVertices(d, borderColorUv));
	}
	
	if(biQuadStr == "1010"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetVertices(c, borderColorUv));
		
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetVertices(c, borderColorUv));
		
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetExtremeVertices(c, d));
		triangles.Add(SetExtremeVertices(d, a));
		
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetExtremeVertices(d, a));
		triangles.Add(SetVertices(a, borderColorUv));
	}
	
	if(biQuadStr == "1011"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetExtremeVertices(a, b));
		triangles.Add(SetExtremeVertices(b, c));
		
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetVertices(a, borderColorUv));
		
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetVertices(d, borderColorUv));
		triangles.Add(SetVertices(a, borderColorUv));
	}

	if(biQuadStr == "1100"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetExtremeVertices(b, c));
		
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetExtremeVertices(d, a));
	}
	
	if(biQuadStr == "1101"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetVertices(d, borderColorUv));
		
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetExtremeVertices(b, c));
		triangles.Add(SetExtremeVertices(c, d));
		
		triangles.Add(SetExtremeVertices(c, d));
		triangles.Add(SetVertices(d, borderColorUv));
		triangles.Add(SetVertices(b, borderColorUv));
	}
	
	if(biQuadStr == "1110"){
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetVertices(b, borderColorUv));
		triangles.Add(SetVertices(c, borderColorUv));
		
		triangles.Add(SetVertices(c, borderColorUv));
		triangles.Add(SetExtremeVertices(c, d));
		triangles.Add(SetExtremeVertices(d, a));
		
		triangles.Add(SetExtremeVertices(d, a));
		triangles.Add(SetVertices(a, borderColorUv));
		triangles.Add(SetVertices(c, borderColorUv));
	}

	if(biQuadStr == "1111"){
		var newCol = 0;
		var triH = 0.0;
		var transAngle = 0.0;
		
		//fixed heights of vertices, with exponent
		var fHeightA = Mathf.Pow(heights[a], topExponent);
		var fHeightB = Mathf.Pow(heights[b], topExponent);
		var fHeightC = Mathf.Pow(heights[c], topExponent);
		var fHeightD = Mathf.Pow(heights[d], topExponent);
		
		var clrs = 0;
		var obj = 0;
		
		//choose diagonal to connect the square
		if(Mathf.Abs(fHeightA-fHeightC) < Mathf.Abs(fHeightB-fHeightD)){
			//tringle ABC and CDA
			//Color ABC
			for(clrs = 0; clrs < colors.Count; clrs++){
				triH = Mathf.Max(fHeightA, fHeightB, fHeightC);
				transAngle = colorTransValue[clrs] / topHeight * interval;
				if(triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]){
					triH = Mathf.Max(fHeightA, fHeightB, fHeightC) - Mathf.Min(fHeightA, fHeightB, fHeightC);
					if(triH > transAngle) newCol = clrs;
				}
			}
			triangles.Add(SetVertices(a, newCol));
			triangles.Add(SetVertices(b, newCol));
			triangles.Add(SetVertices(c, newCol));
			
			//Color CDA
			for(clrs = 0; clrs < colors.Count; clrs++){
				triH = Mathf.Max(fHeightC, fHeightD, fHeightA);
				transAngle = colorTransValue[clrs] / topHeight * interval;
				if(triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]){
					triH = Mathf.Max(fHeightC, fHeightD, fHeightA) - Mathf.Min(fHeightC, fHeightD, fHeightA);
					if(triH > transAngle) newCol = clrs;
				}
			}
			triangles.Add(SetVertices(c, newCol));
			triangles.Add(SetVertices(d, newCol));
			triangles.Add(SetVertices(a, newCol));
		}else{
			//tringle ABD and BCD
			//Color ABD
			for(clrs = 0; clrs < colors.Count; clrs++){
				triH = Mathf.Max(fHeightA, fHeightB, fHeightD);
				transAngle = colorTransValue[clrs] / topHeight * interval;
				if(triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]){
					triH = Mathf.Max(fHeightA, fHeightB, fHeightD) - Mathf.Min(fHeightA, fHeightB, fHeightD);
					if(triH > transAngle) newCol = clrs;
				}
			}
			triangles.Add(SetVertices(a, newCol));
			triangles.Add(SetVertices(b, newCol));
			triangles.Add(SetVertices(d, newCol));
			
			//Color BCD
			for(clrs = 0; clrs < colors.Count; clrs++){
				triH = Mathf.Max(fHeightB, fHeightC, fHeightD);
				transAngle = colorTransValue[clrs] / topHeight * interval;
				if(triH > colorMinHeight[clrs] && triH <= colorMaxHeight[clrs]){
					triH = Mathf.Max(fHeightB, fHeightC, fHeightD) - Mathf.Min(fHeightB, fHeightC, fHeightD);
					if(triH > transAngle) newCol = clrs;
				}
			}
			triangles.Add(SetVertices(b, newCol));
			triangles.Add(SetVertices(c, newCol));
			triangles.Add(SetVertices(d, newCol));
		}
	}
}

function CalcNoise(){
	for(var y = 0; y < size.y; y++){
		for(var x = 0; x < size.x; x++){
			var id = y * size.x + x;
			for(var hmC = 0; hmC < noiseScale.Count; hmC++){
				if(x < size.x-1 && y < size.y-1){
					var xCoord = parseFloat(x) / size.x * noiseScale[hmC];
					var yCoord = parseFloat(y) / size.y * noiseScale[hmC];
					if(hmC == 0){
						heights[id] = Mathf.PerlinNoise(offset[hmC].x + xCoord, offset[hmC].y + yCoord) * alpha[hmC];
					}else{	
						var a = heights[id];
						var b = Mathf.PerlinNoise(offset[hmC].x + xCoord, offset[hmC].y + yCoord) * alpha[hmC];
						//blending height maps
						heights[id] = BlendHeightMaps(a, b, blendMode[hmC]);
					}
				}
			}
			
			//blend height maps with shape("linear burn" blend mode)
			heights[id] = BlendHeightMaps(heights[id], shape.GetPixel(x/size.x * shape.width, y/size.y * shape.height).grayscale, 4);
			
			if(x == 0 || y == 0 || x == size.x-1 || y == size.y-1){
				heights[id] = 0;
			}
		}
	}

	//create texture
	var newColors : Color[] = new Color[16];
	texture = new Texture2D(4, 4);
	for(var newClrs = 0; newClrs < 16; newClrs++){
		if(newClrs < colors.Count){
			newColors[newClrs] = colors[newClrs];
		}else{
			newColors[newClrs] = Color(1, 1, 1, 1);
		}

		var yuv = Mathf.Floor(newClrs / 4);
		var xuv = newClrs - yuv * 4;
		
		//automatically settings "colorsOnUv" 
		colorsOnUv[newClrs] = Vector2(0.25*xuv+0.125, 0.25*yuv+0.125);
	}
	
	newColors[colors.Count] = borderColor;
	newColors[colors.Count+1] = bottomColor;
	borderColorUv = colors.Count;
	bottomColorUv = colors.Count+1;
	
	//apply texture
	texture.SetPixels(newColors);
	texture.Apply();
	texture.filterMode = FilterMode.Point;
}

function BlendHeightMaps(a : float, b : float, mode : int){
	var result : float;
	//multiply
	if(mode == 0){
		result = a*b;
	}
	//darken
	if(mode == 1){
		result = Mathf.Min(a, b);
	}
	//lighten
	if(mode == 2){
		result = Mathf.Max(a, b);
	}
	//exclusion
	if(mode == 3){
		result = 0.5 - 2*(a-0.5)*(b-0.5);
	}
	//linear burn
	if(mode == 4){
		result = a-(1-b);
	}
	
	return result;
}

function Update(){
	if(Input.GetKeyDown(KeyCode.E)){
		GenerateIsland();
	}
}