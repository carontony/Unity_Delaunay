using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class TriangleIndex
{
	private static int index = 0;

	public static int get()
	{
		return TriangleIndex.index++;
	}

}
public class Triangle
{
	public Vector2 a;
	public Vector2 b;
	public Vector2 c;

	// TODO Remove; Only for debugging
	public int index;
	
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

		index = TriangleIndex.get();

		if (a == b || a == c || b == c)
		{
			throw new Exception("OMG");		
		}


		vectorsToEdges();


		color = new  Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
	}

	/*public void edgesToVectors()
	{

		// Dirty parade :)
		a = edgeList[1].a;
		b = edgeList[1].b;
		c = (edgeList[0].a != a && edgeList[0].a != b) ? edgeList[0].a : edgeList[0].b;

		if(a == b || a == c || b == c)
		{
			throw new Exception("Problem assigning vectors to triangle");
		}

		computeCircum();
	}*/

	private void vectorsToEdges()
	{
		edgeList.Clear();
		edgeList.Add(EdgeMaintener.createEdge(a, b, this));
		edgeList.Add(EdgeMaintener.createEdge(a, c, this));
		edgeList.Add(EdgeMaintener.createEdge(b, c, this));

		computeCircum();
	}

	public List<Edge> getAdjacentEdgesFromEdge(Edge e)
	{
		List<Edge> returnEdgeList = new List<Edge>();

		if (edgeList [0] == e)
		{
			returnEdgeList.Add(edgeList [1]);
			returnEdgeList.Add(edgeList [2]);
		} else if (edgeList [1] == e)
		{
			returnEdgeList.Add(edgeList [0]);
			returnEdgeList.Add(edgeList [2]);
		} else if (edgeList [2] == e)
		{
			returnEdgeList.Add(edgeList [0]);
			returnEdgeList.Add(edgeList [1]);
		} else
			throw new Exception("pas de edge shared (sortEdgesByShared)");
		
		return returnEdgeList;
	}

	// Recupère le vecteur opposé à un edge
	public Vector2 getOppositeVectorFromEdge(Edge e)
	{
		// Test if Edge e is a part of this triangle
		if (!((e.a == a || e.a == b || e.a == c) && (e.b == a || e.b == b || e.b == c)))
			throw new Exception("This edge is not a part of this triangle");

		if (a != e.a && a != e.b)
			return a;
		else if (b != e.a && b != e.b)
			return b;
		else 
			return c;
	}

	// Return if the Triangle is builded with vector V
	public bool isVectorOf(Vector2 v)
	{
		return (v == a || v == b || v == c);
	}



	public void removeEdges()
	{
		for (int i = 0; i < edgeList.Count; i++)
		{
			EdgeMaintener.removeEdge(edgeList [i], this);
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
		Vector2 halfAB = a + AB * 0.5f;
		Vector2 halfAC = a + AC * 0.5f;
		
		// build vectors perpendicular to ab and ac
		Vector2 perpAB = Vector3.Cross(AB, N);
		Vector2 perpAC = Vector3.Cross(AC, N);
		
		// find intersection between the two lines
		// D: halfAB + t*perpAB
		// E: halfAC + s*perpAC
		circumCenter = Geometry.LineLineIntersection(halfAB, perpAB, halfAC, perpAC);

		//circumCenter = GetCircumcenter(a,b,c);

		circumRadius = Vector2.Distance(circumCenter, b);

		if (!(Mathf.Approximately(Vector2.Distance(circumCenter, a), Vector2.Distance(circumCenter, b))))
		{
			UnityEngine.Debug.Log(Vector2.Distance(circumCenter, a).ToString());
			UnityEngine.Debug.Log(Vector2.Distance(circumCenter, b));
			//throw new Exception("fuck");
		}
	}

	public  Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
	{
		// m1 - center of (a,b), the normal goes through it
		float f1 = (b.x - a.x) / (a.y - b.y);
		Vector2 m1 = new Vector2((a.x + b.x) / 2, (a.y + b.y) / 2);
		float g1 = m1.y - f1 * m1.x;

		float f2 = (c.x - b.x) / (b.y - c.y);
		Vector2 m2 = new Vector2((b.x + c.x) / 2, (b.y + c.y) / 2);
		float g2 = m2.y - f2 * m2.x;

		// degenerated cases
		// - 3 points on a line
		if (f1 == f2)
		{
			return new Vector2(0, 0);
		}
		   // - a, b have the same height -> slope of normal of |ab| = infinity
		   else if (a.y == b.y)
			return new Vector2(m1.x, f2 * m1.x + g2);
		else if (b.y == c.y)
			return new Vector2(m2.x, f1 * m2.x + g1);

		float x = (g2 - g1) / (f1 - f2);
		return new Vector2(x, f1 * x + g1);
	}

	public  Vector2 Intersect(Vector2 line1V1, Vector2 line1V2, Vector2 line2V1, Vector2 line2V2)
	{
		//Line1
		float A1 = line1V2.y - line1V1.y;
		float B1 = line1V2.x - line1V1.x;
		float C1 = A1 * line1V1.x + B1 * line1V1.y;
		 
		//Line2
		float A2 = line2V2.y - line2V1.y;
		float B2 = line2V2.x - line2V1.x;
		float C2 = A2 * line2V1.x + B2 * line2V1.y;
		
		float det = A1 * B2 - A2 * B1;
		if (det == 0)
		{
			return new Vector2(0, 0);//parallel lines
		} else
		{
			float x = (B2 * C1 - B1 * C2) / det;
			float y = (A1 * C2 - A2 * C1) / det;
			return new Vector2(x, y);
		}
	}

	~Triangle()
	{
		this.removeEdges();
	}

}
