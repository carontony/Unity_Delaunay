using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Voronoi 
{
	List<Vector2> site;
	Delaunay delaunay;

	public Voronoi(Delaunay _delaunay)
	{
		delaunay = _delaunay;
		site = delaunay.getSites();
	}
	private bool isOut(Vector2 p, Triangle t)
	{
		Vector2 p0 = site[0];
		Vector2 p1 = site[1];
		Vector2 p2 = site[2];
		Vector2 p3 = site[3];

		if(t.a == p0 || t.a == p1 || t.a == p2 || t.a == p3)
			return true;
		if(t.b == p0 || t.b == p1 || t.b == p2 || t.b == p3)
			return true;
		if(t.c == p0 || t.c == p1 || t.c == p2 || t.c == p3)
			return true;

		return false;
	}

	public List<Cell> createVoronoi()
	{
		List<Cell> region = new List<Cell>();

		
		for (int i = 4; i< site.Count; i++)
		{
			// TODO Refactor
			// Find one Triange that contain the point
			foreach (Triangle t in delaunay.triangleList.Values)
			{
				//if(isOut(site[i], t))
				   //break;
				if (t.a == site [i] || t.b == site [i] || t.c == site [i])
				{
					region.Add(createRegion(site [i], t, t));
					break;
				}
			}
		}
		
		return region;
	}

	public void triangleTraversal(Vector2 p, Triangle firstTriangle, Triangle t, Edge previousEdge, ref Cell cell)
	{
		//List
		Edge e = t.edgeList [0].ContainVector(p) && t.edgeList [0] != previousEdge ? t.edgeList [0] : (t.edgeList [1].ContainVector(p) && t.edgeList [1] != previousEdge ? t.edgeList [1] : t.edgeList [2]);
		
		Triangle adjTriangle1 = e.triangleList.ElementAt(0).Value;
		Triangle adjTriangle2 = e.triangleList.ElementAt(1).Value;
		
		Triangle nextTriangle = adjTriangle1 == t ? adjTriangle2 : adjTriangle1;

		//if(nextTriangle.circumCenter.x <= 20 && nextTriangle.circumCenter.x >= 0 && t.circumCenter.x <= 20 && t.circumCenter.x >= 0)
		Debug.DrawLine(t.circumCenter, nextTriangle.circumCenter, Color.green, 50);
		cell.AddEdge(t.circumCenter, nextTriangle.circumCenter);
		
		if (nextTriangle != firstTriangle)
		{
			triangleTraversal(p, firstTriangle, nextTriangle, e, ref cell);
		}
		// End
		else
		{
			
		}
	}
	
	public Cell createRegion(Vector2 site, Triangle firstTriangle, Triangle t)
	{
		Cell cell = new Cell();
		cell.center = site;
		
		triangleTraversal(site, firstTriangle, t, null, ref cell);
		
		Vector2 v0 = site;
		Vector2 v1 = cell.cellEdgeList.ElementAt(0).Value.a;
		Vector2 v2 = cell.cellEdgeList.ElementAt(0).Value.b;
		Vector3 surfaceNormal = Vector3.Cross (v2 - v0, v1 - v0).normalized;
		
		if(surfaceNormal != Vector3.back)
			cell.cellEdgeList.Reverse();
		
		UnityEngine.Debug.Log("::"+surfaceNormal);
		
		return cell;
	}

}

