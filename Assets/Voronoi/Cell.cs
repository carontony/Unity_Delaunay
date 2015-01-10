using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Cell 
{
	public Dictionary<CellEdge,CellEdge> cellEdgeList = new Dictionary<CellEdge,CellEdge>();

	public Cell()
	{
	}

	public void addEdge(Vector2 a, Vector2 b)
	{
		CellEdge ce = CellEdgeMaintener.createEdge(a, b, this);
		cellEdgeList.Add(ce, ce);
	}

}

