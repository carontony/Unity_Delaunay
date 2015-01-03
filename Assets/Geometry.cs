using UnityEngine;
using System.Collections;

public static class Geometry
{
	public static bool isInCircum(Vector2 p, Triangle t)
	{
		return PointInCircle(p, t.circumCenter, t.circumRadius);
	}
	
	
	// TODO manque de précision
	public static bool PointInCircle(Vector2 p, Vector2 center, float radius)
	{
		radius = Mathf.Round(radius * 1000f) / 1000f;
		float distance = (p - center).magnitude;
		distance = Mathf.Round(distance * 1000f) / 1000f;

		return distance < radius;
		//return ( (p.x - center.x)^2 + (p.y - center.y)^2 < radius^2 );
		//float 	D = Mathf.Sqrt((float)Math.Pow(center.x - p.x, 2) + Mathf.Pow(center.y - p.y, 2));
		//return D < radius;
		
		
		
	}
	public static Vector3 LineLineIntersection(Vector3 originD, Vector3 directionD, Vector3 originE, Vector3 directionE) {
		directionD.Normalize();
		directionE.Normalize();
		var N = Vector3.Cross(directionD, directionE);
		var SR = originD - originE;
		var absX = Mathf.Abs(N.x);
		var absY = Mathf.Abs(N.y);
		var absZ = Mathf.Abs(N.z);
		float t;
		if (absZ > absX && absZ > absY) {
			t = (SR.x*directionE.y - SR.y*directionE.x)/N.z;
		} else if (absX > absY) {
			t = (SR.y*directionE.z - SR.z*directionE.y)/N.x;
		} else {
			t = (SR.z*directionE.x - SR.x*directionE.z)/N.y;
		}
		return originD - t*directionD;
	}
}
