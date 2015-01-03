using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
	Delaunay myDelaunay;
	List<Vector2> points = new List<Vector2>();


	// Use this for initialization
	void Start ()
	{

	}

	void Init()
	{
		points = CreateRandomPoint(100);
		myDelaunay = new Delaunay(20, 20, points);
	}

	// Update is called once per frame
	void Update ()
	{
		// Needed for stepByStepAuto
		if(myDelaunay != null)
			myDelaunay.Update();
	}

	void Clear ()
	{
		myDelaunay.Clear();
		points = CreateRandomPoint(100);
		myDelaunay = new Delaunay(20, 20, points);
	}

	private List<Vector2> CreateRandomPoint(int polygonNumber) {
		List<Vector2> points = new List<Vector2>();
		for (int i = 0; i < polygonNumber; i++) {
			points.Add(new Vector2(UnityEngine.Random.Range(0.2f,19.8f), UnityEngine.Random.Range(0.2f,19.8f)));
		}
		
		return points;
	}

	void OnDrawGizmos(){
		// Needed for showing Gizmo
		if(myDelaunay != null)
			myDelaunay.OnDrawGizmos();
	}

	void OnGUI () {

		int spacing 		= 10;
		int buttonHeight 	= 40;
		int buttonWidth 	= 120;
		int indexX 			= 0;
		int indexY			= 0;

		if (GUI.Button (new Rect ((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "Auto")) {
			Init ();
			myDelaunay.delaunayMode = Delaunay.DelaunayMode.auto;
			myDelaunay.Auto();
		}
		indexX++;
		if (GUI.Button (new Rect ((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "StepByStepAuto")) {
			_Debug.allowDebug = false;
			Init ();
			myDelaunay.delaunayMode = Delaunay.DelaunayMode.stepByStepAuto;
		}
		indexX++;
		if (GUI.Button (new Rect ((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "StepByStep")) {
			_Debug.allowDebug = true;
			myDelaunay.delaunayMode = Delaunay.DelaunayMode.stepByStep;
			myDelaunay.StepByStepNext();
			myDelaunay.debugTriangleIndex = -1;
		}
		indexX-=2;
		indexY++;
		if (GUI.Button (new Rect ((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "Clear")) {
			Clear();
		}
		indexX++;
		if (GUI.Button (new Rect ((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "Show triangle")) {
			myDelaunay.delaunayMode = Delaunay.DelaunayMode.showTriangle;
			if(myDelaunay.debugTriangleIndex < myDelaunay.triangleList.Count-1)
			{
				myDelaunay.debugTriangleIndex++;
			}
			else
				myDelaunay.debugTriangleIndex = 0;
		}

		/*if (GUI.Button (new Rect (300,10,150,100), "Show triangle")) {
			myDelaunay.debugMode = 2;
			if(myDelaunay.debugTriangleIndex < myDelaunay.triangleList.Count-1)
			{
				myDelaunay.debugTriangleIndex++;
			}
			else
				myDelaunay.debugTriangleIndex = 0;
		}
		
		if (GUI.Button (new Rect (0,110,150,100), "Voronoi")) {
			//createVoronoi();
			
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			myDelaunay.auto ();
			watch.Stop();
			double elapsedMS = watch.ElapsedMilliseconds;
			
			UnityEngine.Debug.Log(elapsedMS);
			
			myDelaunay.createVoronoi();
		}*/
		
		//_Debug.Log(debugTriangleIndex);
		
		
	}
}

