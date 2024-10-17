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
	/// 플레이어 사망
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
	/// 클라이언트에서 킬 로그 UI를 생성
	/// </summary>
	[ClientRpc(RequireOwnership = false)]
	private void KillLogClientRpc(string attackName, string deadName, Color attackColor, Color deadColor)
	{
		if (!_view.gameObject.activeInHierarchy) return; // 비활성화되어 있다면 코루틴을 실행할 수 없으니 반환

		KillLogPanelUI ui = Instantiate(_killLogUIPrefab, _uiContainer); // 풀링으로 변경 필요
		ui.Init(attackName, attackColor, deadName, deadColor, 3f);
	}
}
