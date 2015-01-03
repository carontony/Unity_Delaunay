using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class EdgeMaintener{
	
	public static List<Edge> edgeList = new List<Edge>();
	
	public static Edge createEdge(Vector2 a, Vector2 b, Triangle t)
	{
		// TODO test dictionary
		for(int i = 0; i < edgeList.Count; i++)
		{
			// Edge deja dans la liste
			if((edgeList[i].a == a && edgeList[i].b == b) || (edgeList[i].a == b && edgeList[i].b == a))
			{
				// Triangle deja dans ce edge
				for(int j = 0; j < edgeList[i].triangleList.Count; j++)
				{
					if(edgeList[i].triangleList[j] == t)
						return edgeList[i];
				}
				
				edgeList[i].triangleList.Add(t);
				
				return edgeList[i];
				
			}
		}
		Edge edge = new Edge();
		edge.a = a;
		edge.b = b;
		edge.triangleList.Add(t);
		edgeList.Add(edge);
		
		return edge;
	}
	
	public static bool removeEdge(Edge edge, Triangle t)
	{
		// TODO remove from list if empty
		
		//--
	
		for(int j = 0; j < edge.triangleList.Count; j++)
		{
			if(edge.triangleList[j] == t)
			{
				edge.triangleList.RemoveAt(j);
				return true;
				//return edgeList[i];
			}

		}

		return false;
		
	}
}
public class Edge{
	public Vector2 a;
	public Vector2 b;
	
	public List<Triangle> triangleList = new List<Triangle>();
}

