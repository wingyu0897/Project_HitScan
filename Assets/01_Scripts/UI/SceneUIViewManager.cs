
public class SceneUIViewManager : UIViewManager
{
	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(gameObject);
	}

	protected override void Start()
	{
	}
}
