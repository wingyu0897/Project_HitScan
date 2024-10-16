
public class SceneUIViewManager : UIViewManager
{
	protected override void Start()
	{
		DontDestroyOnLoad(gameObject);
	}
}
