using UnityEngine;

namespace Define
{
	public enum MESSAGE_TYPE
	{
		Kill,
	}

	public enum TEAM_TYPE
	{
		Red = 0,
		Blue,
	}

	public static class Utility
	{

		public static Color GetColorByTeam(TEAM_TYPE team)
		{
			return team switch
			{
				TEAM_TYPE.Red => new Color32(238, 0, 2, 255),
				TEAM_TYPE.Blue => new Color32(0, 102, 255, 255),
				_ => Color.black,
			};
		}
	}
}
