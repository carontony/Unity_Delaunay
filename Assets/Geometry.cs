using UnityEngine;
using System.Collections;

public static class Geometry
{
	public static bool isInCircum(Vector2 p, Triangle t)
	{
		return PointInCircle(p, t.circumCenter, t.circumRadius);
		//return !isPointInCircle(t.circumCenter.x, t.circumCenter.y, t.circumRadius, p.x, p.y);
		//return ContainsPoint(t.circumCenter, t.circumRadius, p);
		//return InCircle(p, t.a, t.b, t.c);
	}

	private static bool InCircle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
	{ 
		//Return TRUE if the point (xp,yp) lies inside the circumcircle 
		//made up by points (x1,y1) (x2,y2) (x3,y3) 
		//NOTE: A point on the edge is inside the circumcircle 

		if (System.Math.Abs(p1.y - p2.y) < float.Epsilon && System.Math.Abs(p2.y - p3.y) < float.Epsilon)
		{ 
			//INCIRCUM - F - Points are coincident !! 
			return false; 
		} 
		
		float m1, m2; 
		float mx1, mx2; 
		float my1, my2; 
		float xc, yc; 
		
		if (System.Math.Abs(p2.y - p1.y) < float.Epsilon)
		{ 
			m2 = -(p3.x - p2.x) / (p3.y - p2.y); 
			mx2 = (p2.x + p3.x) * 0.5f; 
			my2 = (p2.y + p3.y) * 0.5f; 
			//Calculate CircumCircle center (xc,yc) 
			xc = (p2.x + p1.x) * 0.5f; 
			yc = m2 * (xc - mx2) + my2; 
		} else if (System.Math.Abs(p3.y - p2.y) < float.Epsilon)
		{ 
			m1 = -(p2.x - p1.x) / (p2.y - p1.y); 
			mx1 = (p1.x + p2.x) * 0.5f; 
			my1 = (p1.y + p2.y) * 0.5f; 
			//Calculate CircumCircle center (xc,yc) 
			xc = (p3.x + p2.x) * 0.5f; 
			yc = m1 * (xc - mx1) + my1; 
		} else
		{ 
			m1 = -(p2.x - p1.x) / (p2.y - p1.y); 
			m2 = -(p3.x - p2.x) / (p3.y - p2.y); 
			mx1 = (p1.x + p2.x) * 0.5f; 
			mx2 = (p2.x + p3.x) * 0.5f; 
			my1 = (p1.y + p2.y) * 0.5f; 
			my2 = (p2.y + p3.y) * 0.5f; 
			//Calculate CircumCircle center (xc,yc) 
			xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2); 
			yc = m1 * (xc - mx1) + my1; 
		} 
		
		float dx = p2.x - xc; 
		float dy = p2.y - yc; 
		float rsqr = dx * dx + dy * dy; 
		//float r = Math.Sqrt(rsqr); //Circumcircle radius 
		dx = p.x - xc; 
		dy = p.y - yc; 
		float drsqr = dx * dx + dy * dy; 
		
		return (drsqr < rsqr); 
	} 

	public static bool ContainsPoint(Vector2 centre, float radius, Vector2 toTest)
	{

		float dX = Mathf.Abs(toTest.x - centre.x);
		float dY = Mathf.Abs(toTest.y - centre.y);
		
		float sumOfSquares = dX * dX + dY * dY;
		
		float distance = Mathf.Sqrt(sumOfSquares);
		
		return (radius < distance);
	}

	
	
	// TODO manque de précision
	//  doit etre strictement IN
	public static bool PointInCircle(Vector2 p, Vector2 center, float radius)
	{


		//float.Epsilon
		//radius = Mathf.Round(radius * 1000f) / 1000f;
		float distance = (p - center).magnitude;
		//distance = Mathf.Round(distance * 1000f) / 1000f;

		return distance < (radius - Mathf.Epsilon);

		//if(Mathf.Approximately(distance, radius)) return false;

		//return (distance < radius-Mathf.Epsilon && distance < radius + Mathf.Epsilon);
	}


	public static bool PointInPoly()
	{
		return true;
	}

	public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
	{
		var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
		var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;
		
		if ((s < 0) != (t < 0))
			return false;
		
		var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
		if (A < 0.0)
		{
			s = -s;
			t = -t;
			A = -A;
		}
		return s > 0 && t > 0 && (s + t) < A;
	}

	public static bool PointInTrianglev2(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
	{
		// Compute vectors        
		Vector2 v0 = c - a;
		Vector2 v1 = b - a;
		Vector2 v2 = p - a;
			
		// Compute dot products
		float dot00 = Vector2.Dot(v0, v0);
		float dot01 = Vector2.Dot(v0, v1);
		float dot02 = Vector2.Dot(v0, v2);
		float dot11 = Vector2.Dot(v1, v1);
		float dot12 = Vector2.Dot(v1, v2);
			
		// Compute barycentric coordinates
		float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
		float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
		float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
			
		// Check if point is in triangle
		return (u >= 0) && (v >= 0) && (u + v < 1);
	}

	public static Vector3 LineLineIntersection(Vector3 originD, Vector3 directionD, Vector3 originE, Vector3 directionE)
	{
		directionD.Normalize();
		directionE.Normalize();
		Vector3 N = Vector3.Cross(directionD, directionE);
		Vector3 SR = originD - originE;
		float absX = Mathf.Abs(N.x);
		float absY = Mathf.Abs(N.y);
		float absZ = Mathf.Abs(N.z);
		float t;
		if (absZ > absX && absZ > absY)
		{
			t = (SR.x * directionE.y - SR.y * directionE.x) / N.z;
		} else if (absX > absY)
		{
			t = (SR.y * directionE.z - SR.z * directionE.y) / N.x;
		} else
		{
			t = (SR.z * directionE.x - SR.x * directionE.z) / N.y;
		}
		return originD - t * directionD;
	}
}
