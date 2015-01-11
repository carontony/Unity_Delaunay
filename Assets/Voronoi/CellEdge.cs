using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CellEdge 
{
	public Vector2 a;
	public Vector2 b;

	public int uid;

	public Dictionary<Cell, Cell> cellList = new Dictionary<Cell,Cell>();

	public CellEdge(Vector2 _a, Vector2 _b)
	{
		a = _a; 
		b = _b;
	}

}

