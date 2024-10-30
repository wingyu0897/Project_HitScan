
public class SettingUIViewManager : UIViewManager
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
