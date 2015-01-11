using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using System.Xml;
using System.Xml.Serialization;

public class Main : MonoBehaviour
{
	Delaunay delaunay;
	Voronoi voronoi;

	List<Vector2> points = new List<Vector2>();
	List<Cell> cellList = new List<Cell>();
	
	
	
	// Use this for initialization
	void Start()
	{
		//EdgeCeilMaintener.createEdge(new Vector2(1, 2),new Vector2(2, 3));
		//EdgeCeilMaintener.createEdge(new Vector2(2, 3),new Vector2(1, 2));
		//return;
		points = CreateRandomPoint(485);
		delaunay = new Delaunay(20, 20, points);
		delaunay.Auto();

		voronoi = new Voronoi(delaunay);
		cellList=voronoi.createVoronoi();
		//MapGenerator m = new MapGenerator(cellList);
		
		foreach(Cell cell in cellList)
		{
			MeshCreator a = new MeshCreator(cell);
		}
	}

	void Init()
	{
		string fileName = "savepoints.txt";
		if (File.Exists(fileName))
			File.Delete(fileName);
		
		StreamWriter sr = File.CreateText(fileName);
		
		points = CreateRandomPoint(130);
		
		XmlSerializer xs = new XmlSerializer(typeof(List<Vector2>));
		using (StreamReader rd = new StreamReader("savepoints.xml"))
		{
			points = xs.Deserialize(rd) as List<Vector2>;
			
		}
		
		
		delaunay = new Delaunay(20, 20, points);
		
		
		
		
		
		/*using (StreamWriter wr = new StreamWriter("savepoints.xml"))
		{
			xs.Serialize(wr, points);
		}*/
		
		for (int i = 0; i < points.Count; i++)
		{
			sr.WriteLine("points.Add(new Vector2(" + points [i].x + "," + points [i].y + "));\r");
		}
		sr.Close();
	}
	
	// Update is called once per frame
	void Update()
	{
		// Needed for stepByStepAuto
		if (delaunay != null)
			delaunay.Update();
	}
	
	void Clear()
	{
		if (delaunay != null)
			delaunay.Clear();
		points = CreateRandomPoint(100);
		delaunay = new Delaunay(20, 20, points);
	}
	
	private List<Vector2> CreateRandomPoint(int polygonNumber)
	{
		List<Vector2> points = new List<Vector2>();
		for (int i = 0; i < polygonNumber; i++)
		{
			points.Add(new Vector2(UnityEngine.Random.Range(0.2f, 19.8f), UnityEngine.Random.Range(0.2f, 19.8f)));
		}
		
		return points;
	}
	
	void OnDrawGizmos()
	{
		// Needed for showing Gizmo
		if (delaunay != null)
			delaunay.OnDrawGizmos();
	}
	
	void OnGUI()
	{
		
		int spacing = 10;
		int buttonHeight = 40;
		int buttonWidth = 120;
		int indexX = 0;
		int indexY = 0;
		
		if (GUI.Button(new Rect((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "Auto"))
		{
			Init();
			delaunay.delaunayMode = Delaunay.DelaunayMode.auto;
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			delaunay.Auto();
			watch.Stop();
			double elapsedMS = watch.ElapsedMilliseconds;
			
			UnityEngine.Debug.Log(elapsedMS);
			
		}
		indexX++;
		if (GUI.Button(new Rect((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "StepByStepAuto"))
		{
			_Debug.allowDebug = false;
			Init();
			delaunay.delaunayMode = Delaunay.DelaunayMode.stepByStepAuto;
		}
		indexX++;
		if (GUI.Button(new Rect((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "StepByStep"))
		{
			_Debug.allowDebug = true;
			if (delaunay == null)
				Init();
			delaunay.delaunayMode = Delaunay.DelaunayMode.stepByStep;
			delaunay.StepByStepNext();
			//delaunay.debugTriangleIndex = -1;
		}
		indexX -= 2;
		indexY++;
		if (GUI.Button(new Rect((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "Clear"))
		{
			Clear();
		}
		indexX++;
		if (GUI.Button(new Rect((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "Show triangle"))
		{
			delaunay.delaunayMode = Delaunay.DelaunayMode.showTriangle;
			/*if(delaunay.debugTriangleIndex < delaunay.triangleList.Count-1)
			{
				delaunay.debugTriangleIndex++;
			}
			else
				delaunay.debugTriangleIndex = 0;*/
		}
		indexX++;
		if (GUI.Button(new Rect((buttonWidth + spacing) * indexX + spacing, (buttonHeight + spacing) * indexY + spacing, buttonWidth, buttonHeight), "View Voronoi"))
		{
			voronoi = new Voronoi(delaunay);
			voronoi.createVoronoi();

		}		
		
	}
}

