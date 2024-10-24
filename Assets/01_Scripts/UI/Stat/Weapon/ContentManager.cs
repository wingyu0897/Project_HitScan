using UnityEngine;

public class ContentManager
{
	public void CreatePanel(Transform container, UIPanel prefab, IPanelData panelData)
	{
		UIPanel panel = GameObject.Instantiate(prefab, container);
		panel.BindData(panelData);
	}
}
