using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EdgeComparer : IEqualityComparer<Vector4>
{
	public bool Equals(Vector4 a, Vector4 b)
	{
		if(a == b)
			return true;
		
		Vector2 a1 = new Vector2(a.x, a.y);
		Vector2 a2 = new Vector2(a.z, a.w);
		Vector2 b1 = new Vector2(b.x, b.y);
		Vector2 b2 = new Vector2(b.z, b.w);
		
		return (a1 == b2 && a2 == b1);
		
		//return  ((a1 == b1 && a2 == b2) || (a1 == b2 && a2 == b1));		
		
	}
	
	// Always sort vector to have an unique key not depending to a or b order
	public int GetHashCode(Vector4 v)
	{
		Vector2 a = new Vector2(v.x, v.y);
		Vector2 b = new Vector2(v.z, v.w);
		
		if( a.x < b.x && a.y < b.y)
			return new Vector4(a.x, a.y, b.x, b.y).GetHashCode();
		
		if( a.x == b.x && a.y < b.y)
			return new Vector4(a.x, a.y, b.x, b.y).GetHashCode();
		
		if( a.x < b.x && a.y == b.y)
			return new Vector4(a.x, a.y, b.x, b.y).GetHashCode();
		
		return new Vector4(b.x, b.y, a.x, a.y).GetHashCode();
		
	}
}

