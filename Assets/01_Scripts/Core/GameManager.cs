using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(gameObject);
	}

	public async Task<string> CreateRelayGame()
	{
		SceneManager.LoadScene("Game");

		string relayCode = await RelayManager.Instance.CreateRelay();
		return relayCode;
	}

	public void JoinRelayGame(string code)
	{
		StartCoroutine(JoinRelayCo(code));
	}

	IEnumerator JoinRelayCo(string code)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync("Game");

		while (!operation.isDone)
			yield return null;

		RelayManager.Instance.JoinRelay(code);
	}
}
