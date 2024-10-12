using UnityEngine;

namespace Define
{
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
				TEAM_TYPE.Red => Color.red,
				TEAM_TYPE.Blue => Color.blue,
				_ => Color.black,
			};
		}
	}
}
