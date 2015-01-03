// CARON TONY

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics ;


struct EdgeToFlip{
	public Vector2 v;
	public Edge e;
}

public class Delaunay : MonoBehaviour {

	private int polygonNumber = 1000;
	List<Vector2> points = new List<Vector2>();
	int pointCursor = 0;

	List<Triangle> triangleList = new List<Triangle>();
	List<Triangle> triangleToAddList = new List<Triangle>();

	List<EdgeToFlip> edgeToFlipList = new List<EdgeToFlip>();

	List<Vector2> gizDebug = new List<Vector2>();

	int debugMode = 1;
	int debugTriangleIndex = 0;
	int debugIterate = 0;

	/*public void Delaunay(int width, int height, List<Vector2> points)
	{

	}*/

	// Use this for initialization
	void Start () {
		//points = CreateRandomPoint();
		// TODO Geer les point au meme endroit!

		points.Add(new Vector2(0,0));
		points.Add(new Vector2(0,20));
		points.Add(new Vector2(20,0));
		points.Add(new Vector2(20,20));
		pointCursor = 4;	
	

		points.AddRange(CreateRandomPoint());

		triangleList.Add(new Triangle(new Vector2(0,0), new Vector2(0,20), new Vector2(20,20)));
		triangleList.Add(new Triangle(new Vector2(0,0), new Vector2(20,0), new Vector2(20,20)));

	}

	void auto()
	{
		_Debug.allowDebug = false;

		Edge e;
		Triangle t;

		// TODO ne pas faire a l'envers
		for (int i = points.Count-1; i >= 0; i--) 
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

			for (int j = edgeToFlipList.Count-1; j >= 0; j--) 
			{
				flipEdge(edgeToFlipList[j].e, edgeToFlipList[j].v);
				edgeToFlipList.RemoveAt(j);
			}
		}



	}

	void step1()
	{
		
		//Debug.ClearDeveloperConsole();
		if(EdgeMaintener.edgeList.Count >=6)
		{
			_Debug.Log ("::We have "+EdgeMaintener.edgeList[5].triangleList.Count+" triangles");
		}
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
			//_Debug.Log("FLIPIPIPIP");


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

	public bool isLegalEdge(Triangle t1, Triangle t2)
	{
		bool ret = true;

		if(Geometry.isInCircum(t1.a, t2) || Geometry.isInCircum(t1.b, t2) || Geometry.isInCircum(t1.c, t2))
		   ret = false;

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
		if(!getSharedVector(adjTriangle1.edgeList[t1index1[1]], adjTriangle1.edgeList[t1index1[2]], out sharedTriangle1Vector))
		{
			throw new Exception("pas de edge shared (getSharedVector)");
		}
		
		// recupere le vecteur partagé des 2 segments elligible
		if(!getSharedVector(adjTriangle2.edgeList[t1index2[1]], adjTriangle2.edgeList[t1index2[2]], out sharedTriangle2Vector))
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
		if(getSharedVector(adjTriangle1.edgeList[t1index1[1]], adjTriangle2.edgeList[t1index2[2]], out sharedTriangle1Vector))
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
	public bool getSharedVector(Edge a, Edge b, out Vector2 shared){

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

	
	// Update is called once per frame
	void Update () {
		//if(pointCursor > 19) return;
		if(debugMode == 3)
		{		
			step1();
			debugTriangleIndex = -1;
		}

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
		if(!getSharedVector(adjTriangle1.edgeList[t1index1[1]], adjTriangle1.edgeList[t1index1[2]], out sharedTriangle1Vector))
		{
			throw new Exception("pas de edge shared (getSharedVector)");
		}
		
		// recupere le vecteur opposé à l'egde invalide du triangle2
		if(!getSharedVector(adjTriangle2.edgeList[t1index2[1]], adjTriangle2.edgeList[t1index2[2]], out sharedTriangle2Vector))
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
			else if(PointInTriangle(p, triangleList[i].a, triangleList[i].b, triangleList[i].c))
			{
				t = triangleList[i];
				return true;
			}
			else if(pointOnTriangle(p, triangleList[i], out e))
			{
				t = triangleList[i];
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
	public bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
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
	
	private List<Vector2> CreateRandomPoint() {
		// Use Vector2f, instead of Vector2
		// Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
		List<Vector2> points = new List<Vector2>();
		for (int i = 0; i < polygonNumber; i++) {
			points.Add(new Vector2(UnityEngine.Random.Range(0.2f,19.8f), UnityEngine.Random.Range(0.2f,19.8f)));
		}
		
		return points;
	}

	/*private List<Vector2> CreateRandomPoint() {
		List<Vector2> points = new List<Vector2>();
		//for (int i = 0; i < polygonNumber; i++) {
			for(float x = 0.2f; x < 19.8f; x+=0.9f)
			{
				for(float y = 0.2f; y < 19.8f; y+=0.9f)
				{
					points.Add(new Vector2(x,y));
				}
			}

		//}
		
		return points;
	}*/

	void OnDrawGizmos() {



		if(debugMode == 1 || debugMode == 3)
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
			
			/*for (int i = 0; i < gizDebug.Count; i+=2) {
			Gizmos.color = Color.white;
			int f = i+1;
			Gizmos.DrawLine(gizDebug[i], gizDebug[f]);

			_Debug.Log("Draw a line between "+gizDebug[i].x+":"+gizDebug[i].y+" and "+gizDebug[f].x+":"+gizDebug[f].y);
			}*/
		}

		else if(debugMode == 2)
		{
			for (int i = 0; i < triangleList.Count; i++) {
				Gizmos.color = Color.grey;
				/*Gizmos.DrawLine(triangleList[i].a, triangleList[i].b);
				Gizmos.DrawLine(triangleList[i].b, triangleList[i].c);
				Gizmos.DrawLine(triangleList[i].c, triangleList[i].a);*/

				Gizmos.DrawLine(triangleList[i].edgeList[0].a, triangleList[i].edgeList[0].b);
				Gizmos.DrawLine(triangleList[i].edgeList[1].a, triangleList[i].edgeList[1].b);
				Gizmos.DrawLine(triangleList[i].edgeList[2].a, triangleList[i].edgeList[2].b);
				
				Gizmos.color = Color.green;
				//Gizmos.DrawWireSphere(triangleList[i].circumCenter, triangleList[i].circumRadius);
			}

			Gizmos.color = triangleList[debugTriangleIndex].color;
			Gizmos.color = Color.green;
			/*Gizmos.DrawLine(triangleList[i].a, triangleList[i].b);
			Gizmos.DrawLine(triangleList[i].b, triangleList[i].c);
			Gizmos.DrawLine(triangleList[i].c, triangleList[i].a);*/
			
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
		else if(debugMode == 4)
		{
			//createVoronoi();
		}



		
	}

	void OnGUI () {
		
		
		if (GUI.Button (new Rect (0,10,150,100), "Step")) {
			debugMode = 1;
			step1();
			debugTriangleIndex = -1;
		}
		if (GUI.Button (new Rect (150,10,150,100), "Auto")) {
			debugMode = 3;
			step1();
			debugTriangleIndex = -1;
		}
		if (GUI.Button (new Rect (300,10,150,100), "Show triangle")) {
			debugMode = 2;
			if(debugTriangleIndex < triangleList.Count-1)
			{
				debugTriangleIndex++;
			}
			else
				debugTriangleIndex = 0;
		}

		if (GUI.Button (new Rect (0,110,150,100), "Voronoi")) {
			//createVoronoi();

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			auto ();
			watch.Stop();
			double elapsedMS = watch.ElapsedMilliseconds;

			UnityEngine.Debug.Log(elapsedMS);

			createVoronoi();
		}

		//_Debug.Log(debugTriangleIndex);
		

	}
}



