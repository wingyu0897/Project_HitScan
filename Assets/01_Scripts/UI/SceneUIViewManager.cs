using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneUIViewManager : UIViewManager
{
	protected override void Start()
	{
		DontDestroyOnLoad(gameObject);
	}
}
