// CARON TONY
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO
/*
Remove triangleList from Delaunay class
Add a new finding point method
Delete triangle not use
Rename point to "site"
Capitalize method :)
Add namespace to Delaunay class

*/

// Helped by priority:
// https://dl.dropboxusercontent.com/u/84854464/totologic/fully_dynamic_constrained_delaunay_triangulation.pdf
public class Delaunay
{
	List<Vector2> points = new List<Vector2>();
	int pointCursor = 0;
	public Dictionary<Triangle, Triangle> triangleList = new Dictionary<Triangle,Triangle>();
	public Dictionary<Triangle, Triangle> triangleDebugList = new Dictionary<Triangle,Triangle>();
	public Dictionary<Edge, Edge> edgeDebugList = new Dictionary<Edge,Edge>();
	
	List<Triangle> triangleToAddList = new List<Triangle>();
	List<EdgeToFlip> edgeToFlipList = new List<EdgeToFlip>();
	List<Vector2> gizDebug = new List<Vector2>();
	public int debugMode = 1;
	public Triangle debugTriangleIndex;
	public int debugIterate = 0;
	
	public enum DelaunayMode
	{
		stepByStep               = 1,
		stepByStepAuto   = 2,
		auto                    = 3,
		showTriangle    = 4
	}
	
	// Auto by default
	public DelaunayMode delaunayMode = DelaunayMode.stepByStep;
	
	public Delaunay(int width, int height, List<Vector2> _points)
	{
		EdgeMaintener.edgeList.Clear();
		
		points.Add(new Vector2(0, 0));
		points.Add(new Vector2(0, height));
		points.Add(new Vector2(width, 0));
		points.Add(new Vector2(width, height));
		pointCursor = 4;                       
		
		_points.Reverse();
		points.AddRange(_points);
		
		Triangle t1 = new Triangle(new Vector2(0, 0), new Vector2(0, 20), new Vector2(20, 20));
		Triangle t2 = new Triangle(new Vector2(0, 0), new Vector2(20, 0), new Vector2(20, 20));
		
		triangleList.Add(t1, t1);
		triangleList.Add(t2, t2);
	}
	
	public void Clear()
	{
		
	}
	
	public void Update()
	{
		if (delaunayMode == DelaunayMode.stepByStepAuto)
		{              
			StepByStepNext();
		}
	}
	
	public void Auto()
	{
		_Debug.allowDebug = false;
		
		Edge e;
		Triangle t;
		
		// TODO ne pas faire a l'envers
		for (int i = points.Count-1; i >= 4; i--)
		{
			// Try to find on witch triangle point is
			// TODO : Implement a non naive approch here
			if (!getTriangleFromPoint(points [i], out t, out e))
			{
				// Point not found or not valid => remove from list
				points.RemoveAt(i);
				continue;
			}
			
			// Create the new triangles
			if (e == null)
				insertPointIn(points [i], t);
			else
				insertPointOn(points [i], t, e);
			
			while (edgeToFlipList.Count > 0)
				for (int j = edgeToFlipList.Count-1; j >= 0; j--)
			{
				flipEdge(edgeToFlipList [j].edgeToFlip, edgeToFlipList [j].pointInserted);
				edgeToFlipList.RemoveAt(j);
			}
			/*{
                                EdgeToFlip ef = edgeToFlipList[edgeToFlipList.Count-1];
                                edgeToFlipList.RemoveAt(edgeToFlipList.Count-1);
                                flipEdge(ef.e, ef.v);
                        }*/
			/*for (int j = edgeToFlipList.Count-1; j >= 0; j--)
                        {
                                flipEdge(edgeToFlipList[j].e, edgeToFlipList[j].v);
                                edgeToFlipList.RemoveAt(j);
                        }*/
		}
		
		
		
	}
	
	public void StepByStepNext()
	{
		gizDebug.Clear();
		
		if (edgeToFlipList.Count > 0)
		{
			for (int i = edgeToFlipList.Count-1; i >= 0; i--)
			{
				flipEdge(edgeToFlipList [i].edgeToFlip, edgeToFlipList [i].pointInserted);
				edgeToFlipList.RemoveAt(i);
				debugShowNext();
				return;
			}
		}
		
		
		if (pointCursor != points.Count)
		{
			Vector2 nextPoint = points [pointCursor];
			Edge e;
			Triangle t;
			
			if (!getTriangleFromPoint(nextPoint, out t, out e))
			{
				points.RemoveAt(pointCursor);
				return;
			}
			
			if (e == null)
				insertPointIn(nextPoint, t);
			else
				insertPointOn(nextPoint, t, e);
			
			pointCursor++;
			
			
		}
		
		debugShowNext();
	}
	
	public void debugShowNext()
	{
		_Debug.Log("find " + edgeToFlipList.Count + " edge(s) to flip");
		
		if (edgeToFlipList.Count > 0)
		{
			for (int i = edgeToFlipList.Count-1; i >= 0; i--)
			{
				_Debug.Log("We will Try to flip this edge");
				gizDebug.Add(edgeToFlipList [i].edgeToFlip.a);
				gizDebug.Add(edgeToFlipList [i].edgeToFlip.b);
				
				return;
			}              
			
		}
	}
	
	
	public bool isLegalEdge(Triangle t1, Vector2 a)
	{
		return !Geometry.isInCircum(a, t1);
	}
	
	public void flipEdge(Edge edgeToFlip, Vector2 pointInserted)
	{
		Vector2 oppositeVectorT1, oppositeVectorT2;
		
		_Debug.Log(++debugIterate);
		
		if (edgeToFlip.triangleList.Count > 2)
			throw new Exception("This edge has more than 2 triangles");
		
		
		if (edgeToFlip.triangleList.Count < 2)
		{
			_Debug.Log("Edge Alone, nothing to do");
			return; // nothing to do, no adjacent triangle
		}
		
		// Recupere les 2 triangles de l'edges
		Triangle adjTriangle1 = edgeToFlip.triangleList.First().Value;
		Triangle adjTriangle2 = edgeToFlip.triangleList.Last().Value;
		
		// Recupère le vecteur opposé à l'edge pour le triangle 1
		oppositeVectorT1 = adjTriangle1.getOppositeVectorFromEdge(edgeToFlip);
		
		// Recupère le vecteur opposé à l'edge pour le triangle 2
		oppositeVectorT2 = adjTriangle2.getOppositeVectorFromEdge(edgeToFlip);
		
		
		// Test if the edge is Delaunay against the opposite point
		if (adjTriangle1.isVectorOf(pointInserted))
		{
			if (isLegalEdge(adjTriangle1, oppositeVectorT2))
			{
				_Debug.Log("Edge is valid1");
				return; // Shared edge is legal
			}
		} else if (adjTriangle2.isVectorOf(pointInserted))
		{
			if (isLegalEdge(adjTriangle2, oppositeVectorT1))
			{
				_Debug.Log("Edge is valid2");
				return; // Shared edge is legal
			}
		} else
			throw new Exception("No inserted point in the two triangles");
		
		
		_Debug.Log("Edge is invalid, going to flip");
		
		// Create the two new triangles
		Triangle t1 = new Triangle(edgeToFlip.a, oppositeVectorT1, oppositeVectorT2);
		Triangle t2 = new Triangle(edgeToFlip.b, oppositeVectorT1, oppositeVectorT2);
		
		triangleList.Add(t1, t1);
		triangleList.Add(t2, t2);
		
		List<Edge> adjEdge;
		adjEdge = adjTriangle1.isVectorOf(pointInserted) ? adjTriangle2.getAdjacentEdgesFromEdge(edgeToFlip) : adjTriangle1.getAdjacentEdgesFromEdge(edgeToFlip);
		
		foreach (Edge e in adjEdge)
		{
			edgeToFlipList.Add(new EdgeToFlip(e, pointInserted));
		}
		
		adjTriangle1.removeEdges();
		triangleList.Remove(adjTriangle1);
		
		adjTriangle2.removeEdges();
		triangleList.Remove(adjTriangle2);
	}
	
	
	
	public Edge getTriangleEdgeFromVectors(Triangle t, Vector2 a, Vector2 b)
	{
		Edge e = null;
		
		for (int i = 0; i < t.edgeList.Count; i++)
		{
			if ((t.edgeList [i].a == a && t.edgeList [i].b == b) || (t.edgeList [i].a == b && t.edgeList [i].b == a))
			{
				e = t.edgeList [i];
				break;
			}              
		}
		
		if (e == null)
		{
			throw new Exception("getTriangleEdgeFromVectors not found");
		}
		
		return e;
	}
	
	
	
	
	
	
	private void insertPointIn(Vector2 p, Triangle t)
	{
		
		Triangle t1 = new Triangle(p, t.a, t.c);
		Triangle t2 = new Triangle(p, t.c, t.b);
		Triangle t3 = new Triangle(p, t.a, t.b);
		
		triangleList.Add(t1, t1);
		triangleList.Add(t2, t2);
		triangleList.Add(t3, t3);
		
		edgeToFlipList.Add(new EdgeToFlip(t.edgeList [0], p));
		edgeToFlipList.Add(new EdgeToFlip(t.edgeList [1], p));
		edgeToFlipList.Add(new EdgeToFlip(t.edgeList [2], p));
		
		
		
		t.removeEdges();
		triangleList.Remove(t);
		
		for (int f = 0; f < EdgeMaintener.edgeList.Count; f++)
		{
			if (EdgeMaintener.edgeList [f].triangleList.Count > 2)
				throw new Exception("ARGGGGGGGGGGGGx");
		}
		
	}
	
	// In this case, we need to create 2 triangles in the 2 triangles shared by the edge
	private void insertPointOn(Vector2 p, Triangle t, Edge e)
	{
		Vector2 oppositeVectorT1, oppositeVectorT2;
		
		Triangle adjTriangle1 = e.triangleList.First().Value;
		Triangle adjTriangle2 = e.triangleList.Last().Value;
		
		// recupere le vecteur opposé à l'egde invalide du triangle1
		oppositeVectorT1 = adjTriangle1.getOppositeVectorFromEdge(e);
		oppositeVectorT2 = adjTriangle2.getOppositeVectorFromEdge(e);
		
		
		Triangle t1 = new Triangle(p, e.a, oppositeVectorT1);
		Triangle t2 = new Triangle(p, e.a, oppositeVectorT2);
		Triangle t3 = new Triangle(p, e.b, oppositeVectorT1);
		Triangle t4 = new Triangle(p, e.b, oppositeVectorT2);
		
		triangleList.Add(t1, t1);
		triangleList.Add(t2, t2);
		triangleList.Add(t3, t3);
		triangleList.Add(t4, t4);
		
		List<Edge> adjEdge;
		adjEdge = adjTriangle1.getAdjacentEdgesFromEdge(e);
		
		foreach (Edge ed in adjEdge)
		{
			edgeToFlipList.Add(new EdgeToFlip(ed, p));
		}
		
		adjEdge = adjTriangle2.getAdjacentEdgesFromEdge(e);
		
		foreach (Edge ed in adjEdge)
		{
			edgeToFlipList.Add(new EdgeToFlip(ed, p));
		}
		
		adjTriangle1.removeEdges();
		triangleList.Remove(adjTriangle1);
		
		adjTriangle2.removeEdges();
		triangleList.Remove(adjTriangle2);
		
		for (int f = 0; f < EdgeMaintener.edgeList.Count; f++)
		{
			if (EdgeMaintener.edgeList [f].triangleList.Count > 2)
				throw new Exception("ARGGGGGGGGGGGG");
		}
	}
	
	private bool getTriangleFromPoint(Vector2 p, out Triangle t, out Edge e)
	{
		e = null;
		t = null;
		
		foreach (Triangle tr in triangleList.Values)
		{
			if (p == tr.a || p == tr.b || p == tr.c)
			{
				// Point deja existant
				return false;
			} else if (Geometry.PointInTriangle(p, tr.a, tr.b, tr.c))
			{
				t = tr;
				return true;
			} else if (pointOnTriangle(p, tr, out e))
			{
				t = tr;
				return true; // TODO !!!!! remettre a true
			}
			
		}
		throw new Exception("Point pas trouvé dans un triangle");
	}
	
	// Home made :) sure we can do better
	public bool pointOnTriangle(Vector2 p, Triangle t, out Edge e)
	{
		Vector2 direction, newPoint;
		float distance;
		
		e = null;
		
		direction = (t.b - t.a).normalized;
		distance = (p - t.a).magnitude;
		newPoint = t.a + (direction * distance);
		
		if (newPoint == p && distance <= (t.b - t.a).magnitude)
		{
			e = getTriangleEdgeFromVectors(t, t.a, t.b);
			return true;
		}
		
		direction = (t.c - t.b).normalized;
		distance = (p - t.b).magnitude;
		newPoint = t.b + (direction * distance);
		
		if (newPoint == p && distance <= (t.c - t.b).magnitude)
		{
			e = getTriangleEdgeFromVectors(t, t.b, t.c);
			return true;
		}
		
		direction = (t.c - t.a).normalized;
		distance = (p - t.a).magnitude;
		newPoint = t.a + (direction * distance);
		
		if (newPoint == p && distance <= (t.c - t.a).magnitude)
		{
			e = getTriangleEdgeFromVectors(t, t.a, t.c);
			return true;
		}
		
		return false;
	}
	
	public void toto(Edge e, float? _angle)//,Poly a)
	{
		if (edgeDebugList.ContainsKey(e))
			return;
		edgeDebugList.Add(e, e);
		
		Triangle adjTriangle1 = e.triangleList.Count > 0 ? e.triangleList.ElementAt(0).Value : null;
		
		List<Edge> edgeList = adjTriangle1.getAdjacentEdgesFromEdge(e);
		
		// Nous avons 2 candidat
		for (int i = 0; i < edgeList.Count; i++)
		{
			
		}
		
		
		Triangle adjTriangle2 = e.triangleList.Count > 1 ? e.triangleList.ElementAt(1).Value : null;
		
		
		
		float angle = Mathf.DeltaAngle(Mathf.Atan2(adjTriangle1.circumCenter.y, adjTriangle1.circumCenter.x) * Mathf.Rad2Deg,
		                               Mathf.Atan2(adjTriangle2.circumCenter.y, adjTriangle2.circumCenter.x) * Mathf.Rad2Deg);
		
		if (_angle == null || angle <= _angle)
		{
			Debug.DrawLine(adjTriangle1.circumCenter, adjTriangle2.circumCenter, Color.green, 10);
		}
		
		
		// Test des 3 edges
		for (int i = 0; i < adjTriangle2.edgeList.Count; i++)
		{
			toto(adjTriangle2.edgeList [i], angle);
		}
	}
	
	public void triangleTraversal(Vector2 p, Triangle firstTriangle, Triangle t, Edge previousEdge, ref Poly poly)
	{
		//List
		Edge e = t.edgeList [0].ContainVector(p) && t.edgeList [0] != previousEdge ? t.edgeList [0] : (t.edgeList [1].ContainVector(p) && t.edgeList [1] != previousEdge ? t.edgeList [1] : t.edgeList [2]);
		
		Triangle adjTriangle1 = e.triangleList.ElementAt(0).Value;
		Triangle adjTriangle2 = e.triangleList.ElementAt(1).Value;
		
		Triangle nextTriangle = adjTriangle1 == t ? adjTriangle2 : adjTriangle1;
		
		Debug.DrawLine(t.circumCenter, nextTriangle.circumCenter, Color.green, 20);
		poly.point.Add(nextTriangle.circumCenter);
		
		if (nextTriangle != firstTriangle)
		{
			triangleTraversal(p, firstTriangle, nextTriangle, e, ref poly);
		}
		// End
		else
		{
			
		}
	}
	
	public Poly createRegion(Vector2 site, Triangle firstTriangle, Triangle t)
	{
		Poly poly = new Poly();
		poly.center = site;
		
		triangleTraversal(site, firstTriangle, t, null, ref poly);
		
		return poly;
	}
	
	public List<Poly> createVoronoi()
	{
		List<Poly> region = new List<Poly>();
		
		for (int i = 4; i<points.Count; i++)
		{
			// TODO Refactor
			// Find one Triange that contain the point
			foreach (Triangle t in triangleList.Values)
			{
				if (t.a == points [i] || t.b == points [i] || t.c == points [i])
				{
					region.Add(createRegion(points [i], t, t));
				}
			}
		}
		
		return region;
	}
	
	
	public void OnDrawGizmos()
	{
		if (delaunayMode == DelaunayMode.auto || delaunayMode == DelaunayMode.stepByStep || delaunayMode == DelaunayMode.stepByStepAuto)
		{
			Vector2 oppositeVectorT1, oppositeVectorT2;
			
			for (int i = 0; i<EdgeMaintener.edgeList.Count; i++)
			{
				Edge e = EdgeMaintener.edgeList [i];
				
				Triangle adjTriangle1 = e.triangleList.Count > 0 ? e.triangleList.ElementAt(0).Value : null;
				Triangle adjTriangle2 = e.triangleList.Count > 1 ? e.triangleList.ElementAt(1).Value : null;
				
				if (adjTriangle1 != null)
				{
					// Don't redraw a triangle
					if (!triangleDebugList.ContainsKey(adjTriangle1))
					{
						triangleDebugList.Add(adjTriangle1, adjTriangle1);
						
						Gizmos.color = Color.grey;//adjTriangle1.color;
						Gizmos.DrawLine(adjTriangle1.edgeList [0].a, adjTriangle1.edgeList [0].b);
						Gizmos.DrawLine(adjTriangle1.edgeList [1].a, adjTriangle1.edgeList [1].b);
						Gizmos.DrawLine(adjTriangle1.edgeList [2].a, adjTriangle1.edgeList [2].b);
					}
					
					if (adjTriangle2 != null)
					{
						oppositeVectorT2 = adjTriangle2.getOppositeVectorFromEdge(e);
						
						// Show a red sphere if the edge is invalid
						if (Geometry.isInCircum(oppositeVectorT2, adjTriangle1))
						{
							Gizmos.color = Color.red;
							Gizmos.DrawWireSphere(adjTriangle1.circumCenter, adjTriangle1.circumRadius);
							Gizmos.DrawSphere(oppositeVectorT2, 0.5f);
							
							Gizmos.DrawSphere(e.a, 0.2f);
							Gizmos.DrawSphere(e.b, 0.2f);
						}
					}
				}
				
				if (adjTriangle2 != null)
				{
					// Don't redraw a triangle
					if (!triangleDebugList.ContainsKey(adjTriangle2))
					{
						triangleDebugList.Add(adjTriangle2, adjTriangle2);
						
						Gizmos.color = Color.grey;//adjTriangle2.color;
						Gizmos.DrawLine(adjTriangle2.edgeList [0].a, adjTriangle2.edgeList [0].b);
						Gizmos.DrawLine(adjTriangle2.edgeList [1].a, adjTriangle2.edgeList [1].b);
						Gizmos.DrawLine(adjTriangle2.edgeList [2].a, adjTriangle2.edgeList [2].b);
					}
					
					if (adjTriangle1 != null) // Can't be null
					{
						oppositeVectorT1 = adjTriangle1.getOppositeVectorFromEdge(e);
						
						// Show a red sphere if the edge is invalid
						if (Geometry.isInCircum(oppositeVectorT1, adjTriangle2))
						{
							Gizmos.color = Color.red;
							Gizmos.DrawWireSphere(adjTriangle2.circumCenter, adjTriangle2.circumRadius);
							Gizmos.DrawSphere(oppositeVectorT1, 0.5f);
							
							Gizmos.DrawSphere(e.a, 0.2f);
							Gizmos.DrawSphere(e.b, 0.2f);
						}
					}
				}
			}
			if (pointCursor > 0)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(points [pointCursor - 1], 0.2f);
			}
			
			if (gizDebug.Count > 0)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(gizDebug [0], gizDebug [1]);
			}
			
			triangleDebugList.Clear();
			
			
		} else if (delaunayMode == DelaunayMode.showTriangle)
		{
			foreach (Triangle t in triangleList.Values)
			{
				Gizmos.color = Color.grey;
				
				Gizmos.DrawLine(t.edgeList [0].a, t.edgeList [0].b);
				Gizmos.DrawLine(t.edgeList [1].a, t.edgeList [1].b);
				Gizmos.DrawLine(t.edgeList [2].a, t.edgeList [2].b);
				
				Gizmos.color = Color.green;
				//Gizmos.DrawWireSphere(triangleList[i].circumCenter, triangleList[i].circumRadius);
			}
			
			/*Gizmos.color = triangleList[debugTriangleIndex].color;
                        Gizmos.color = Color.green;
                               
                        Gizmos.DrawLine(triangleList[debugTriangleIndex].edgeList[0].a, triangleList[debugTriangleIndex].edgeList[0].b);
                        Gizmos.DrawLine(triangleList[debugTriangleIndex].edgeList[1].a, triangleList[debugTriangleIndex].edgeList[1].b);
                        Gizmos.DrawLine(triangleList[debugTriangleIndex].edgeList[2].a, triangleList[debugTriangleIndex].edgeList[2].b);

                        for(int j = 0; j < pointCursor; j++)
                        {
                                if(Geometry.isInCircum(points[j], triangleList[debugTriangleIndex]))
                                {
                                        Gizmos.color = Color.red;
                                        Gizmos.DrawWireSphere(triangleList[debugTriangleIndex].circumCenter, triangleList[debugTriangleIndex].circumRadius);
                                        Gizmos.DrawSphere(points[j], 0.4f);

                                        Gizmos.color = Color.white;
                                        Gizmos.DrawSphere(triangleList[debugTriangleIndex].edgeList[0].a, 0.8f);
                                        Gizmos.DrawSphere(triangleList[debugTriangleIndex].edgeList[0].b, 0.8f);
                                        Gizmos.DrawSphere(triangleList[debugTriangleIndex].edgeList[1].a, 0.8f);

                                        _Debug.Log("test du point "+points[j]+", radius: "+triangleList[debugTriangleIndex].circumRadius+" center: "+triangleList[debugTriangleIndex].circumCenter);
                                        _Debug.Log("Distance: "+((points[j] - (Vector2)triangleList[debugTriangleIndex].circumCenter).magnitude));
                                       
                                }
                        }

                        Gizmos.DrawWireSphere(triangleList[debugTriangleIndex].circumCenter, triangleList[debugTriangleIndex].circumRadius);*/
			
		}
		
		
		
		
	}
	
	
}