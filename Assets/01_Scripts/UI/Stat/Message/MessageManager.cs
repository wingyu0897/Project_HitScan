using Define;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
	[SerializeField] private RectTransform _container;

	[Header("Prefabs")]
	[SerializeField] private Message _killMessage;

	[Header("Settings")]
	[SerializeField] private int _maxMessageCount = 5;
	[SerializeField] private float _messageSpace = 30f;

	[ContextMenu("Message")]
	public void AddMessage()
	{
		ShowMessage(MESSAGE_TYPE.Kill, "SSSS");
	}

    public void ShowMessage(MESSAGE_TYPE messageType, string message)
	{
		switch (messageType)
		{
			case MESSAGE_TYPE.Kill:
				{
					Message ui = Instantiate(_killMessage, _container);
					ui.TextMesh.SetText($"<b>{message}</b> KILLED");
					AddMessage(ui);
				}
				break;
		}
	}

	private void AddMessage(Message message)
	{
		if (transform.childCount >= _maxMessageCount)
		{
			while (transform.childCount >= _maxMessageCount)
			{
				Destroy(transform.GetChild(0).gameObject);
			}
		}
		_container.sizeDelta = new Vector2(0, _messageSpace * 2 * _container.childCount);
		message.transform.localPosition = Vector3.zero;
	}
}
