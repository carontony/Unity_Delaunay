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

	public static void LogOnScreen<T>(T s, Vector3 pos)
	{

		UnityEngine.Object debugPrefab = Resources.Load("Prefabs/debugtext");	
		GameObject debugText;
		GameObject debugContainer = GameObject.Find("Debug");

		debugText = (GameObject) MonoBehaviour.Instantiate (debugPrefab);
		debugText.transform.position = pos;
		debugText.name = s.ToString();
		//debugText.transform.parent = debugContainer.transform;
		
		TextMesh t = (TextMesh)debugText.GetComponent(typeof(TextMesh));
		t.text = s.ToString();


	}

}

