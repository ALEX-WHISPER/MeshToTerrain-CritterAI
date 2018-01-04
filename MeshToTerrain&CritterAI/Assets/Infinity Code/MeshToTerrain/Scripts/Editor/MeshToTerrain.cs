/*       INFINITY CODE 2013         */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MeshToTerrain : EditorWindow 
{
	[MenuItem("Window/Infinity Code/Mesh to Terrain", false, 2)] 
	static void OpenWindow () 
	{
		EditorWindow.GetWindow<MeshToTerrain>(false, "Mesh to Terrain");
	}
	
	private int baseMapResolution = 1024;
	private MeshToTerrainBounds bounds = MeshToTerrainBounds.autoDetect;
	private GameObject boundsGameObject;
	private GameObject container;
	private int detailResolution = 2048;
	private int heightmapResolution = 128;
	private Vector3 maxBounds = Vector3.zero;
	private List<GameObject> mesh = new List<GameObject>();
	private MeshToTerrainFindType meshFindType = MeshToTerrainFindType.gameObjects;
	private int meshLayer = 31;
	private Vector3 minBounds = Vector3.zero;
	private int newTerrainCountX = 1;
	private int newTerrainCountY = 1;
	private bool overwriteExists = false;
	private float raycastDistance = 1000;
	private int resolutionPerPatch = 16;
	private Vector2 scrollPos = Vector2.zero;
	private bool showMeshes = true;
	private bool showTerrains = true;
	private bool showTextures = true;
	private List<MeshCollider> tempColliders;
	private List<Terrain> terrain = new List<Terrain>();
	private MeshToTerrainSelectTerrainType terrainType = MeshToTerrainSelectTerrainType.existTerrains;
	private MeshToTerrainTextureType textureType = MeshToTerrainTextureType.noTexture;
	private Color textureEmptyColor = Color.white;
	private int textureHeight = 1024;
	private int textureWidth = 1024;
	
	private void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		
		showMeshes = EditorGUILayout.Foldout(showMeshes, "Meshes: ");
		if (showMeshes)
		{
			meshFindType = (MeshToTerrainFindType)EditorGUILayout.EnumPopup("Mesh select type: ", meshFindType);
			
			if (meshFindType == MeshToTerrainFindType.gameObjects)
			{
				for( int i = 0; i < mesh.Count; i++ ) 
				{
					mesh[i] = (GameObject)EditorGUILayout.ObjectField( mesh[i], typeof(GameObject), true);
					if (mesh[i] == null) 
					{
						mesh.RemoveAt(i);
						i--;
					}
				}
				GameObject newMesh = (GameObject)EditorGUILayout.ObjectField( null, typeof(GameObject), true);
				if (newMesh != null) 
				{
					bool findedMesh = false;
					foreach(GameObject cMesh in mesh) if (newMesh == cMesh) { findedMesh = true; break; }
					if (!findedMesh) mesh.Add(newMesh);
					else EditorUtility.DisplayDialog("Warning", "GameObject already added", "OK");
				}
			}
			else if (meshFindType == MeshToTerrainFindType.layers)
			{
				meshLayer = EditorGUILayout.LayerField("Layer: ", meshLayer);
			}
			
			EditorGUILayout.Space();
		}
		
		showTerrains = EditorGUILayout.Foldout(showTerrains, "Terrains: ");
		if (showTerrains)
		{
			terrainType = (MeshToTerrainSelectTerrainType) EditorGUILayout.EnumPopup("Type: ", terrainType);
			
			if (terrainType == MeshToTerrainSelectTerrainType.existTerrains)
			{
				for( int i = 0; i < terrain.Count; i++ ) 
				{
					terrain[i] = (Terrain)EditorGUILayout.ObjectField(terrain[i], typeof(Terrain), true);
					if (terrain[i] == null)
					{
						terrain.RemoveAt(i);
						i--;
					}
				}
				Terrain newTerrain = (Terrain) EditorGUILayout.ObjectField(null, typeof(Terrain), true);
				
				if (newTerrain != null) 
				{
					bool findedTerrain = false;
					foreach(Terrain cTerrain in terrain) if (newTerrain == cTerrain) { findedTerrain = true; break; }
					if (!findedTerrain) terrain.Add(newTerrain);
					else EditorUtility.DisplayDialog("Warning", "Terrain already added", "OK");
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Count terrains. X: ", GUILayout.ExpandWidth(false));
				newTerrainCountX = EditorGUILayout.IntField(newTerrainCountX, GUILayout.ExpandWidth(false));
				GUILayout.Label("Y: ", GUILayout.ExpandWidth(false));
				newTerrainCountY = EditorGUILayout.IntField(newTerrainCountY, GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				
				bounds = (MeshToTerrainBounds)EditorGUILayout.EnumPopup("Bounds: ", bounds);
				if (bounds == MeshToTerrainBounds.fromGameobject) boundsGameObject = (GameObject)EditorGUILayout.ObjectField("Bounds GameObject: ", boundsGameObject, typeof(GameObject), true);
				
				detailResolution = EditorGUILayout.IntField("Detail Resolution", detailResolution);
				resolutionPerPatch = EditorGUILayout.IntField("Resolution Per Patch", resolutionPerPatch);
				baseMapResolution = EditorGUILayout.IntField("Base Map Resolution", baseMapResolution);
				heightmapResolution = EditorGUILayout.IntField("Height Map Resolution", heightmapResolution);
			}
			EditorGUILayout.Space();
		}
		
		showTextures = EditorGUILayout.Foldout(showTextures, "Textures: ");
		if (showTextures)
		{
			textureType = (MeshToTerrainTextureType)EditorGUILayout.EnumPopup("Type: ", textureType);
			
			if (textureType == MeshToTerrainTextureType.bakeMainTextures)
			{
				textureWidth = EditorGUILayout.IntField("Width: ", textureWidth);
				textureHeight = EditorGUILayout.IntField("Height: ", textureHeight);
				textureEmptyColor = EditorGUILayout.ColorField("Empty color: ", textureEmptyColor);
			}
			
			EditorGUILayout.Space();
		}
		
		EditorGUILayout.EndScrollView();
		
		if (GUILayout.Button("Start", GUILayout.Height(40)))
		{
			preStart();
		}
	}
	
	private void addTempCollider(GameObject go, bool recursive)
	{
		if (go.GetComponent<Collider>() == null) tempColliders.Add(go.AddComponent<MeshCollider>());
		if (recursive) for (int i = 0; i < go.transform.GetChildCount(); i++) addTempCollider(go.transform.GetChild(i).gameObject, true);
	}
	
	private bool checkAssetNames()
	{
		bool showDialog = false;
		
		for (int j = 0; j < newTerrainCountY; j++)
		{
			for (int i = 0; i < newTerrainCountX; i++)
			{
				string terrainename = string.Format("Assets/Generated Terrain {0}x{1}.asset", i, j);
				string texturename = string.Format("Assets/Generated Terrain {0}x{1}.png", i, j);;
				if ((terrainType == MeshToTerrainSelectTerrainType.newTerrains && File.Exists(terrainename)) || (textureType == MeshToTerrainTextureType.bakeMainTextures && File.Exists(texturename)))
				{
					showDialog = true;
					break;
				}
			}
			if (showDialog) break;
		}
		
		if (showDialog)
		{
			int result = EditorUtility.DisplayDialogComplex("Warning", "Some assets already exist.                                ", "Overwrite", "Create with new names", "Cancel");
			overwriteExists = result == 0;
			return result != 2;
		}
		return true;
	}
	
	private void createNewTerrains()
	{
		terrain = new List<Terrain>();
		
		const string containerName = "Generated terrains";
		string cName = containerName;
		int index = 1;
		while (GameObject.Find(cName) != null)
		{
			cName = containerName + " " + index.ToString();
			index++;
		}
		
		container = new GameObject(cName);
		container.transform.position = new Vector3(0, minBounds.y - 5, 0);
		
		float w = maxBounds.x - minBounds.x;
		float h = maxBounds.z - minBounds.z;
		
		int sW = Mathf.FloorToInt(w / newTerrainCountX);
		int sH = Mathf.FloorToInt(h / newTerrainCountY);
		int sY = Mathf.FloorToInt((maxBounds.y - minBounds.y) * 1.5f);
		float offX = (w - sW * newTerrainCountX) / 2;
		float offY = (h - sH * newTerrainCountY) / 2;
		
		for (int j = 0; j < newTerrainCountY; j++)
		{
			for (int i = 0; i < newTerrainCountX; i++)
			{
				TerrainData tdata = new TerrainData();
				tdata.SetDetailResolution(detailResolution, resolutionPerPatch);
				tdata.baseMapResolution = baseMapResolution;
				tdata.heightmapResolution = heightmapResolution;
				tdata.size = new Vector3(sW, sY, sH);
				string terrainName = string.Format("Generated Terrain {0}x{1}", i, j);
				string filename = Path.Combine("Assets", terrainName + ".asset");
				index = 1;
				
				while(File.Exists(filename) && !overwriteExists)
				{
					filename = Path.Combine("Assets", terrainName + " " + index.ToString() + ".asset");
					index++;
				}
				
				AssetDatabase.CreateAsset(tdata, filename);
				GameObject terrainGO = Terrain.CreateTerrainGameObject(tdata);
				terrainGO.name = terrainName;
				terrainGO.transform.parent = container.transform;
				terrainGO.transform.localPosition = new Vector3(minBounds.x + i * sW + offX, 0, minBounds.z + j * sH + offY);
				terrain.Add(terrainGO.GetComponent<Terrain>());
			}
		}
		
		if (terrain.Count > 1)
		{
			for (int i = 0; i < terrain.Count; i++)
			{
				int leftIndex = (i % newTerrainCountX != 0)? i - 1: -1;
				int rightIndex = (i % newTerrainCountX != newTerrainCountX - 1)? i + 1: -1;
				int topIndex = i - newTerrainCountX;
				int bottomIndex = i + newTerrainCountX;
				Terrain left = (newTerrainCountX > 1 && leftIndex != -1)? terrain[leftIndex]: null;
				Terrain right = (newTerrainCountX > 1 && rightIndex != -1)? terrain[rightIndex]: null;
				Terrain top = (newTerrainCountY > 1 && topIndex >= 0)? terrain[topIndex]: null;
				Terrain bottom = (newTerrainCountY > 1 && bottomIndex < terrain.Count)? terrain[bottomIndex]: null;
				terrain[i].SetNeighbors(left, bottom, right, top);
			}
		}
	}
	
	private void FindBounds()
	{
		minBounds = Vector3.zero;
		maxBounds = Vector3.zero;
		
		foreach (GameObject m in mesh)
		{
			foreach (Renderer r in m.GetComponentsInChildren<Renderer>())
			{
				Vector3 min = r.bounds.min;
				Vector3 max = r.bounds.max;
				if (minBounds == Vector3.zero) 
				{
					minBounds = min;
					maxBounds = max;
				}
				else
				{
					if (minBounds.x > min.x) minBounds.x = min.x;
					if (minBounds.y > min.y) minBounds.y = min.y;
					if (minBounds.z > min.z) minBounds.z = min.z;
					
					if (maxBounds.x < max.x) maxBounds.x = max.x;
					if (maxBounds.y < max.y) maxBounds.y = max.y;
					if (maxBounds.z < max.z) maxBounds.z = max.z;
				}
			}
		}
	}
	
	private int FindFreeLayer()
	{
		bool[] ls = new bool[32];
		
		for (int i = 0; i < 32; i++) ls[i] = true;
		foreach(GameObject go in (GameObject[])FindObjectsOfType(typeof(GameObject))) ls[go.layer] = false;
		
		for (int i = 31; i > 0; i--) if (ls[i]) return i;
		return -1;
	}
	
	private List<GameObject> FindGameObjectsWithLayer (int layer)
	{
    	GameObject[] goArray = (GameObject[])FindObjectsOfType(typeof(GameObject));
    	List<GameObject> goList = new List<GameObject>();
    	foreach(GameObject go in goArray) if (go.GetComponent<MeshFilter>() != null && go.layer == layer) goList.Add(go);
    	return goList;
	}
	
	private void preStart()
	{
		tempColliders = new List<MeshCollider>();
		
		if (terrainType == MeshToTerrainSelectTerrainType.newTerrains && bounds == MeshToTerrainBounds.fromGameobject)
		{
			if (boundsGameObject == null)
			{
				EditorUtility.DisplayDialog("Error", "Boundaries GameObject are not set.", "OK"); 
				return;
			}
			else 
			{
				Renderer r = boundsGameObject.GetComponent<Renderer>();
				if (r == null)
				{
					EditorUtility.DisplayDialog("Error", "Boundaries GameObject does not contain the Renderer component.", "OK"); 
					return;
				}
				minBounds = r.bounds.min;
				maxBounds = r.bounds.max;
			}
		}
		else FindBounds();
		
		if (minBounds == Vector3.zero && maxBounds == Vector3.zero) 
		{
			EditorUtility.DisplayDialog("Error", "Can not define the boundaries of the model.", "OK"); 
			return;
		}
		
		if (!checkAssetNames()) return;
		
		if (terrainType == MeshToTerrainSelectTerrainType.newTerrains) createNewTerrains();
		if (terrain.Count == 0) 
		{
			EditorUtility.DisplayDialog("Error", "No terrains added.", "OK"); 
			return; 
		}
		else if (meshFindType == MeshToTerrainFindType.gameObjects)
		{
			if (mesh.Count == 0) 
			{
				EditorUtility.DisplayDialog("Error", "No meshes added.", "OK"); 
				return; 
			}
			else 
			{
				meshLayer = FindFreeLayer();
				if (meshLayer == -1) 
				{ 
					meshLayer = 31;
					EditorUtility.DisplayDialog("Error", "Can not find the free layer.", "OK"); 
					return; 
				}
			}
		}
		else if (meshFindType == MeshToTerrainFindType.layers)
		{
			if (meshLayer == 0) 
			{
				EditorUtility.DisplayDialog("Error", "Cannot use dafault layer.", "OK"); 
				return; 
			}
		}
		
		List<MeshToTerrainObject> objs = new List<MeshToTerrainObject>();
		
		if (meshFindType == MeshToTerrainFindType.gameObjects)
		{
			for(int i = 0; i < mesh.Count; i++)
			{
				MeshFilter[] mfs = (MeshFilter[])mesh[i].GetComponentsInChildren<MeshFilter>();
				foreach (MeshFilter m in mfs) 
				{
					GameObject go = m.gameObject;
					objs.Add(new MeshToTerrainObject(go));
					addTempCollider(go, false);
					go.layer = meshLayer;
				}
			}
		}
		else if (meshFindType == MeshToTerrainFindType.layers)
		{
			List<GameObject> gos = FindGameObjectsWithLayer(meshLayer);
			foreach(GameObject go in gos) addTempCollider(go, false);
		}
		
		List<TerrainData> tData = new List<TerrainData>();
		foreach (Terrain t in terrain) tData.Add(t.terrainData);
		
		Undo.RegisterUndo(tData.ToArray(), "Mesh to Terrain");
		
		foreach (Terrain t in terrain) startTerrainUpdate(t);
		removeTempColliders();
		if (meshFindType == MeshToTerrainFindType.gameObjects) foreach (MeshToTerrainObject m in objs) m.gameobject.layer = m.layer;
		
		if (terrainType == MeshToTerrainSelectTerrainType.newTerrains) EditorGUIUtility.PingObject(container);
		else foreach (Terrain t in terrain) EditorGUIUtility.PingObject(t.gameObject);
	}
	
	private void startTerrainUpdate(Terrain t)
	{
		int mLayer = 1 << meshLayer;
		
		raycastDistance = maxBounds.y - t.transform.position.y + 10;
		
		Vector3 vScale = t.terrainData.heightmapScale;
		Vector3 beginPoint = t.transform.position;
		beginPoint.y += raycastDistance;
		float[,] heights = new float[t.terrainData.heightmapWidth, t.terrainData.heightmapHeight];
		float tdist = raycastDistance;
		
		RaycastHit hit;
		Vector3 curPoint;
		
		for (int i = 0; i < t.terrainData.heightmapWidth; i++)
		{
			for (int j = 0; j < t.terrainData.heightmapHeight; j++)
			{
				curPoint = beginPoint + new Vector3(i * vScale.x, 0, j * vScale.z);
				if (Physics.Raycast(curPoint, -Vector3.up, out hit, raycastDistance, mLayer)) heights[j, i] = (tdist - hit.distance) / vScale.y;
				else heights[j, i] = 0;
			}
		}
		
		t.terrainData.SetHeights(0, 0, heights);
		
		if (textureType == MeshToTerrainTextureType.bakeMainTextures)
		{
			Texture2D texture = new Texture2D(textureWidth, textureHeight);
			vScale.x = vScale.x * t.terrainData.heightmapWidth / textureWidth;
			vScale.z = vScale.z * t.terrainData.heightmapHeight / textureHeight;
			
			Color[] colors = new Color[textureWidth * textureHeight];
			Renderer lastRenderer = null;
			Mesh m = null;
			Vector2[] uv = null;
			int []triangles = null;
			Vector3[] verticles = null;
			
			for (int i = 0; i < textureWidth; i++)
			{
				for (int j = 0; j < textureHeight; j++)
				{
					curPoint = beginPoint + new Vector3(i * vScale.x, 0, j * vScale.z);
					int cPos = j * textureWidth + i;
					
					if (Physics.Raycast(curPoint, -Vector3.up, out hit, raycastDistance, mLayer)) 
					{
						Renderer renderer = hit.collider.GetComponent<Renderer>();
						if (renderer != null && renderer.sharedMaterial != null)
						{
							if (lastRenderer != renderer)
							{
								lastRenderer = renderer;
								m = renderer.GetComponent<MeshFilter>().sharedMesh;
								triangles = m.triangles;
								verticles = m.vertices;
								uv = m.uv;
							}
							Material mat = renderer.sharedMaterial;
							if (mat.mainTexture != null)
							{
								Texture2D mainTexture = (Texture2D)mat.mainTexture;
								Vector3 localPoint = renderer.transform.InverseTransformPoint(hit.point);
								int triangle = hit.triangleIndex * 3;
								Vector3 v1 = verticles[triangles[triangle]];
								Vector3 v2 = verticles[triangles[triangle + 1]];
								Vector3 v3 = verticles[triangles[triangle + 2]];
								Vector3 f1 = v1 - localPoint;
								Vector3 f2 = v2 - localPoint;
								Vector3 f3 = v3 - localPoint;
								float a = Vector3.Cross(v1 - v2, v1 - v3).magnitude;
								float a1 = Vector3.Cross(f2, f3).magnitude / a;
								float a2 = Vector3.Cross(f3, f1).magnitude / a;
								float a3 = Vector3.Cross(f1, f2).magnitude / a;
								Vector3 textureCoord = uv[triangles[triangle]] * a1 + uv[triangles[triangle + 1]] * a2 + uv[triangles[triangle + 2]] * a3;
		
								colors[cPos] = mainTexture.GetPixelBilinear(textureCoord.x, textureCoord.y);
							}
							else
							{
								colors[cPos] = mat.color;
							}
						}
						else
						{
							colors[cPos] = textureEmptyColor;
						}
					}
					else colors[cPos] = textureEmptyColor;
				}
			}
			
			texture.SetPixels(colors);
			texture.Apply();
			
			
			string textureFilename = Path.Combine("Assets", t.name + ".png");
			int index = 1;
			
			while(File.Exists(textureFilename) && !overwriteExists)
			{
				textureFilename = Path.Combine("Assets", t.name + " " + index.ToString() + ".png");
				index++;
			}
			
			File.WriteAllBytes(textureFilename, texture.EncodeToPNG());
			AssetDatabase.Refresh();
			texture = (Texture2D)AssetDatabase.LoadAssetAtPath(textureFilename, typeof(Texture2D));
			
			List<SplatPrototype> sps = new List<SplatPrototype>();
			SplatPrototype sp = new SplatPrototype();
			sp.tileSize = new Vector2(t.terrainData.heightmapWidth * t.terrainData.heightmapScale.x, t.terrainData.heightmapHeight * t.terrainData.heightmapScale.z);
			sp.texture = texture;
			sps.Add(sp);
			
			t.terrainData.splatPrototypes = sps.ToArray();
		}
		t.Flush();
	}
	
	private void removeTempColliders()
	{
		foreach(MeshCollider mc in tempColliders) DestroyImmediate(mc);
	}
}

public class MeshToTerrainObject
{
	public GameObject gameobject;
	public int layer;
	
	public MeshToTerrainObject(GameObject gameObject)
	{
		gameobject = gameObject;
		layer = gameObject.layer;
	}
}

public enum MeshToTerrainBounds
{
	autoDetect,
	fromGameobject
}

public enum MeshToTerrainFindType
{
	gameObjects,
	layers
}
			
public enum MeshToTerrainSelectTerrainType
{
	existTerrains,
	newTerrains
}

public enum MeshToTerrainTextureType
{
	noTexture,
	bakeMainTextures
}