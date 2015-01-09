using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using System.Linq;

public static class EdgeMaintener
{
	
	public static List<Edge> edgeList = new List<Edge>();
	
	public static Edge createEdge(Vector2 a, Vector2 b, Triangle t)
	{
		// TODO test dictionary
		for (int i = 0; i < edgeList.Count; i++)
		{
			// Edge deja dans la liste
			if ((edgeList [i].a == a && edgeList [i].b == b) || (edgeList [i].a == b && edgeList [i].b == a))
			{
				// Triangle deja dans ce edge
				if (edgeList [i].triangleList.ContainsKey(t))
				{
					return edgeList [i];
				}
				
				
				edgeList [i].triangleList.Add(t, t);
				
				if (edgeList [i].triangleList.Count == 4)
				{
					UnityEngine.Debug.LogWarning("Triangle 4");
				}
				
				return edgeList [i];
				
			}
		}
		Edge edge = new Edge();
		edge.a = a;
		edge.b = b;
		edge.triangleList.Add(t, t);
		edgeList.Add(edge);
		
		return edge;
	}
	
	public static bool removeEdge(Edge edge, Triangle t)
	{
		// TODO remove from list if empty
		
		//--
		
		if (edge.triangleList.ContainsKey(t))
			edge.triangleList.Remove(t);
		else
			throw new Exception("Problème de flotant surement");
		
		for (int i = EdgeMaintener.edgeList.Count-1; i >= 4; i--)
		{
			if (EdgeMaintener.edgeList [i].triangleList.Count == 0)
			{
				EdgeMaintener.edgeList.RemoveAt(i);
			}
		}
		
		
		return false;
		
	}
}
public class Edge
{
	public Vector2 a;
	public Vector2 b;
	
	// A key contain always the opposite triangle as value
	public Dictionary<Triangle, Triangle> triangleList = new Dictionary<Triangle,Triangle>();
	
	public bool ContainVector(Vector2 v)
	{
		return (v == a || v == b);
	}
}

class EdgeToFlip
{
	public Vector2 pointInserted;
	public Edge edgeToFlip;
	
	public EdgeToFlip(Edge _edgeToFlip, Vector2 _pointInserted)
	{
		pointInserted = _pointInserted;
		edgeToFlip = _edgeToFlip;
	}
}

