using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Triangle
{
	public Vector2 a;
	public Vector2 b;
	public Vector2 c;


	public Edge edgeAb;
	public Edge edgeBc;
	public Edge edgeCa;
	
	public float circumRadius;
	public Vector2 circumCenter;
	
	public bool toDelete = false;

	public List<Edge> edgeList = new List<Edge>();
	
	public Color color = Color.yellow;
	
	public Triangle(Vector2 _a, Vector2 _b, Vector2 _c)
	{
		a = _a;
		b = _b;
		c = _c;


		vectorsToEdges();


		color = new  Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
	}

	public void edgesToVectors()
	{

		// Dirty parade :)
		a = edgeList[0].a;
		b = edgeList[0].b;
		c = (edgeList[1].a != a && edgeList[1].a != b) ? edgeList[1].a : edgeList[1].b;

		if(a == b || a == c || b == c)
		{
			throw new Exception("Problem assigning vectors to triangle");
		}

		computeCircum();
	}

	public void vectorsToEdges()
	{
		edgeList.Clear();
		edgeList.Add(EdgeMaintener.createEdge(a, b, this));
		edgeList.Add(EdgeMaintener.createEdge(a, c, this));
		edgeList.Add(EdgeMaintener.createEdge(b, c, this));

		computeCircum();
	}



	public void removeEdges()
	{
		for(int i = 0; i < edgeList.Count; i++)
		{
			EdgeMaintener.removeEdge(edgeList[i], this);
		}
	}


	
	// Todo : mrettre ca private et recalculer auto des que on change un set sur a, b ou c
	public void computeCircum()
	{

		// lines from a to b and a to c
		Vector2 AB = b - a;
		Vector2 AC = c - a;
		
		// perpendicular vector on triangle
		Vector3 N = Vector3.Normalize(Vector3.Cross(AB, AC));
		
		// find the points halfway on AB and AC
		Vector2 halfAB = a + AB*0.5f;
		Vector2 halfAC = a + AC*0.5f;
		
		// build vectors perpendicular to ab and ac
		Vector2 perpAB = Vector3.Cross(AB, N);
		Vector2 perpAC = Vector3.Cross(AC, N);
		
		// find intersection between the two lines
		// D: halfAB + t*perpAB
		// E: halfAC + s*perpAC
		circumCenter = Geometry.LineLineIntersection(halfAB, perpAB, halfAC, perpAC);
		circumRadius = Vector2.Distance(circumCenter, a);
	}
}
