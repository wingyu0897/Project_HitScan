using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float move;

    public void Move(Vector3 camPos)
	{
        camPos.z = 0;
        transform.position = camPos * (1f - move);
	}
}
