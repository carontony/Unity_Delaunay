using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator 
{
	List<Cell> cellList = new List<Cell>();
	List<Region> regionList = new List<Region>();

	Dictionary<Cell, Region> CellToRegion = new Dictionary<Cell,Region>();

	public void makeIsland()
	{
		// Depend to the height and with map
		float radius = 8f; 
		int xh = 10;
		int yh = 10;

		// Create Region
		foreach(Cell cell in cellList)
		{
			Region r = new Region();
			r.cell = cell;

			//https://sites.google.com/site/jicenospam/buildanisland
			float radiusL = radius*radius - ( r.cell.center.x - xh )*( r.cell.center.x - xh ) - (r.cell.center.y - yh)*(r.cell.center.y - yh);

			if(radiusL >= 0)
			{
				r.color = Color.gray;
			}
			else
			{
				r.color = Color.blue;
			}


		}
	}
	public MapGenerator(List<Cell> _cellList)
	{
		cellList = _cellList;
		makeIsland();

		Dictionary<Vector4, CellEdge> cellEdgeList = CellEdgeMaintener.cellEdgeList;
		Dictionary<Vector2,Vector3> newVertices = new Dictionary<Vector2,Vector3>(); // Dictionary is used here to know when a vertex is already inserted
		Dictionary<Vector2, int> vIndex = new Dictionary<Vector2,int>(); // grrr : Used to find a key index in the newVertices Dictionary

		List<int> newTriangles = new List<int>();
		List<Vector2> newUV = new List<Vector2>();
		List<Color> newColors = new List<Color>();

		GameObject regionGO;
		UnityEngine.Object RegionPrefab = Resources.Load("Prefabs/Region");
		regionGO =(GameObject) MonoBehaviour.Instantiate (RegionPrefab);
		
		Mesh mesh = regionGO.GetComponent<MeshFilter> ().mesh ;

		int index = 0;

		// Cell edge traversal
		for(int i = 0; i < cellEdgeList.Count; i++)
		{
			Vector2 a = cellEdgeList.ElementAt(i).Value.a;
			Vector2 b = cellEdgeList.ElementAt(i).Value.b;

			//int woodPerlin = radius*radius - ( region.center.x - xh )*( region.center.x - xh ) - (region.center.y - yh)*(region.center.y - yh);
			float woodPerlin = (float)PerlinNoise((int)a.x, 0, (int)a.y, 5, 3.8f, 0f);
			float woodPerlinb = (float)PerlinNoise((int)b.x, 0, (int)b.y, 5, 3.8f, 0f);
			//woodPerlin = -0.5f;
			woodPerlin = -woodPerlin;
			woodPerlinb = -woodPerlinb;

			Color color = new  Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
			color = Color.white;

			// Index in newVertice for this edge
			int p0, p1, p2;
			
			if(!newVertices.ContainsKey(a))
			{
				newVertices.Add(a, new Vector3(a.x, a.y, woodPerlin));
				newUV.Add(new Vector2(0,0));
				newColors.Add(color);
				p0 = index;
				vIndex.Add(a, index++);
			}
			else
				p0 = vIndex[a];

			if(!newVertices.ContainsKey(b))
			{
				newVertices.Add(b, new Vector3(b.x, b.y, woodPerlinb));
				newUV.Add(new Vector2(0,0));
				newColors.Add(color);
				p1 = index;
				vIndex.Add(b, index++);
			}
			else
				p1 = vIndex[b];

			// Draw triangles
			for(int j = 0; j < cellEdgeList.ElementAt(i).Value.cellList.Count; j++)
			{
				Cell cell =  cellEdgeList.ElementAt(i).Value.cellList.ElementAt(j).Value;

				// Add centerpoint
				if(!newVertices.ContainsKey(cell.center))
				{
					newVertices.Add(cell.center, new Vector3(cell.center.x, cell.center.y, woodPerlin));
					newUV.Add(new Vector2(0,0));
					newColors.Add(color);
					p2 = index;
					vIndex.Add(cell.center, index++);
				}
				else
					p2 = vIndex[cell.center];


				Vector3 surfaceNormal = Vector3.Cross (newVertices.ElementAt(p0).Value - newVertices.ElementAt(p2).Value, newVertices.ElementAt(p1).Value - newVertices.ElementAt(p2).Value).normalized;
	
				if(Vector3.Angle(surfaceNormal,Vector3.back) < 90)
				{
					newTriangles.Add(p0);
					newTriangles.Add(p1);
					newTriangles.Add(p2);
				}
				else
				{
					newTriangles.Add(p2);
					newTriangles.Add(p1);
					newTriangles.Add(p0);
				}


			}

			mesh.Clear ();
			mesh.vertices = newVertices.Values.ToArray();
			mesh.triangles = newTriangles.ToArray();
			mesh.uv = newUV.ToArray(); // add this line to the code here
			mesh.colors = newColors.ToArray();
			//mesh.Optimize ();
			mesh.RecalculateNormals ();
		}

	
	}

	
	int PerlinNoise(int x,int y, int z, float scale, float height, float power){
		float rValue;
		
		rValue=Noise.Noise.GetNoise (((double)x) / scale, ((double)y)/ scale, ((double)z) / scale);
		rValue*=height;
		
		if(power!=0){
			rValue=Mathf.Pow( rValue, power);
		}
		
		return (int) rValue;
	}
}

