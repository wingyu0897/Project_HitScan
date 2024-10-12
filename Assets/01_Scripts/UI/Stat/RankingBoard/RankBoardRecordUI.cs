using TMPro;
using UnityEngine;

public class RankBoardRecordUI : MonoBehaviour
{
    public ulong ClientId;
    public string UserName;
    public int Rank;
    public int Kills;
    public Define.TEAM_TYPE Team;

    [SerializeField] private TextMeshProUGUI _userNameTxt;
    [SerializeField] private TextMeshProUGUI _rankTxt;
    [SerializeField] private TextMeshProUGUI _KillsTxt;

    public void SetOwner(ulong clientId)
	{
        ClientId = clientId;
	}

    public void SetName(string userName)
	{
        UserName = userName;
        _userNameTxt.text = UserName;
	}

    public void UpdateValue(int rank, int kills)
	{
        Rank = rank;
        Kills = kills;

        _rankTxt.text = Rank.ToString();
        _KillsTxt.text = Kills.ToString();
    }
}
