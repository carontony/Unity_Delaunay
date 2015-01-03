using UnityEngine;
using System;

public static class _Debug 
{
	public static bool allowDebug = true;

	public static void Log<T>(T s)
	{ 
		if (Debug.isDebugBuild && allowDebug) 
			Debug.Log(s);
	}

}

