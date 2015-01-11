using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

