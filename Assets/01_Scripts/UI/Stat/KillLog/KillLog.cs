using Unity.Netcode;
using UnityEngine;
using Define;

public class KillLog : NetworkBehaviour
{
    [SerializeField] private KillLogPanelUI _killLogUIPrefab;
    [SerializeField] private RectTransform _uiContainer;

	private GamePlayView _view;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		Debug.LogWarning("KillLogUI Prefab Pooling");

		_view = UIManager.UIViewManager.GetView<GamePlayView>();

		if (IsHost)
			PlayerAgent.OnPlayerDie += HandlePlayerDie;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (IsHost)
			PlayerAgent.OnPlayerDie -= HandlePlayerDie;
	}

	/// <summary>
	/// �÷��̾� ���
	/// </summary>
	private void HandlePlayerDie(object sender, PlayerAgent.PlayerEventArgs args)
	{
		string attack = GameManager.Instance.NetworkServer.GetUserDataByClientID(args.Player.Health.LastHitClientId)?.UserName ?? "";
		string dead = GameManager.Instance.NetworkServer.GetUserDataByClientID(args.ClientID).UserName;
		Color attackColor = Utility.GetColorByTeam(GameManager.Instance.GetTeam(args.Player.Health.LastHitClientId));
		Color deadColor = Utility.GetColorByTeam(GameManager.Instance.GetTeam(args.ClientID));

		KillLogClientRpc(attack, dead, attackColor, deadColor);
	}

	/// <summary>
	/// Ŭ���̾�Ʈ���� ų �α� UI�� ����
	/// </summary>
	[ClientRpc(RequireOwnership = false)]
	private void KillLogClientRpc(string attackName, string deadName, Color attackColor, Color deadColor)
	{
		if (!_view.gameObject.activeInHierarchy) return; // ��Ȱ��ȭ�Ǿ� �ִٸ� �ڷ�ƾ�� ������ �� ������ ��ȯ

		KillLogPanelUI ui = Instantiate(_killLogUIPrefab, _uiContainer); // Ǯ������ ���� �ʿ�
		ui.Init(attackName, attackColor, deadName, deadColor, 3f);
	}
}
