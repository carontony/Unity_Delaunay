﻿// CARON TONY

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics ;


struct EdgeToFlip{
	public Vector2 v;
	public Edge e;
}

// Helped by priority:
// https://dl.dropboxusercontent.com/u/84854464/totologic/fully_dynamic_constrained_delaunay_triangulation.pdf
public class Delaunay {


	List<Vector2> points = new List<Vector2>();
	int pointCursor = 0;

	public List<Triangle> triangleList = new List<Triangle>();
	List<Triangle> triangleToAddList = new List<Triangle>();

	List<EdgeToFlip> edgeToFlipList = new List<EdgeToFlip>();

	List<Vector2> gizDebug = new List<Vector2>();

	public int debugMode = 1;
	public int debugTriangleIndex = 0;
	public int debugIterate = 0;

	public enum DelaunayMode {
		stepByStep 		= 1,
		stepByStepAuto 	= 2,
		auto			= 3,
		showTriangle	= 4
	}
	
	// Auto by default
	public DelaunayMode delaunayMode = DelaunayMode.stepByStep;

	public Delaunay(int width, int height, List<Vector2> _points)
	{
		EdgeMaintener.edgeList.Clear();

		points.Add(new Vector2(0,0));
		points.Add(new Vector2(0,height));
		points.Add(new Vector2(width,0));
		points.Add(new Vector2(width,height));
		pointCursor = 4;			

		_points.Reverse();
		points.AddRange(_points);

	
		/*Triangle t1 = new Triangle(new Vector2(12.02694f, 3.062051f), new Vector2(7.014543f, 2.657841f), new Vector2(8.496894f, 0.2177948f));
		t1.circumRadius = 2.536501f;
		t1.circumCenter.x = 9.547636f;
		t1.circumCenter.y = 2.526427f;

		Triangle t2 = new Triangle(new Vector2(12.02694f, 3.062051f), new Vector2(7.014543f, 2.657841f), new Vector2(8.197275f, 14.52549f));
		t2.circumRadius = 6.142125f;
		t2.circumCenter.x = 9.070292f;
		t2.circumCenter.y = 8.445724f;


		triangleList.Add(t1);
		  triangleList.Add(t2);

		isLegalEdge(triangleList[1], triangleList[0]);*/
		
		triangleList.Add(new Triangle(new Vector2(0,0), new Vector2(0,20), new Vector2(20,20)));
		triangleList.Add(new Triangle(new Vector2(0,0), new Vector2(20,0), new Vector2(20,20)));
	}


	public void Clear()
	{

	}
	public void Update()
	{
		if(delaunayMode == DelaunayMode.stepByStepAuto)
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
			if(!getTriangleFromPoint(points[i], out t, out e))
			{
				// Point not found or not valid => remove from list
				points.RemoveAt(i);
				continue;
			}

			// Create the new triangles
			if(e == null)
				insertPointIn(points[i], t);
			else
				insertPointOn(points[i], t, e);


			for (int j = triangleList.Count-1; j >= 0; j--) 
			{				
				if(triangleList[j].toDelete == true)
				{
					triangleList[j].removeEdges();
					triangleList.RemoveAt(j);
				}
			}		

			while(edgeToFlipList.Count > 0)
			for (int j = edgeToFlipList.Count-1; j >= 0; j--) 
			{
				flipEdge(edgeToFlipList[j].e, edgeToFlipList[j].v);
				edgeToFlipList.RemoveAt(j);
			}
		}



	}

	public void StepByStepNext()
	{
		gizDebug.Clear();

		if( edgeToFlipList.Count > 0 )
		{
			for (int i = edgeToFlipList.Count-1; i >= 0; i--) 
			{
				flipEdge(edgeToFlipList[i].e, edgeToFlipList[i].v);
				edgeToFlipList.RemoveAt(i);
				debugShowNext();
				return;
			}
		}

		
		if(pointCursor != points.Count)
		{
			Vector2 nextPoint = points[pointCursor];
			Edge e;
			Triangle t;
			
			if(!getTriangleFromPoint(nextPoint, out t, out e))
			{
				points.RemoveAt(pointCursor);
				return;
			}

			if(e == null)
				insertPointIn(nextPoint, t);
			else
				insertPointOn(nextPoint, t, e);
			
			pointCursor++;

			for (int i = triangleList.Count-1; i >= 0; i--) {
		
				if(triangleList[i].toDelete == true)
				{
					_Debug.Log ("Remove triangle "+i);
				
					triangleList[i].removeEdges();
					triangleList.RemoveAt(i);
				}
			}			
		}

		debugShowNext();
		

		/*for (int i = edgeToFlipList.Count-1; i >= 0; i--) 
		{
			flipEdge(edgeToFlipList[i]);
		}*/
		

	}

	public void debugShowNext()
	{
		_Debug.Log("find "+edgeToFlipList.Count+" edge(s) to flip");
		
		if( edgeToFlipList.Count > 0 )
		{
			for (int i = edgeToFlipList.Count-1; i >= 0; i--) 
			{
				_Debug.Log("We will Try to flip this edge");
				gizDebug.Add(edgeToFlipList[i].e.a);
				gizDebug.Add(edgeToFlipList[i].e.b);
				
				return;
			}		
			
		}
	}

	// TODO ne pas tester les 3 points mais uniquement l'opposé de l'edge potentiellment invalid
	public bool isLegalEdge(Triangle t1, Triangle t2)
	{
		bool ret = true;
		bool ret2 = true; // TODO avirer : juste pour verifier l'algo

		// Test si un des points du triangle t1 est dans le circum de t2
		if(Geometry.isInCircum(t1.a, t2) || Geometry.isInCircum(t1.b, t2) || Geometry.isInCircum(t1.c, t2))
		   ret = false;

		// TODO avirer : juste pour verifier l'algo
		if(Geometry.isInCircum(t2.a, t1) || Geometry.isInCircum(t2.b, t1) || Geometry.isInCircum(t2.c, t1))
		{
			ret2 = false;
		}
		// TODO avirer : juste pour verifier l'algo
		if(ret != ret2)
		{
			_Debug.Log ("a:"+t1.a+" b:"+t1.b+" c:"+t1.c);
			_Debug.Log ("a:"+t2.a+" b:"+t2.b+" c:"+t2.c);
			//throw new Exception("Problème de flotant surement");
		}

		return ret;
	}

	public void flipEdge(Edge edgeToFlip, Vector2 pointInserted)
	{
		debugIterate++;
		_Debug.Log(debugIterate);

		Vector2 sharedTriangle1Vector, sharedTriangle2Vector;

		_Debug.Log("This edge has "+edgeToFlip.triangleList.Count+" triangles");

		if(edgeToFlip.triangleList.Count > 2)
		{
			throw new Exception("This edge has more than 2 triangles");
		}

		if(edgeToFlip.triangleList.Count < 2)
		{
			_Debug.Log("Edge Alone, nothing to do");
			return; // nothing to do, no adjacent triangle
		}
	
		Triangle adjTriangle1 = edgeToFlip.triangleList[0];
		Triangle adjTriangle2 = edgeToFlip.triangleList[1];


		// Test if Delaunay
		if(isLegalEdge(adjTriangle2, adjTriangle1)){
			_Debug.Log("Edge is valid");
		return; // Shared edge is legal
		}

		_Debug.Log("Edge is invalid, going to flip");

		// Shared always at index 0 // TODO dirty
		List<int> t1index1 = sortEdgesByShared(adjTriangle1, edgeToFlip);
		List<int> t1index2 = sortEdgesByShared(adjTriangle2, edgeToFlip);	
		
		// Remove the invalid edge from the 2 triangles
		EdgeMaintener.removeEdge(edgeToFlip, adjTriangle1);
		EdgeMaintener.removeEdge(edgeToFlip, adjTriangle2);		

		//Triangle triangleContainPointInserted = isVectorTriangle(pointInserted, adjTriangle1) ? adjTriangle1 : adjTriangle2;
		if(!isVectorOfTriangle(pointInserted, adjTriangle1))
		{
			EdgeToFlip a = new EdgeToFlip();
			a.v = pointInserted;
			
			a.e = adjTriangle1.edgeList[t1index1[1]];
			edgeToFlipList.Add(a);
			
			a.e = adjTriangle1.edgeList[t1index1[2]];
			edgeToFlipList.Add(a);
		}
		else if(!isVectorOfTriangle(pointInserted, adjTriangle2))
		{
			EdgeToFlip a = new EdgeToFlip();
			a.v = pointInserted;
			
			a.e = adjTriangle2.edgeList[t1index2[1]];
			edgeToFlipList.Add(a);
			
			a.e = adjTriangle2.edgeList[t1index2[2]];
			edgeToFlipList.Add(a);
		}
		else
		{
			throw new Exception("oups");
		}



		
		// recupere le vecteur partagé des 2 segments elligible
		if(!getSharedVectorByEdge(adjTriangle1.edgeList[t1index1[1]], adjTriangle1.edgeList[t1index1[2]], out sharedTriangle1Vector))
		{
			throw new Exception("pas de edge shared (getSharedVector)");
		}
		
		// recupere le vecteur partagé des 2 segments elligible
		if(!getSharedVectorByEdge(adjTriangle2.edgeList[t1index2[1]], adjTriangle2.edgeList[t1index2[2]], out sharedTriangle2Vector))
		{
			throw new Exception("pas de edge shared (getSharedVector)");
		}

		// FlipEdge : Remplace l'edge invalide par le valide
		adjTriangle1.edgeList[t1index1[0]] =  EdgeMaintener.createEdge(sharedTriangle1Vector, sharedTriangle2Vector, adjTriangle1);
		adjTriangle2.edgeList[t1index2[0]] =  EdgeMaintener.createEdge(sharedTriangle1Vector, sharedTriangle2Vector, adjTriangle2);

		if(adjTriangle1.edgeList[t1index1[0]] != adjTriangle2.edgeList[t1index2[0]])
		{
			throw new Exception("AIE");
		}


		
		// TODO peut etre ne pas faire joujou avec les triangles existants mais plutot tout effacer et tout reconstruire
		if(getSharedVectorByEdge(adjTriangle1.edgeList[t1index1[1]], adjTriangle2.edgeList[t1index2[2]], out sharedTriangle1Vector))
		{
			EdgeMaintener.removeEdge(adjTriangle1.edgeList[t1index1[2]], adjTriangle1);
			EdgeMaintener.removeEdge(adjTriangle2.edgeList[t1index2[2]], adjTriangle2);

			Edge nextEdgeToFlip1 = adjTriangle1.edgeList[t1index1[1]];
			Edge nextEdgeToFlip2 = adjTriangle1.edgeList[t1index1[2]];	

			Edge saveT1_2 = adjTriangle1.edgeList[t1index1[2]];
			//adjTriangle1.edgeList[t1index1[2]] = adjTriangle2.edgeList[t1index2[2]];
			//adjTriangle2.edgeList[t1index2[2]] = saveT1_2;

			adjTriangle1.edgeList[t1index1[2]] = EdgeMaintener.createEdge(adjTriangle2.edgeList[t1index2[2]].a, adjTriangle2.edgeList[t1index2[2]].b, adjTriangle1);
			adjTriangle2.edgeList[t1index2[2]] = EdgeMaintener.createEdge(saveT1_2.a, saveT1_2.b, adjTriangle2);
			
			adjTriangle1.edgesToVectors();
			adjTriangle2.edgesToVectors();

		}
		else
		{
			EdgeMaintener.removeEdge(adjTriangle1.edgeList[t1index1[2]], adjTriangle1);
			EdgeMaintener.removeEdge(adjTriangle2.edgeList[t1index2[1]], adjTriangle2);

			Edge saveT1_2 = adjTriangle1.edgeList[t1index1[2]];
			//adjTriangle1.edgeList[t1index1[2]] = adjTriangle2.edgeList[t1index2[1]];
			//adjTriangle2.edgeList[t1index2[1]] = saveT1_2;

			adjTriangle1.edgeList[t1index1[2]] = EdgeMaintener.createEdge(adjTriangle2.edgeList[t1index2[1]].a, adjTriangle2.edgeList[t1index2[1]].b, adjTriangle1);
			adjTriangle2.edgeList[t1index2[1]] = EdgeMaintener.createEdge(saveT1_2.a, saveT1_2.b, adjTriangle2);;

			adjTriangle1.edgesToVectors();
			adjTriangle2.edgesToVectors();
		}

	}

	// Return if the Triangle is builded with vector V
	public bool isVectorOfTriangle(Vector2 v, Triangle t)
	{
			return (v == t.a || v == t.b || v == t.c);
	}

	public bool getTriangleEdgeFromVectors(Triangle t, Vector2 a, Vector2 b, out Edge e)
	{
		e = null;
		for(int i = 0; i < t.edgeList.Count; i++)
		{
			if((t.edgeList[i].a == a && t.edgeList[i].b == b) || (t.edgeList[i].a == b && t.edgeList[i].b == a))
			{
				e = t.edgeList[i];
				return true;
			}		
		}
		return false;
	}

	// recupere le vecteur partagé des 2 segments elligible
	public bool getSharedVectorByEdge(Edge a, Edge b, out Vector2 shared){

		bool ret = true;
		shared = new Vector2(0,0);

		if(a.a == b.a)
			shared =  a.a;
		else if(a.a == b.b)
			shared = a.a;
		else if(a.b == b.a)
			shared =  a.b;
		else if(a.b == b.b)
			shared = a.b;
		else
			ret = false;

		return ret;
	}

	// Illegal Always first
	public List<int> sortEdgesByShared(Triangle t, Edge e)
	{		
		List<int> indexList = new List<int>();
		if(t.edgeList[0] == e)
		{
			indexList.Add(0);
			indexList.Add(1);
			indexList.Add(2);
		}
		else if(t.edgeList[1] == e)
		{
			indexList.Add(1);
			indexList.Add(0);
			indexList.Add(2);
		}
		else if(t.edgeList[2] == e)
		{
			indexList.Add(2);
			indexList.Add(1);
			indexList.Add(0);
		}
		else
			throw new Exception("pas de edge shared (sortEdgesByShared)");
		
		return indexList;
	}

	// Illegal Always first
	public List<Edge> sortIllegalEdge(Triangle t, Edge e)
	{
		List<Edge> edgeList = new List<Edge>();

		if(t.edgeAb == e)
		{
			edgeList.Add(t.edgeAb);
			edgeList.Add(t.edgeCa);
			edgeList.Add(t.edgeBc);
		}
		else if(t.edgeCa == e)
		{
			edgeList.Add(t.edgeCa);
			edgeList.Add(t.edgeAb);
			edgeList.Add(t.edgeBc);
		}
		else if(t.edgeBc == e)
		{
			edgeList.Add(t.edgeBc);
			edgeList.Add(t.edgeAb);
			edgeList.Add(t.edgeCa);
		}

		return edgeList;
	}


	void showInvalid()
	{
	}

	


	private void insertPointIn(Vector2 p, Triangle t)
	{
		t.toDelete = true;

		triangleList.Add(new Triangle(p, t.a, t.c));
		triangleList.Add(new Triangle(p, t.c, t.b));
		triangleList.Add(new Triangle(p, t.a, t.b));

		EdgeToFlip a = new EdgeToFlip();
		a.v = p;
		
		a.e = t.edgeList[0];
		edgeToFlipList.Add(a);
		
		a.e = t.edgeList[1];
		edgeToFlipList.Add(a);
		
		a.e = t.edgeList[2];
		edgeToFlipList.Add(a);

	}

	// In this case, we need to create 2 triangles in the 2 triangles shared by the edge
	private void insertPointOn(Vector2 p, Triangle t, Edge e)
	{
		Vector2 sharedTriangle1Vector, sharedTriangle2Vector;

		Triangle adjTriangle1 = e.triangleList[0];
		Triangle adjTriangle2 = e.triangleList[1];

		// Need to be deleted
		adjTriangle1.toDelete = true;
		adjTriangle2.toDelete = true;

		// Shared always at index 0 // TODO dirty
		List<int> t1index1 = sortEdgesByShared(adjTriangle1, e);
		List<int> t1index2 = sortEdgesByShared(adjTriangle2, e);	

		// recupere le vecteur opposé à l'egde invalide du triangle1
		if(!getSharedVectorByEdge(adjTriangle1.edgeList[t1index1[1]], adjTriangle1.edgeList[t1index1[2]], out sharedTriangle1Vector))
		{
			throw new Exception("pas de edge shared (getSharedVector)");
		}
		
		// recupere le vecteur opposé à l'egde invalide du triangle2
		if(!getSharedVectorByEdge(adjTriangle2.edgeList[t1index2[1]], adjTriangle2.edgeList[t1index2[2]], out sharedTriangle2Vector))
		{
			throw new Exception("pas de edge shared (getSharedVector)");
		}
		
		triangleList.Add(new Triangle(p, adjTriangle1.edgeList[t1index1[0]].a, sharedTriangle1Vector));
		triangleList.Add(new Triangle(p, adjTriangle1.edgeList[t1index1[0]].a, sharedTriangle2Vector));
		triangleList.Add(new Triangle(p, adjTriangle1.edgeList[t1index1[0]].b, sharedTriangle1Vector));
		triangleList.Add(new Triangle(p, adjTriangle1.edgeList[t1index1[0]].b, sharedTriangle2Vector));

		EdgeToFlip a = new EdgeToFlip();
		a.v = p;
		
		a.e = adjTriangle1.edgeList[t1index1[1]];
		edgeToFlipList.Add(a);

		a.e = adjTriangle1.edgeList[t1index1[2]];
		edgeToFlipList.Add(a);

		a.e = adjTriangle2.edgeList[t1index2[1]];
		edgeToFlipList.Add(a);

		a.e = adjTriangle2.edgeList[t1index2[2]];
		edgeToFlipList.Add(a);
	}




	private bool getTriangleFromPoint(Vector2 p, out Triangle t, out Edge e)
	{
		e = null;
		t = null;
		for (int i = 0; i < triangleList.Count; i++) {

			if(p == triangleList[i].a || p == triangleList[i].b || p == triangleList[i].c)
			{
				// Point deja existant
				return false;
			}
			else if(Geometry.PointInTriangle(p, triangleList[i].a, triangleList[i].b, triangleList[i].c))
			{
				t = triangleList[i];
				return true;
			}
			else if(pointOnTriangle(p, triangleList[i], out e))
			{
				t = triangleList[i];
				return false; // TODO !!!!! remettre a true
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

		direction 	= ( t.b - t.a ).normalized;
		distance 	= ( p - t.a ).magnitude;
		newPoint 	= t.a + (direction * distance);

		if(newPoint == p && distance <= ( t.b - t.a ).magnitude )
		{
			if(!getTriangleEdgeFromVectors(t, t.a, t.b, out e))
			{
				throw new Exception("getTriangleEdgeFromVectors not found");
			}
			return true;
		}

		direction 	= ( t.c - t.b ).normalized;
		distance 	= ( p - t.b ).magnitude;
		newPoint 	= t.b + (direction * distance);

		if(newPoint == p && distance <= ( t.c - t.b ).magnitude )
		{
			if(!getTriangleEdgeFromVectors(t, t.b, t.c, out e))
			{
				throw new Exception("getTriangleEdgeFromVectors not found");
			}
			return true;
		}

		direction 	= ( t.c - t.a ).normalized;
		distance 	= ( p - t.a ).magnitude;
		newPoint 	= t.a + (direction * distance);
		
		if(newPoint == p && distance <= ( t.c - t.a ).magnitude )
		{
			if(!getTriangleEdgeFromVectors(t, t.a, t.c, out e))
			{
				throw new Exception("getTriangleEdgeFromVectors not found");
			}
			return true;
		}

		return false;
	}


	public void createVoronoi()
	{
		for(int i = 0; i < triangleList.Count; i++)
		{
			_createVoronoi(triangleList[i]);
			return;
		}
	}

	public void _createVoronoi(Triangle t)
	{
		for(int i = 0; i < triangleList.Count; i++)
		{
			for(int j = 0; j < triangleList[i].edgeList.Count; j++)
			{
				for(int k = 0; k < triangleList[i].edgeList[j].triangleList.Count; k++)
				{
					UnityEngine.Debug.DrawLine(triangleList[i].circumCenter,triangleList[i].edgeList[j].triangleList[k].circumCenter, Color.yellow, 4);
				}
			}
		}
	}

	/*public void __createVoronoi(Triangle t)
	{
		for(int i = 0; i < t.edgeList.Count; i++)
		{
			for(int j = 0; j < t.edgeList[i].triangleList.Count; j++)
			{
				if(t.edgeList[i].triangleList[j] != t)
				{
					//Gizmos.color = Color.yellow;
					//Gizmos.DrawLine(t.circumCenter, t.edgeList[i].triangleList[j].circumCenter);
					Drawing.DrawLine(t.circumCenter, t.edgeList[i].triangleList[j].circumCenter, Color.yellow, 4);

					_createVoronoi(t.edgeList[i].triangleList[j]);
				}
			}
		}
	}*/
	

	public void OnDrawGizmos() {



		if(delaunayMode == DelaunayMode.auto || delaunayMode == DelaunayMode.stepByStep || delaunayMode == DelaunayMode.stepByStepAuto)
		{
			if(triangleList.Count > 0)
			{
				for (int i = 0; i < triangleList.Count; i++) {
					Gizmos.color = triangleList[i].color;
					
					Gizmos.DrawLine(triangleList[i].edgeList[0].a, triangleList[i].edgeList[0].b);
					Gizmos.DrawLine(triangleList[i].edgeList[1].a, triangleList[i].edgeList[1].b);
					Gizmos.DrawLine(triangleList[i].edgeList[2].a, triangleList[i].edgeList[2].b);
					
					Gizmos.color = Color.green;
					//Gizmos.DrawWireSphere(triangleList[i].circumCenter, triangleList[i].circumRadius);

					for(int j = 0; j < pointCursor; j++)
					{
						if(Geometry.isInCircum(points[j], triangleList[i]))
						{
							Gizmos.color = Color.red;
							Gizmos.DrawWireSphere(triangleList[i].circumCenter, triangleList[i].circumRadius);

						}
					}
				}
			}
			
			if(pointCursor > 0)
			{
				Gizmos.color = Color.blue;
				for (int i = 0; i < pointCursor; i++) {
					Gizmos.DrawSphere(points[i], 0.2f);
				}
			}
			
			if(gizDebug.Count > 0)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(gizDebug[0], gizDebug[1]);
			}

		}

		else if(delaunayMode == DelaunayMode.showTriangle)
		{
			for (int i = 0; i < triangleList.Count; i++) {
				Gizmos.color = Color.grey;

				Gizmos.DrawLine(triangleList[i].edgeList[0].a, triangleList[i].edgeList[0].b);
				Gizmos.DrawLine(triangleList[i].edgeList[1].a, triangleList[i].edgeList[1].b);
				Gizmos.DrawLine(triangleList[i].edgeList[2].a, triangleList[i].edgeList[2].b);
				
				Gizmos.color = Color.green;
				//Gizmos.DrawWireSphere(triangleList[i].circumCenter, triangleList[i].circumRadius);
			}

			Gizmos.color = triangleList[debugTriangleIndex].color;
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

			Gizmos.DrawWireSphere(triangleList[debugTriangleIndex].circumCenter, triangleList[debugTriangleIndex].circumRadius);

		}



		
	}


}



