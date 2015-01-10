using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public static class CellEdgeMaintener
{
	public static EdgeComparer edgeComparer = new EdgeComparer();
	public static Dictionary<Vector4, CellEdge> cellEdgeList = new Dictionary<Vector4,CellEdge>(edgeComparer);

	public static CellEdge createEdge(Vector2 a, Vector2 b, Cell c)
	{
		Vector4 edgeV4 = new Vector4(a.x, a.y, b.x, b.y);

		Debug.Log("Try to insert "+a.ToString()+" and "+b.ToString());

		// This edge doesn't exist : Add it
		if(!cellEdgeList.ContainsKey(edgeV4))
		{
			CellEdge ce = new CellEdge(a, b);
			ce.cellList.Add(c,c);
			cellEdgeList.Add(edgeV4, ce); 
			return ce;
		}
		// Already existe : return it
		else
		{
			// Add new ceil in the edge
			if(!cellEdgeList[edgeV4].cellList.ContainsKey(c))
				cellEdgeList[edgeV4].cellList.Add(c,c);

			return cellEdgeList[edgeV4];	
		}
	}
}












