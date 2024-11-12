using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private ParallaxLayer[] layers;
	[SerializeField] private Transform cameraTrm;

	private void Awake()
	{
		layers = GetComponentsInChildren<ParallaxLayer>();
	}

	private void Update() 
	{
		foreach (ParallaxLayer layer in layers)
		{
			layer.Move(cameraTrm.position);
		}
	}
}
