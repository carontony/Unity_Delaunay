using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshCreator 
{
	public float woodPerlin;
	List<Region> regionList = new List<Region>();

	Dictionary<int,int> EdgeElevationTraversal = new Dictionary<int,int>(); // Dictionary is used here to know when a vertex is already inserted
	
	public Vector2 Contract(Vector2 p, Vector2 centerpoint)
	{
		Vector2 dir;
		float distance;

		distance = (centerpoint - p).magnitude;
		dir = (centerpoint - p).normalized;

		return p + (dir * (distance / 6));
	}

	public Region createRegion(Cell cell)
	{
		Region r = new Region();
		r.cell = cell;

		// TODO Depend to the height and with map
		float radius = 8f; 
		int xh = 10;
		int yh = 10;

		// Is ground ?
		float groudRadius = radius*radius - ( r.cell.center.x - xh )*( r.cell.center.x - xh ) - (r.cell.center.y - yh)*(r.cell.center.y - yh);
		r.color = groudRadius >= 0 ? Color.gray : Color.blue;

		// Is mountain
		float notMountain = (float)PerlinNoise((int)cell.center.x, 0, (int)cell.center.y, 5, 3.8f, 0f);
		r.color = notMountain == 0 && groudRadius >= 0 ? Color.black : r.color;

		if(notMountain == 0 && groudRadius >= 0)
			propagateElevation(cell);


		_Debug.LogOnScreen("0", cell.center);

		return r;
	}

	public void propagateElevation()
	{
		for(int i=0; i < cell.cellEdgeList.Count; i++)
		{
			
		}
	}

	public MeshCreator(Cell cell)
	{
		Dictionary<Vector2,Vector3> newVertices = new Dictionary<Vector2,Vector3>(); // Dictionary is used here to know when a vertex is already inserted
		Dictionary<Vector2, int> vIndex = new Dictionary<Vector2,int>(); // grrr : Used to find a key index in the newVertices Dictionary

		List<int> newTriangles = new List<int>();
		List<Vector2> newUV = new List<Vector2>();
		List<Color> newColors = new List<Color>();

		Region r = createRegion(cell);

		Color color = new  Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
		color = Color.blue;

		GameObject cellGO;
		UnityEngine.Object cellPrefab = Resources.Load("Prefabs/Region");
		cellGO =(GameObject) MonoBehaviour.Instantiate (cellPrefab);
	
		Mesh mesh = cellGO.GetComponent<MeshFilter> ().mesh ;
		cellGO.name = woodPerlin.ToString();

		// Maintain an index of vertices
		int index = 0;

		// Index in newVertice for this edge
		int p0, p1, p2;

		// Index in newVertice for this contracted point
		int pc0, pc1;

		Vector2 contractedPoint;

		// Add center point
		newVertices.Add(cell.center, new Vector3(cell.center.x, cell.center.y, 0));
		newUV.Add(new Vector2(0,0));
		newColors.Add(r.color);
		p2 = index;
		vIndex.Add(cell.center, index++);

		// Parcous des edges de la cellule
		for(int i=0; i < cell.cellEdgeList.Count; i++)
		{
			Vector2 a = cell.cellEdgeList.ElementAt(i).Value.a;
			Vector2 b = cell.cellEdgeList.ElementAt(i).Value.b;
			
			if(!newVertices.ContainsKey(a))
			{
				newVertices.Add(a, new Vector3(a.x, a.y, 0));
				newUV.Add(new Vector2(0,0));
				newColors.Add(r.color);
				p0 = index;
				vIndex.Add(a, index++);

				contractedPoint = Contract(a, cell.center);
				newVertices.Add(contractedPoint, new Vector3(contractedPoint.x, contractedPoint.y, 0));
				newUV.Add(new Vector2(0,0));
				newColors.Add(r.color);
				pc0 = index;
				vIndex.Add(contractedPoint, index++);
			}
			else
			{
				p0 = vIndex[a];

				contractedPoint = Contract(a, cell.center);
				pc0 = vIndex[contractedPoint];
			}
			
			if(!newVertices.ContainsKey(b))
			{
				newVertices.Add(b, new Vector3(b.x, b.y, 0));
				newUV.Add(new Vector2(0,0));
				newColors.Add(r.color);
				p1 = index;
				vIndex.Add(b, index++);

				contractedPoint = Contract(b, cell.center);
				newVertices.Add(contractedPoint, new Vector3(contractedPoint.x, contractedPoint.y, 0));
				newUV.Add(new Vector2(0,0));
				newColors.Add(r.color);
				pc1 = index;
				vIndex.Add(contractedPoint, index++);
			}
			else
			{
				p1 = vIndex[b];

				contractedPoint = Contract(b, cell.center);
				pc1 = vIndex[contractedPoint];
			}

			// Border triangle
			createTriangle(p0, pc0, p1, newVertices, newTriangles);
			createTriangle(pc0, pc1, p1, newVertices, newTriangles);

			// Center triangle
			createTriangle(pc0, pc1, p2, newVertices, newTriangles);
		}


		mesh.Clear ();
		mesh.vertices = newVertices.Values.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray(); // add this line to the code here
		mesh.colors = newColors.ToArray();
		//mesh.Optimize ();
		mesh.RecalculateNormals ();
		
	}

	public void createTriangle(int p0, int p1, int p2, Dictionary<Vector2,Vector3> newVertices, List<int> newTriangles)
	{
		Vector3 surfaceNormal = getNormal(newVertices.ElementAt(p0).Value, newVertices.ElementAt(p1).Value, newVertices.ElementAt(p2).Value);

		if(Vector3.Angle(surfaceNormal,Vector3.back) < 90)
		{
			newTriangles.Add(p0);
			newTriangles.Add(p1);
			newTriangles.Add(p2);
		}
		else
		{
			newTriangles.Add(p0);
			newTriangles.Add(p2);
			newTriangles.Add(p1);
		}
	}

	public Vector3 getNormal(Vector2 a, Vector2 b, Vector2 c)
	{
		return Vector3.Cross (a - c, b - c).normalized;
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

